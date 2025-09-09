using Backend.Domain.Wallet;
// Backend.Contracts/Wallet/CreateLedgerRequest.cs
namespace Backend.Contracts.Wallet;
public sealed record CreateLedgerRequest(
    Guid UserId,
    string Currency,          // "TWD","USD"...
    long Amount,            // 單位=分，可正可負，!= 0
    string TxType,            // 交易型別
    long? OrderId = null,
    object? Meta = null,    // 任意 JSON 物件
    bool RejectNegative = true // 若為 true，交易後餘額 < 0 則拒絕
);
