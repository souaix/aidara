namespace Frontend.Web.ViewModels
{
    /// <summary>
    /// 前端 Modal 編輯服務表單的 ViewModel
    /// </summary>
    public class BossServiceEditVm
    {
        /// <summary>使用者 ID（由登入狀態帶入）</summary>
        public Guid UserId { get; set; }

        /// <summary>服務項目 ID（例如 cleaning001）。</summary>
        public string? ItemId { get; set; }

        /// <summary>最低價格（初始可空，稍後由 API 帶回填）</summary>
        public int? MinPrice { get; set; }

        /// <summary>最高價格（初始可空，稍後由 API 帶回填）</summary>
        public int? MaxPrice { get; set; }
    }
}
