using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.ViewModels.Boss
{
    public class BossServiceJoinRow
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ItemId { get; set; } = default!;
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public string? CategoryName { get; set; }
        public string? SubcategoryName { get; set; }
        public string? ItemName { get; set; }

        public string? Method { get; set; }

        public int CityId { get; set; }
        public string? CityName { get; set; }
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public int PostalId { get; set; }
        public string? PostalCode { get; set; }
        public string? Locality { get; set; }
        public string? AreaNote { get; set; }

        public int AddrCityId { get; set; }
        public string? AddrCityName { get; set; }
        public int AddrDistrictId { get; set; }
        public string? AddrDistrictName { get; set; }
        public string? Street { get; set; }
        public string? AddressNo { get; set; }
        public int AddressPostalId { get; set; }
        public string? Phone { get; set; }
        public string? ContactName { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }

    public class BossServiceFullVm
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ItemId { get; set; } = default!;
        public string? CategoryName { get; set; }
        public string? SubcategoryName { get; set; }
        public string? ItemName { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public List<string> Methods { get; set; } = new();
        public List<BossServiceAreaVm> Areas { get; set; } = new();
        public List<BossServiceAddressVm> Addresses { get; set; } = new();
    }

    public class BossServiceAreaVm
    {
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public int PostalId { get; set; }
        public string? PostalCode { get; set; }
        public string? Locality { get; set; }
        public string? Note { get; set; }
    }

    public class BossServiceAddressVm
    {
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public string? Street { get; set; }
        public string? AddressNo { get; set; }
        public int AddressPostalId { get; set; }
        public string? Phone { get; set; }
        public string? ContactName { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }

}

