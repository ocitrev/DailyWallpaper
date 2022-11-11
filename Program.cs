using System.Text.Json;
using Serilog;

static class Program
{
    static async Task<JsonDocument> BingApi()
    {
        const string apiUrl = "https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US";
        Log.Information($"Fetching {apiUrl}");

        HttpClient client = new();
        var result = await client.GetStreamAsync(apiUrl);

        return await JsonDocument.ParseAsync(result);
    }

    static async Task<string> GetLastUpdateTimestamp()
    {
        try
        {
            return await File.ReadAllTextAsync(GetTimestampPath());
        }
        catch
        {
            return string.Empty;
        }
    }

    static string GetTimestampPath()
    {
        return Path.Combine(GetAppDataFolder(), "current.txt");
    }

    static string GetWallpaperPath()
    {
        return Path.Combine(GetAppDataFolder(), "bg.jpg");
    }

    static async Task<bool> NeedWallpaperUpdate(JsonElement image, IDesktopWallpaper desktopWallpaper)
    {
        string ts = await GetLastUpdateTimestamp();
        string jsonTimestamp = image.GetProperty("startdate").GetString();
        Log.Information("Local timestamp '{LastTS}', Web timestamp '{WebTS}'", ts, jsonTimestamp);

        if (jsonTimestamp != ts)
        {
            return true;
        }

        if (desktopWallpaper.GetWallpaper(null, out string currentWallpaper) == HRESULT.S_OK)
        {
            if (string.Compare(GetWallpaperPath(), currentWallpaper, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return false;
            }

            Log.Information("Current wallpaper has the wrong path, need to download wallpaper");
        }

        return true;
    }

    static string GetAppDataFolder()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appDataFolder = Path.Combine(localAppData, "DailyWallpaper");
        Directory.CreateDirectory(appDataFolder);
        return appDataFolder;
    }

    static async Task DownloadWallpaper(Uri imageUri, string imagePath)
    {
        Log.Information("Downloading {Url}", imageUri);

        using (FileStream filestream = new(imagePath, FileMode.Create))
        {
            HttpClient client = new();
            var stream = await client.GetStreamAsync(imageUri);
            await stream.CopyToAsync(filestream);
        }
    }

    static Task DownloadWallpaper(string relativeUrl, string imagePath)
    {
        Uri imageUri = new(new Uri("https://www.bing.com"), relativeUrl);
        return DownloadWallpaper(imageUri, imagePath);
    }

    static async Task DownloadWallpaper(JsonElement image, IEnumerable<Resolution> wantedResolutions, string imagePath)
    {
        foreach (Resolution resolution in wantedResolutions)
        {
            string baseurl = image.GetProperty("urlbase").GetString();

            try
            {
                await DownloadWallpaper($"{baseurl}_{resolution}.jpg", imagePath);
                return;
            }
            catch (HttpRequestException ex)
            {
                // ignore error, will continue to next
                Log.Warning("Failed to download image: {Message}", ex.Message);
            }
        }

        string url = image.GetProperty("url").GetString();
        await DownloadWallpaper(url, imagePath);
    }

    static async Task UpdateTimestamp(string timestamp)
    {
        Log.Information("Timestamp updated to {Timestamp}", timestamp);
        await File.WriteAllTextAsync(GetTimestampPath(), timestamp);
    }

    static IEnumerable<Resolution> GetMonitorResolutions()
    {
        // sort higher resolution first
        return Resolution.GetAllMonitorResolution().Distinct().OrderByDescending(x => x);
    }

    static IDesktopWallpaper GetIDesktopWallpaper()
    {
        try
        {
            return (IDesktopWallpaper)new DesktopWallpaperClass();
        }
        catch (System.Runtime.InteropServices.COMException ex)
        {
            Log.Error(ex, "Failed to create IDesktopWallpaper");
        }

        return null;
    }

    static async Task<int> Main()
    {
        using var log = new LoggerConfiguration()
            .WriteTo.File(Path.Join(GetAppDataFolder(), "log.txt"),
                fileSizeLimitBytes: 1024 * 1024 * 10,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 5)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:w3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Logger = log;
        IDesktopWallpaper desktopWallpaper = GetIDesktopWallpaper();

        if (desktopWallpaper == null)
            return 1;

        JsonDocument doc = await BingApi();
        var firstImage = doc.RootElement.GetProperty("images")[0];

        if (await NeedWallpaperUpdate(firstImage, desktopWallpaper))
        {
            string wallpaperPath = GetWallpaperPath();
            await DownloadWallpaper(firstImage, GetMonitorResolutions(), wallpaperPath);
            desktopWallpaper.SetWallpaper(null, wallpaperPath);

            await UpdateTimestamp(firstImage.GetProperty("startdate").GetString());
        }

        return 0;
    }
}
