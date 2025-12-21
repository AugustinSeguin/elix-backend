namespace ElixBackend.Business.DTO;

using ElixBackend.Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class ResourceDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est requis.")]
    public required string Name { get; set; }

    public required LocalizationDto Localization { get; set; }

    public string? PhoneNumber { get; set; }

    public static ResourceDto ResourceToResourceDto(Resource resource)
    {
        return new ResourceDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Localization = new LocalizationDto
            {
                Latitude = resource.Localization.Latitude,
                Longitude = resource.Localization.Longitude
            },
            PhoneNumber = resource.PhoneNumber
        };
    }
}

public class LocalizationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
