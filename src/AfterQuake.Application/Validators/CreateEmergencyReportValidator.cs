using FluentValidation;
using AfterQuake.Application.DTOs;

namespace AfterQuake.Application.Validators;

public class CreateEmergencyReportValidator : AbstractValidator<CreateEmergencyReportDto>
{
    public CreateEmergencyReportValidator()
    {
        RuleFor(x => x.EmergencyType).IsInEnum().WithMessage("Selecciona un tipo de emergencia válido.");
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage("Latitud inválida.");
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage("Longitud inválida.");
        RuleFor(x => x.AffectedPeople).GreaterThanOrEqualTo(0).WithMessage("Número de afectados inválido.");
        RuleFor(x => x.ReporterPhone).MaximumLength(20).WithMessage("Teléfono demasiado largo.");
        RuleFor(x => x.Description).MaximumLength(2000).WithMessage("La descripción es demasiado larga (máx. 2000 caracteres).");
    }
}

public class CreatePersonReportValidator : AbstractValidator<CreatePersonReportDto>
{
    public CreatePersonReportValidator()
    {
        RuleFor(x => x.ReportType).IsInEnum().WithMessage("Tipo de reporte inválido.");
        RuleFor(x => x.MissingPersonName).NotEmpty().WithMessage("El nombre de la persona es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre es demasiado largo.");
        RuleFor(x => x.ContactPhone).NotEmpty().WithMessage("El teléfono de contacto es obligatorio.")
            .MaximumLength(20).WithMessage("Teléfono demasiado largo.");
        RuleFor(x => x.Description).MaximumLength(2000).WithMessage("La descripción es demasiado larga.");
        When(x => x.Age.HasValue, () =>
        {
            RuleFor(x => x.Age!.Value).InclusiveBetween(0, 120).WithMessage("Edad inválida.");
        });
    }
}

public class CreateHelpRequestValidator : AbstractValidator<CreateHelpRequestDto>
{
    public CreateHelpRequestValidator()
    {
        RuleFor(x => x.RequestType).IsInEnum().WithMessage("Selecciona un tipo de ayuda válido.");
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage("Latitud inválida.");
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage("Longitud inválida.");
        RuleFor(x => x.PeopleCount).InclusiveBetween(1, 100).WithMessage("Número de personas inválido.");
        RuleFor(x => x.RequesterPhone).NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(20).WithMessage("Teléfono demasiado largo.");
        RuleFor(x => x.Description).MaximumLength(2000).WithMessage("La descripción es demasiado larga.");
    }
}

public class CreateAlertValidator : AbstractValidator<CreateAlertDto>
{
    public CreateAlertValidator()
    {
        RuleFor(x => x.AlertType).IsInEnum().WithMessage("Tipo de alerta inválido.");
        RuleFor(x => x.Severity).IsInEnum().WithMessage("Severidad inválida.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(200).WithMessage("El título es demasiado largo.");
        RuleFor(x => x.Message).MaximumLength(4000).WithMessage("El mensaje es demasiado largo.");
    }
}

public class CreateShelterValidator : AbstractValidator<CreateShelterDto>
{
    public CreateShelterValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre del albergue es obligatorio.")
            .MaximumLength(200).WithMessage("Nombre demasiado largo.");
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage("Latitud inválida.");
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage("Longitud inválida.");
        RuleFor(x => x.TotalCapacity).GreaterThan(0).WithMessage("La capacidad debe ser mayor a 0.");
    }
}

public class CreateDonationValidator : AbstractValidator<CreateDonationDto>
{
    public CreateDonationValidator()
    {
        RuleFor(x => x.DonationType).IsInEnum().WithMessage("Tipo de donación inválido.");
        When(x => x.DonationType == Domain.Enumerations.DonationType.Monetary, () =>
        {
            RuleFor(x => x.MonetaryAmount).NotNull().WithMessage("El monto es obligatorio.")
                .GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");
        });
        When(x => x.DonationType == Domain.Enumerations.DonationType.InKind, () =>
        {
            RuleFor(x => x.ItemName).NotEmpty().WithMessage("El nombre del artículo es obligatorio.");
            RuleFor(x => x.ItemQuantity).NotNull().GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
        });
    }
}

public class RegisterVolunteerValidator : AbstractValidator<RegisterVolunteerDto>
{
    public RegisterVolunteerValidator()
    {
        RuleFor(x => x.Skills).NotEmpty().WithMessage("Indica al menos una habilidad.")
            .MaximumLength(1000).WithMessage("La lista de habilidades es demasiado larga.");
        RuleFor(x => x.MaxHoursPerDay).InclusiveBetween(1, 24).WithMessage("Horas por día inválidas.");
    }
}
