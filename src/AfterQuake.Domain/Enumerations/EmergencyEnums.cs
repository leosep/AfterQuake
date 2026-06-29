namespace AfterQuake.Domain.Enumerations;

public enum AlertLevel
{
    Green = 0,
    Yellow = 1,
    Orange = 2,
    Red = 3
}

public enum EmergencyType
{
    StructuralCollapse = 0,
    Fire = 1,
    Flood = 2,
    Landslide = 3,
    Medical = 4,
    Tsunami = 5,
    GasLeak = 6,
    Other = 7
}

public enum EmergencySeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum EmergencyStatus
{
    Pending = 0,
    Assigned = 1,
    InProgress = 2,
    Resolved = 3,
    Cancelled = 4
}

public enum PersonReportType
{
    Missing = 0,
    Found = 1,
    Safe = 2
}

public enum PersonReportStatus
{
    Active = 0,
    Resolved = 1,
    Duplicate = 2,
    Closed = 3
}

public enum HelpRequestType
{
    Water = 0,
    Food = 1,
    Shelter = 2,
    Medicine = 3,
    MedicalAttention = 4,
    Rescue = 5,
    Transportation = 6,
    Clothing = 7,
    Hygiene = 8,
    Communication = 9,
    Other = 10
}

public enum HelpRequestPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum HelpRequestStatus
{
    Pending = 0,
    Assigned = 1,
    InProgress = 2,
    Resolved = 3,
    Cancelled = 4
}

public enum HelpOfferType
{
    Donation = 0,
    Transport = 1,
    TemporaryHousing = 2,
    VolunteerWork = 3,
    MedicalSupplies = 4,
    Equipment = 5,
    Food = 6,
    Other = 7
}

public enum ShelterStatus
{
    Active = 0,
    Full = 1,
    Closed = 2,
    UnderRepair = 3
}

public enum DonationType
{
    Monetary = 0,
    InKind = 1
}

public enum DonationStatus
{
    Pending = 0,
    Received = 1,
    Distributed = 2,
    Cancelled = 3
}

public enum ServiceType
{
    Water = 0,
    Electricity = 1,
    Gas = 2,
    Telecommunications = 3,
    Roads = 4,
    Internet = 5
}

public enum ServiceStatusType
{
    Operational = 0,
    Partial = 1,
    Interrupted = 2,
    Critical = 3
}

public enum VolunteerSkill
{
    Medical = 0,
    Rescue = 1,
    Logistics = 2,
    Psychology = 3,
    Communication = 4,
    Engineering = 5,
    FirstAid = 6,
    SearchAndRescue = 7,
    Driving = 8,
    HeavyMachinery = 9,
    Translation = 10,
    Other = 11
}

public enum VolunteerStatus
{
    Available = 0,
    Assigned = 1,
    OnDuty = 2,
    Resting = 3,
    Unavailable = 4
}

public enum AlertType
{
    Aftershock = 0,
    Tsunami = 1,
    Evacuation = 2,
    ShelterUpdate = 3,
    ServiceRestoration = 4,
    General = 5,
    Weather = 6
}

public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    Sms = 2,
    All = 3
}

public enum UserRole
{
    Citizen = 0,
    Volunteer = 1,
    ReliefOrganization = 2,
    Administrator = 3,
    SuperAdministrator = 4
}
