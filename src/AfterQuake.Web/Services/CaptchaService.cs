namespace AfterQuake.Web.Services;

public class CaptchaService
{
    private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly Dictionary<string, string> _captchaStore = new();

    public (string id, string svg) Generate()
    {
        var id = Guid.NewGuid().ToString();
        var code = new string(Enumerable.Range(0, 5).Select(_ => Characters[Random.Shared.Next(Characters.Length)]).ToArray());
        _captchaStore[id] = code;
        if (_captchaStore.Count > 1000)
        {
            var expired = _captchaStore.Keys.Take(500).ToList();
            foreach (var k in expired)
                _captchaStore.Remove(k);
        }
        return (id, GenerateSvg(code));
    }

    public bool Validate(string id, string code) =>
        _captchaStore.TryGetValue(id, out var stored) &&
        string.Equals(stored, code, StringComparison.OrdinalIgnoreCase) &&
        _captchaStore.Remove(id);

    private static string GenerateSvg(string code)
    {
        var chars = code.Select((c, i) =>
            $"<text x=\"{10 + i * 18}\" y=\"30\" font-size=\"22\" font-family=\"monospace\" " +
            $"fill=\"#{Random.Shared.Next(0x333333, 0x999999):X6}\" " +
            $"transform=\"rotate({Random.Shared.Next(-15, 15)} {10 + i * 18} 25)\">{c}</text>");
        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"110\" height=\"40\" viewBox=\"0 0 110 40\">" +
               $"<rect width=\"110\" height=\"40\" fill=\"#f0f0f0\" rx=\"4\"/>" +
               string.Join("", chars) + "</svg>";
    }
}
