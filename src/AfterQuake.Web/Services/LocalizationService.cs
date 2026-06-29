using System.Collections.Concurrent;
using System.Globalization;

namespace AfterQuake.Web.Services;

public interface ILocalizationService
{
    string this[string key] { get; }
    string Get(string key, params object[] args);
    string Get(string key, string culture);
    string CurrentCulture { get; }
    void SetCulture(string culture);
}

public class LocalizationService : ILocalizationService
{
    private static readonly ConcurrentDictionary<string, string> _es = new();
    private static readonly ConcurrentDictionary<string, string> _en = new();

    private static readonly string _defaultCulture = "es-ES";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    static LocalizationService()
    {
        LoadSpanish();
        LoadEnglish();
    }

    public string CurrentCulture
    {
        get
        {
            var culture = _httpContextAccessor.HttpContext?.Items["Culture"] as string;
            return culture ?? _defaultCulture;
        }
    }

    public string this[string key] => Get(key);

    public void SetCulture(string culture)
    {
        if (_httpContextAccessor.HttpContext != null)
            _httpContextAccessor.HttpContext.Items["Culture"] = culture;
    }

    public string Get(string key)
    {
        return Get(key, CurrentCulture);
    }

    public string Get(string key, params object[] args)
    {
        var value = Get(key);
        return args.Length > 0 ? string.Format(value, args) : value;
    }

    public string Get(string key, string culture)
    {
        var dict = culture?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true ? _en : _es;
        return dict.TryGetValue(key, out var value) ? value : $"[[{key}]]";
    }

    private static void AddSpanish(string key, string value) => _es[key] = value;
    private static void AddEnglish(string key, string value) => _en[key] = value;

    private static void LoadSpanish()
    {
        AddSpanish("App.Title", "AfterQuake - Respuesta ante Terremotos");
        AddSpanish("App.ShortTitle", "AfterQuake");
        AddSpanish("App.Description", "Sistema de Respuesta ante Terremotos");
        AddSpanish("App.Footer", "Sistema de Respuesta ante Terremotos");

        AddSpanish("SOS", "SOS");
        AddSpanish("Emergency", "Emergencia");
        AddSpanish("Emergencies", "Emergencias");
        AddSpanish("Report.Emergency", "Reportar Emergencia");
        AddSpanish("Report.Emergency.Description", "Reporte una emergencia sísmica");
        AddSpanish("Emergency.Type", "Tipo de Emergencia");
        AddSpanish("Emergency.Severity", "Severidad");
        AddSpanish("Emergency.Location", "Ubicación");
        AddSpanish("Emergency.Status", "Estado");
        AddSpanish("Emergency.Critical", "Crítico");
        AddSpanish("Emergency.Pending", "Pendiente");
        AddSpanish("Emergency.Assigned", "Asignado");
        AddSpanish("Emergency.Resolved", "Resuelto");

        AddSpanish("Person", "Personas");
        AddSpanish("Person.ReportMissing", "Reportar Persona Desaparecida");
        AddSpanish("Person.ReportFound", "Reportar Persona Encontrada");
        AddSpanish("Person.Missing", "Desaparecido");
        AddSpanish("Person.Found", "Encontrado");
        AddSpanish("Person.Name", "Nombre");
        AddSpanish("Person.LastSeen", "Última vez visto");
        AddSpanish("Person.Description", "Descripción");
        AddSpanish("Person.Photo", "Foto");
        AddSpanish("Person.Search", "Buscar Personas");
        AddSpanish("Person.Results", "Resultados de búsqueda");

        AddSpanish("Help", "Ayuda");
        AddSpanish("Help.Request", "Solicitar Ayuda");
        AddSpanish("Help.Request.Create", "Crear Solicitud de Ayuda");
        AddSpanish("Help.Request.Type", "Tipo de Ayuda");
        AddSpanish("Help.Request.Description", "Describa la ayuda que necesita");
        AddSpanish("Help.Request.Water", "Agua");
        AddSpanish("Help.Request.Food", "Alimentos");
        AddSpanish("Help.Request.Medical", "Atención Médica");
        AddSpanish("Help.Request.Shelter", "Refugio");
        AddSpanish("Help.Request.Rescue", "Rescate");
        AddSpanish("Help.Request.Supplies", "Suministros");

        AddSpanish("Shelter", "Refugio");
        AddSpanish("Shelter.Map", "Mapa de Refugios");
        AddSpanish("Shelter.List", "Lista de Refugios");
        AddSpanish("Shelter.Capacity", "Capacidad");
        AddSpanish("Shelter.Occupancy", "Ocupación");
        AddSpanish("Shelter.Available", "Disponible");

        AddSpanish("Account.Login", "Iniciar Sesión");
        AddSpanish("Account.Register", "Registrarse");
        AddSpanish("Account.Logout", "Cerrar Sesión");
        AddSpanish("Account.Email", "Correo Electrónico");
        AddSpanish("Account.Password", "Contraseña");
        AddSpanish("Account.ConfirmPassword", "Confirmar Contraseña");
        AddSpanish("Account.ForgotPassword", "Olvidé mi Contraseña");
        AddSpanish("Account.RememberMe", "Recordarme");

        AddSpanish("Common.Save", "Guardar");
        AddSpanish("Common.Cancel", "Cancelar");
        AddSpanish("Common.Delete", "Eliminar");
        AddSpanish("Common.Edit", "Editar");
        AddSpanish("Common.Search", "Buscar");
        AddSpanish("Common.Loading", "Cargando...");
        AddSpanish("Common.Error", "Error");
        AddSpanish("Common.Success", "Éxito");
        AddSpanish("Common.Close", "Cerrar");
        AddSpanish("Common.Confirm", "Confirmar");

        AddSpanish("Nav.Guides", "Guías");
        AddSpanish("Nav.Contacts", "Contactos");
        AddSpanish("Nav.Admin", "Administración");
    }

