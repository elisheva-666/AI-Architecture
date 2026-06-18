//using ChineseAuction.Api.Repositories;
//using Microsoft.AspNetCore.Http;
//using System.Threading.Tasks;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;


//namespace ChineseAuction.Api.Middleware
//{
//    public class AttachPurchaserMiddleware
//    {

//        private readonly RequestDelegate _next;

//        public AttachPurchaserMiddleware(RequestDelegate next) => _next = next;

//        public async Task InvokeAsync(HttpContext context, IPurchaserRepository repo)
//        {
//            if (context.User?.Identity?.IsAuthenticated == true)
//            {
//                var sub = context.User.FindFirst(System.Security.Claims.JwtRegisteredClaimNames.Sub)?.Value;
//                if (int.TryParse(sub, out var id))
//                {
//                    var purchaser = await repo.GetByIdAsync(id);
//                    if (purchaser != null) context.Items["Purchaser"] = purchaser;
//                }
//            }

//            await _next(context);
//        }
//    }
//}
