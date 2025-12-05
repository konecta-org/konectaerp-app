namespace UserManagementService.Dtos
{
    public record PagedResultDto<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalItems)
    {
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}