    private static void LoadEnglish()
    {
        AddEnglish("App.Title", "AfterQuake - Earthquake Response");
        AddEnglish("App.ShortTitle", "AfterQuake");
        AddEnglish("App.Description", "Earthquake Response System");
        AddEnglish("App.Footer", "Earthquake Response System");

        AddEnglish("SOS", "SOS");
        AddEnglish("Emergency", "Emergency");
        AddEnglish("Emergencies", "Emergencies");
        AddEnglish("Report.Emergency", "Report Emergency");
        AddEnglish("Report.Emergency.Description", "Report a seismic emergency");
        AddEnglish("Emergency.Type", "Emergency Type");
        AddEnglish("Emergency.Severity", "Severity");
        AddEnglish("Emergency.Location", "Location");
        AddEnglish("Emergency.Status", "Status");
        AddEnglish("Emergency.Critical", "Critical");
        AddEnglish("Emergency.Pending", "Pending");
        AddEnglish("Emergency.Assigned", "Assigned");
        AddEnglish("Emergency.Resolved", "Resolved");

        AddEnglish("Person", "People");
        AddEnglish("Person.ReportMissing", "Report Missing Person");
        AddEnglish("Person.ReportFound", "Report Found Person");
        AddEnglish("Person.Missing", "Missing");
        AddEnglish("Person.Found", "Found");
        AddEnglish("Person.Name", "Name");
        AddEnglish("Person.LastSeen", "Last Seen");
        AddEnglish("Person.Description", "Description");
        AddEnglish("Person.Photo", "Photo");
        AddEnglish("Person.Search", "Search People");
        AddEnglish("Person.Results", "Search Results");

        AddEnglish("Help", "Help");
        AddEnglish("Help.Request", "Request Help");
        AddEnglish("Help.Request.Create", "Create Help Request");
        AddEnglish("Help.Request.Type", "Help Type");
        AddEnglish("Help.Request.Description", "Describe the help you need");
        AddEnglish("Help.Request.Water", "Water");
        AddEnglish("Help.Request.Food", "Food");
        AddEnglish("Help.Request.Medical", "Medical Care");
        AddEnglish("Help.Request.Shelter", "Shelter");
        AddEnglish("Help.Request.Rescue", "Rescue");
        AddEnglish("Help.Request.Supplies", "Supplies");

        AddEnglish("Shelter", "Shelter");
        AddEnglish("Shelter.Map", "Shelter Map");
        AddEnglish("Shelter.List", "Shelter List");
        AddEnglish("Shelter.Capacity", "Capacity");
        AddEnglish("Shelter.Occupancy", "Occupancy");
        AddEnglish("Shelter.Available", "Available");

        AddEnglish("Account.Login", "Sign In");
        AddEnglish("Account.Register", "Register");
        AddEnglish("Account.Logout", "Sign Out");
        AddEnglish("Account.Email", "Email");
        AddEnglish("Account.Password", "Password");
        AddEnglish("Account.ConfirmPassword", "Confirm Password");
        AddEnglish("Account.ForgotPassword", "Forgot Password");
        AddEnglish("Account.RememberMe", "Remember Me");

        AddEnglish("Common.Save", "Save");
        AddEnglish("Common.Cancel", "Cancel");
        AddEnglish("Common.Delete", "Delete");
        AddEnglish("Common.Edit", "Edit");
        AddEnglish("Common.Search", "Search");
        AddEnglish("Common.Loading", "Loading...");
        AddEnglish("Common.Error", "Error");
        AddEnglish("Common.Success", "Success");
        AddEnglish("Common.Close", "Close");
        AddEnglish("Common.Confirm", "Confirm");

        AddEnglish("Nav.Guides", "Guides");
        AddEnglish("Nav.Contacts", "Contacts");
        AddEnglish("Nav.Admin", "Administration");
    }
}
