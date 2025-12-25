namespace Frontend.Web.ViewModels
{
    public class BossProfileEditVm
    {
        /// <summary>
        /// 使用者唯一識別碼
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 使用者頭像 URL
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// 電子郵件（唯一）
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// 手機或電話
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = null!;

        /// <summary>
        /// 性別：M=男, F=女, O=其他, N=未填
        /// </summary>
        public string Gender { get; set; } = "N";

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 上次上線時間
        /// </summary>
        public DateTimeOffset? LastSeenAt { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 行政區
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string? Street { get; set; }

        /// <summary>
        /// 門牌號碼
        /// </summary>
        public string? AddressNo { get; set; }

        /// <summary>
        /// 負責人名稱
        /// </summary>
        public string BossName { get; set; } = string.Empty;
    }
}
