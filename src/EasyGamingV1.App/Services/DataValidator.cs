using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EasyGamingV1.Services;

public static class DataValidator
{
    public static async Task<bool> ValidateAsync()
    {
        var root = Paths.RepoRoot;
        var data = Path.Combine(root, "data", "game-profiles.json");
        var sha = Path.Combine(root, "data", "game-profiles.json.sha256");
        if (!File.Exists(data) || !File.Exists(sha)) return false;

        using var fs = File.OpenRead(data);
        var hash = await SHA256.HashDataAsync(fs);
        var hex = Convert.ToHexString(hash).ToLowerInvariant();
        var expected = (await File.ReadAllTextAsync(sha)).Trim().ToLowerInvariant();
        return hex == expected;
    }
}

public static class Paths
{
    public static string RepoRoot => AppContext.BaseDirectory; // For portable, data ships next to EXE
    public static string LogsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyGaming", "logs");
}
