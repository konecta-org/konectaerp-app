namespace UserManagementService.Events
{
    public record UserCreatedEvent(string UserId, string Email, string FullName, string Role);
}
