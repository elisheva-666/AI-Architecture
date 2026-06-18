using KafkaConsumer.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<AuctionConsumerWorker>();

var host = builder.Build();
host.Run();
