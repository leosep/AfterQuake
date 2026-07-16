using AfterQuake.Application.DTOs;
using AfterQuake.Application.Validators;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Tests.Validators;

public class EmergencyReportValidatorTests
{
    private readonly CreateEmergencyReportValidator _validator = new();

    [Fact]
    public void Latitude0_Longitude0_NullAddress_ShouldFail()
    {
        var dto = new CreateEmergencyReportDto
        {
            EmergencyType = EmergencyType.Medical,
            Latitude = 0,
            Longitude = 0,
            Address = null,
            Description = "Test"
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("ubicación"));
    }

    [Fact]
    public void Latitude0_Longitude0_EmptyAddress_ShouldFail()
    {
        var dto = new CreateEmergencyReportDto
        {
            EmergencyType = EmergencyType.Medical,
            Latitude = 0,
            Longitude = 0,
            Address = "",
            Description = "Test"
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("ubicación"));
    }

    [Fact]
    public void Latitude0_Longitude0_WithAddress_ShouldPass()
    {
        var dto = new CreateEmergencyReportDto
        {
            EmergencyType = EmergencyType.Medical,
            Latitude = 0,
            Longitude = 0,
            Address = "Calle Principal 123, Santo Domingo",
            Description = "Test"
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidCoordinates_ShouldPass()
    {
        var dto = new CreateEmergencyReportDto
        {
            EmergencyType = EmergencyType.Fire,
            Latitude = 18.4861,
            Longitude = -69.9312,
            Address = "Av. Independencia 123",
            Description = "Test"
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }
}
