namespace Frontend.Web.ViewModels
{
    public class BossStoreEditVm
    {
        /// <summary>使用者 ID（由登入狀態帶入）</summary>
        public Guid UserId { get; set; }
        public List<string>? Methods { get; set; } = new();
        public List<BossServiceAreaVm>? Areas { get; set; } = new();
        public List<BossServiceAddressVm>? Addresses { get; set; } = new();
    }

    public class BossServiceAreaVm
    {
        public int CityId { get; set; }
        //public string? CityName { get; set; }
        public int DistrictId { get; set; }
        //public string? DistrictName { get; set; }
        public string? LocalityId { get; set; }
        public string? Note { get; set; }
    }

    public class BossServiceAddressVm
    {
        public int CityId { get; set; }
        //public string? CityName { get; set; }
        public int DistrictId { get; set; }
        //public string? DistrictName { get; set; }
        public string? Street { get; set; }
        public string? AddressNo { get; set; }
        //public string? Phone { get; set; }
        //public string? ContactName { get; set; }
        //public decimal? Lat { get; set; }
        //public decimal? Lng { get; set; }
    }
}
