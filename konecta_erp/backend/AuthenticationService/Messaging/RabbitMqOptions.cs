namespace AuthenticationService.Messaging
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
        public string EmployeeCreatedQueue { get; set; } = "auth.employee-created";
        public string EmployeeCreatedRoutingKey { get; set; } = "hr.employee.created";
        public string EmployeeExitedRoutingKey { get; set; } = "hr.employee.exited";
        public string EmployeeResignationApprovedRoutingKey { get; set; } = "hr.employee.resignation.approved";
        public string UserProvisionedRoutingKey { get; set; } = "auth.user.provisioned";
        public string UserDeactivatedRoutingKey { get; set; } = "auth.user.deactivated";
        public string UserResignedRoutingKey { get; set; } = "auth.user.resigned";
    }
}
