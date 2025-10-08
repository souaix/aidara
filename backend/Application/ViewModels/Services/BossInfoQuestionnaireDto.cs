namespace Backend.Application.ViewModels.Services;

/// <summary>
/// 小老闆服務問卷提交資料
/// </summary>
public class BossInfoQuestionnaireDto
{
    /// <summary>使用者 ID (UUID)</summary>
    public Guid UserId { get; set; }

    // Step1: 服務項目（可保留）
    public List<string> ServiceItemIds { get; set; } = new();

    // Step2: 每個項目的價格區間
    public List<ItemPriceRangeDto> ItemPriceRanges { get; set; } = new();

    // Step3: 服務方式
    public List<string> ServiceMethods { get; set; } = new();

    // Step4: 服務範圍
    public List<ServiceAreaDto> ServiceAreas { get; set; } = new();

    // Step5: 服務據點
    public List<ServiceAddressDto> ServiceAddresses { get; set; } = new();
}

/// <summary>服務項目的價格設定</summary>
public class ItemPriceRangeDto
{
    /// <summary>服務項目 ID（VARCHAR）</summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>最低價格</summary>
    public int MinPrice { get; set; }

    /// <summary>最高價格</summary>
    public int MaxPrice { get; set; }
}

/// <summary>可服務的行政區域</summary>
public class ServiceAreaDto
{
    /// <summary>城市代碼</summary>
    public int CityId { get; set; }

    /// <summary>行政區代碼</summary>
    public int DistrictId { get; set; }
}

/// <summary>服務據點地址</summary>
public class ServiceAddressDto
{
    /// <summary>城市代碼</summary>
    public int CityId { get; set; }

    /// <summary>行政區代碼</summary>
    public int DistrictId { get; set; }

    /// <summary>街道名稱</summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>門牌號碼</summary>
    public string AddressNo { get; set; } = string.Empty;
}
