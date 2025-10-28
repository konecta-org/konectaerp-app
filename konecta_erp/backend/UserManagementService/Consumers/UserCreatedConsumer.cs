using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Events;
using UserManagementService.Models;

namespace UserManagementService.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly AppDbContext _db;

        public UserCreatedConsumer(AppDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var msg = context.Message;

           
            var exists = await _db.Users.AnyAsync(u => u.Id == msg.UserId);
            if (exists) return;

            var user = new User
            {
                Id = msg.UserId,
                Email = msg.Email,
                FullName = msg.FullName,
                Role = string.IsNullOrWhiteSpace(msg.Role) ? "Employee" : msg.Role,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
    }
}
