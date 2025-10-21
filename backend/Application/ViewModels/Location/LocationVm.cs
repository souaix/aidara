namespace Backend.Application.ViewModels;

public class LocationCityVm
{
    public int CityId { get; set; }
    public string CityName { get; set; } = "";
    public string Code { get; set; } = "";
}

public class LocationDistrictVm
{
    public int DistrictId { get; set; }
    public int CityId { get; set; }
    public string DistrictName { get; set; } = "";
    public string Code { get; set; } = "";
}

public class LocationPostalVm
{
    public int PostalId { get; set; }
    public int DistrictId { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string? Locality { get; set; }
}