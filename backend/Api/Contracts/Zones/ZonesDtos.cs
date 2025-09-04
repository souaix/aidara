namespace Backend.Api.Contracts.Zones;

public sealed record ZoneCountItem(
    long Id,
    string Code,
    string Name,
    string Kind,
    double Lon,   // 區中心點（方便前端放一顆數字泡泡）
    double Lat,
    int Count);

public sealed record SearchZonesCountsRequest(
    double MinLon, double MinLat, double MaxLon, double MaxLat);
