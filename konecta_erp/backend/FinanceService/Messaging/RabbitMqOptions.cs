namespace FinanceService.Messaging
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string Exchange { get; set; } = "konecta.erp";
        public string CompensationEventsQueue { get; set; } = "finance.compensation.events";
        public string CompensationProvisionedRoutingKey { get; set; } = "finance.compensation.provisioned";
        public string CompensationBonusesRoutingKey { get; set; } = "finance.compensation.bonuses";
        public string CompensationDeductionsRoutingKey { get; set; } = "finance.compensation.deductions";
    }
}
