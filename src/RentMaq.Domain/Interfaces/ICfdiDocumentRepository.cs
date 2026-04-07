using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface ICfdiDocumentRepository : IRepository<CfdiDocument>
{
    Task<IReadOnlyList<CfdiDocument>> GetByContractAsync(Guid contractId, CancellationToken ct = default);
    Task<CfdiDocument?> GetByUuidFiscalAsync(Guid uuidFiscal, CancellationToken ct = default);
}
