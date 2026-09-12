namespace CampanhasAPI.Messaging;

/// <summary>Seção "RabbitMQ" do appsettings. Em Kubernetes vem de ConfigMap + Secret.</summary>
public class RabbitMQOptions
{
    public const string Secao = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}
