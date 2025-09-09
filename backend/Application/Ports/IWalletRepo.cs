// Backend.Application/Ports/IWalletRepo.cs
using Backend.Contracts.Wallet;

namespace Backend.Application.Ports;
public interface IWalletRepo
{
    Task<LedgerDto> InsertAsync(CreateLedgerRequest req, CancellationToken ct);
    Task<long> GetCurrentBalanceAsync(Guid userId, string currency, CancellationToken ct); // for RejectNegative 預檢
}
