namespace ChineseAuction.Api.Services
{
    public interface IKafkaProducer
    {
        Task PublishAsync<T>(string topic, string key, T message);
    }
}
