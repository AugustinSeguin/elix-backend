namespace ElixBackend.Domain.Entities;

public class Resource
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required Localization Localization { get; set; }
    public string? PhoneNumber { get; set; }
}

public class Localization
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
