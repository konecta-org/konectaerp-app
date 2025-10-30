using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    public class UserQueryParameters
    {
        private const int MaxPageSize = 100;

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        private int _pageSize = 25;

        [Range(1, MaxPageSize)]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Min(MaxPageSize, Math.Max(1, value));
        }

        [MaxLength(128)]
        public string? Department { get; set; }

        [MaxLength(64)]
        public string? Role { get; set; }

        [MaxLength(256)]
        public string? Search { get; set; }

        public bool IncludeDeleted { get; set; }

        public bool OnlyLocked { get; set; }
    }
}
