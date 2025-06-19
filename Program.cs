using System.Text.Json;
using Serilog;

static class Program
{
    static async Task<JsonDocument?> BingApi()
    {
        const string apiUrl = "https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US";
        Log.Information($"Fetching {apiUrl}");

        HttpClient client = new();
        var result = await client.GetStreamAsync(apiUrl);

        try
        {
            return await JsonDocument.ParseAsync(result);
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to parse JSON");
            return null;
        }
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
        string? jsonTimestamp = image.GetProperty("startdate").GetString();
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

    static async Task DownloadWallpaper(Uri imageUri, string imagePath, CancellationToken cancelToken)
    {
        Log.Information("Downloading {Url}", imageUri);

        using (FileStream filestream = new(imagePath, FileMode.Create))
        {
            HttpClient client = new();
            var stream = await client.GetStreamAsync(imageUri, cancelToken);
            await stream.CopyToAsync(filestream, cancelToken);
        }
    }

    static Task DownloadWallpaper(string relativeUrl, string imagePath, CancellationToken cancelToken)
    {
        Uri imageUri = new(new Uri("https://www.bing.com"), relativeUrl);
        return DownloadWallpaper(imageUri, imagePath, cancelToken);
    }

    static async Task DownloadWallpaper(JsonElement image, IEnumerable<Resolution> wantedResolutions, string imagePath, CancellationToken cancelToken)
    {
        foreach (Resolution resolution in wantedResolutions)
        {
            string? baseurl = image.GetProperty("urlbase").GetString();

            try
            {
                await DownloadWallpaper($"{baseurl}_{resolution}.jpg", imagePath, cancelToken);
                return;
            }
            catch (HttpRequestException ex)
            {
                // ignore error, will continue to next
                Log.Warning("Failed to download image: {Message}", ex.Message);
            }
        }

        if (image.GetProperty("url").GetString() is string url)
        {
            await DownloadWallpaper(url, imagePath, cancelToken);
        }
        else
        {
            Log.Error("'url' field not found.");
        }
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

    static IDesktopWallpaper? GetIDesktopWallpaper()
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

    static void SetLockscreenWallpaper(string imagePath)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", false);

            if (key?.GetValue("LockScreenImagePath", null) is string lockScreenImagePath)
            {
                System.IO.File.Copy(imagePath, lockScreenImagePath, true);
                Log.Information("Updated lockscreen wallpaper");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to set lockscreen wallpaper");
        }
    }

    static async Task<int> Main()
    {
        using var log = new LoggerConfiguration()
            .WriteTo.File(Path.Join(GetAppDataFolder(), "log.txt"),
                fileSizeLimitBytes: 1024 * 1024 * 10,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 5)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:w4}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Logger = log;
        IDesktopWallpaper? desktopWallpaper = GetIDesktopWallpaper();

        if (desktopWallpaper == null)
            return 1;

        JsonDocument? doc = await BingApi();

        if (doc == null)
            return 1;

        var firstImage = doc.RootElement.GetProperty("images")[0];

        if (await NeedWallpaperUpdate(firstImage, desktopWallpaper))
        {
            string wallpaperPath = GetWallpaperPath();

            CancellationTokenSource cancelSource = new();
            cancelSource.CancelAfter(TimeSpan.FromSeconds(15));

            try
            {
                await DownloadWallpaper(firstImage, GetMonitorResolutions(), wallpaperPath, cancelSource.Token);
                desktopWallpaper.SetWallpaper(null, wallpaperPath);
                SetLockscreenWallpaper(wallpaperPath);

                if (firstImage.GetProperty("startdate").GetString() is string startdate)
                {
                    await UpdateTimestamp(startdate);
                }
                else
                {
                    Log.Warning("'startdate' field not found.");
                }
            }
            catch (TaskCanceledException)
            {
                Log.Error("Timed out");
                return 2;
            }
        }

        return 0;
    }
}
