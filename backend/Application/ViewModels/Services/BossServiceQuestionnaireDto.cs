namespace Backend.Application.ViewModels.Services;

public class BossServiceQuestionnaireDto
{
    public Guid UserId { get; set; }

    // Step1: 服務項目（可保留，後端若不再用可忽略）
    public List<Guid> ServiceItemIds { get; set; } = new();

    // Step2: 每個項目的價格區間
    public List<ItemPriceRangeDto> ItemPriceRanges { get; set; } = new();

    // Step3: 服務方式
    public List<string> ServiceMethods { get; set; } = new();

    // Step4: 服務範圍
    public List<ServiceAreaDto> ServiceAreas { get; set; } = new();

    // Step5: 服務據點
    public List<ServiceAddressDto> ServiceAddresses { get; set; } = new();
}

public class ItemPriceRangeDto
{
    public Guid ItemId { get; set; }
    public int MinPrice { get; set; }
    public int MaxPrice { get; set; }
}

public class ServiceAreaDto
{
    public int CityId { get; set; }
    public int DistrictId { get; set; }
}

public class ServiceAddressDto
{
    public int CityId { get; set; }
    public int DistrictId { get; set; }
    public string Street { get; set; } = "";
    public string AddressNo { get; set; } = "";
}
