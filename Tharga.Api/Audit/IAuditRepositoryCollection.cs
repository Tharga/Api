using Tharga.MongoDB;

namespace Tharga.Api.Audit;

public interface IAuditRepositoryCollection : IDiskRepositoryCollection<AuditEntryEntity>
{
}
