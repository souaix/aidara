namespace Backend.Contracts.Wallet;

public sealed record BalanceDto(
	string Currency, // 幣別，例如 "Gold" / "Silver" / "TWD"
	int Balance   // 可用餘額（整數單位 = 分 / 點數）
);
