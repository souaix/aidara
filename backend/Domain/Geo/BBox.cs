namespace Backend.Domain.Geo;

public readonly record struct BBox(double MinLon, double MinLat, double MaxLon, double MaxLat);
