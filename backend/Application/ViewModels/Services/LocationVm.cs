namespace Backend.Application.ViewModels.Services;

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
