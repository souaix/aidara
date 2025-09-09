// Backend.Application/Wallet/WalletService.cs
using Backend.Application.Ports;
using Backend.Contracts.Wallet;

namespace Backend.Application.Wallet;

public sealed class WalletService
{
    private readonly Func<IUnitOfWork> _uowFactory;

    public WalletService(Func<IUnitOfWork> uowFactory) => _uowFactory = uowFactory;

    public async Task<LedgerDto> CreateLedgerAsync(CreateLedgerRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Currency))
            throw new ArgumentException("Currency must be exists.", nameof(req.Currency));
        if (req.Amount == 0) throw new ArgumentException("Amount must be non-zero.", nameof(req.Amount));

        await using var uow = _uowFactory();
        await uow.BeginAsync(ct);
        try
        {
            var repo = uow.CreateWalletRepo();

            if (req.RejectNegative)
            {
                var cur = await repo.GetCurrentBalanceAsync(req.UserId, req.Currency, ct);
                var after = cur + req.Amount;
                if (after < 0)
                    throw new InvalidOperationException("Insufficient balance.");
            }

            var dto = await repo.InsertAsync(req, ct);
            await uow.CommitAsync(ct);
            return dto;
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}
