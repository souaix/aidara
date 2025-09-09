// Backend.Contracts/Wallet/LedgerDto.cs
using Backend.Domain.Wallet;

namespace Backend.Contracts.Wallet;

public sealed record LedgerDto(
    long TxId,
    Guid UserId,
    long? OrderId,
    string TxType,   // 👈 改成 string 接 DB 的 "DEPOSIT"
    int Amount,
    string Currency,
    int BalanceAfter,
    string? Meta,
    DateTime CreatedAt
);

