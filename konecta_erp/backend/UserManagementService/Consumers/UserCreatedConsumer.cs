using MassTransit;
using UserManagementService.Events;
using UserManagementService.Services;

namespace UserManagementService.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(IUserService userService, ILogger<UserCreatedConsumer> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;

            try
            {
                await _userService.CreateOrUpdateFromExternalAsync(
                    message.UserId,
                    message.Email,
                    message.FullName,
                    message.Role,
                    context.CancellationToken);

                _logger.LogInformation("Processed user-created event for {UserId}", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process user-created event for {UserId}", message.UserId);
                throw;
            }
        }
    }
}
