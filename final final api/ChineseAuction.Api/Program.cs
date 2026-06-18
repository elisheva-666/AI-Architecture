using ChineseAuction.Api.Data;
using ChineseAuction.Api.Mappings;
using ChineseAuction.Api.Middleware;
using ChineseAuction.Api.Repositories;
using ChineseAuction.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =======================
// serilog
// =======================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// =======================
// Controllers
// =======================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// =======================
// Swagger + JWT
// =======================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ChineseAuction API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =======================
// Redis Cache
// =======================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "ChineseAuction:";
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// =======================
// DbContext
// =======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found")
    ));

// =======================
// DI – Repositories & Services
// =======================
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IDonorRepository, DonorRepository>();
builder.Services.AddScoped<IDonorService, DonorService>();

builder.Services.AddScoped<IGiftRepository, GiftRepository>();
builder.Services.AddScoped<IGiftService, GiftService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ILotteryRepository, LotteryRepository>();
builder.Services.AddScoped<ILotteryService, LotteryService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddAutoMapper(typeof(DonorProfile));

builder.Services.AddHttpClient<IAiService, OpenAiService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// =======================
// JWT Authentication
// =======================
var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing");

var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// =======================
// Rate Limiting — Sliding Window (built-in ASP.NET Core)
// =======================
builder.Services.AddRateLimiter(options =>
{
    var requestLimit   = builder.Configuration.GetValue<int>("RateLimiting:RequestLimit", 100);
    var windowMinutes  = builder.Configuration.GetValue<int>("RateLimiting:TimeWindowMinutes", 1);

    // GlobalLimiter מיושם על כל בקשה — מחולק לפי IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit          = requestLimit,
                Window               = TimeSpan.FromMinutes(windowMinutes),
                SegmentsPerWindow    = 6,   // 6 מקטעים של 10 שניות כל אחד
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0    // לא מחכים בתור — דוחים מיד
            }));

    options.RejectionStatusCode = 429;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.Append("Retry-After",
            TimeSpan.FromMinutes(windowMinutes).TotalSeconds.ToString());

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            message    = "Rate limit exceeded. Please try again later.",
            statusCode = 429
        }, token);
    };
});

// =======================
// Cors
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("Allowspecificorigin", policy =>
      policy.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());   // חובה כדי שהbrowser ישלח/יקבל cookies cross-origin
});


// =======================
// Build App
// =======================
var app = builder.Build();

// =======================
// Middleware pipeline
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

//äèéôåì áùâéàåú
app.UseExceptionHandling();

// øéùåí á÷ùåú
app.UseRequestLogging();

// Sliding Window rate limiter (built-in ASP.NET Core)
app.UseRateLimiter();

app.UseCors("Allowspecificorigin");

// מחלץ את ה-JWT מה-cookie ומוסיף אותו כ-Bearer header
// כך ה-JWT middleware הסטנדרטי של ASP.NET יוכל לאמת אותו
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["auth_token"];
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Authorization = $"Bearer {token}";
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();

