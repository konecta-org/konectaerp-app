namespace UserManagementService.Messaging;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "konecta.erp";
    public string UserEventsQueue { get; set; } = "user-management.user-events";
    public string UserProvisionedRoutingKey { get; set; } = "auth.user.provisioned";
    public string UserDeactivatedRoutingKey { get; set; } = "auth.user.deactivated";
    public string UserResignedRoutingKey { get; set; } = "auth.user.resigned";
    public string UserTerminatedRoutingKey { get; set; } = "auth.user.terminated";
}
