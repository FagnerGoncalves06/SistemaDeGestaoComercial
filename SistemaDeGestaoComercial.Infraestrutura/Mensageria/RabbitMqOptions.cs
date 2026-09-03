namespace SistemaDeGestaoComercial.Infraestrutura.Mensageria;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = RabbitMqTopology.EventsExchange;
    public int PollingIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
    public int RetryDelaySeconds { get; set; } = 15;
    public int MaxRetries { get; set; } = 3;
    public bool Enabled { get; set; } = true;
}

public static class RabbitMqTopology
{
    public const string EventsExchange = "gestao-comercial.events";
    public const string VendaRealizadaRoutingKey = "venda.realizada";
    public const string EstoqueQueue = "gestao-comercial.estoque";
    public const string RetryExchange = "gestao-comercial.retry";
    public const string RetryQueue = "gestao-comercial.estoque.retry";
    public const string DeadLetterExchange = "gestao-comercial.dlx";
    public const string DeadLetterQueue = "gestao-comercial.estoque.dlq";
    public const string ConsumerName = "VendaRealizadaConsumer";
}
