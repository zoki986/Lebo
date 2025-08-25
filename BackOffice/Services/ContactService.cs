using Lebo.Models.Contact;
using Lebo.Models.Shared;
using NPoco;
using Umbraco.Cms.Infrastructure.Scoping;

public interface IContactService
{
    Task<Guid> AddMessageAsync(ContactMessageDto message, CancellationToken ct = default);
    Task<PagedResult<ContactMessage>> GetPageAsync(int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task<bool> RemoveMessageAsync(Guid id, CancellationToken ct = default);
}

public sealed class ContactService : IContactService
{
    private readonly IScopeProvider _scopeProvider;
    private readonly ILogger<ContactService> _logger;

    public ContactService(IScopeProvider scopeProvider, ILogger<ContactService> logger)
    {
        _scopeProvider = scopeProvider;
        _logger = logger;
    }

    public async Task<Guid> AddMessageAsync(ContactMessageDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var db = scope.Database;

            var entity = dto.ToModel();
            entity.Id = Guid.NewGuid();
            entity.SubmittedAt = DateTime.UtcNow;

            await db.InsertAsync(entity);
            _logger.LogInformation("ContactMessage inserted. Id={Id} Email={Email}", entity.Id, entity.Email);
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Insert failed. Email={Email}", dto.Email);
            throw;
        }
    }

    public async Task<PagedResult<ContactMessage>> GetPageAsync(int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 500);

        try
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var db = scope.Database;

            var sql = new Sql("SELECT * FROM ContactMessages ORDER BY SubmittedAt DESC");
            var p = await db.PageAsync<ContactMessage>(page, pageSize, sql);

            return new PagedResult<ContactMessage>
            {
                Items = p.Items,
                Page = (int)p.CurrentPage,
                PageSize = (int)p.ItemsPerPage,
                TotalCount = p.TotalItems
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fetch page failed. Page={Page} Size={Size}", page, pageSize);
            throw;
        }
    }

    public async Task<bool> RemoveMessageAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var db = scope.Database;

            var entity = await db.SingleOrDefaultByIdAsync<ContactMessage>(id);
            if (entity is null)
            {
                _logger.LogWarning("Delete skipped. Not found. Id={Id}", id);
                return false;
            }

            await db.DeleteAsync(entity);
            _logger.LogInformation("ContactMessage deleted. Id={Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed. Id={Id}", id);
            throw;
        }
    }
}
