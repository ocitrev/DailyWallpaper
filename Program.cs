using System.Text.Json;

static class Program
{

    static async Task<JsonDocument> BingApi()
    {
        const string apiUrl = "https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US";
        
        Console.WriteLine($"Fetching {apiUrl}");
        HttpClient client = new();
        var result = await client.GetStreamAsync(apiUrl);
        
        Console.WriteLine("Parsing JSON response");
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

        if (image.GetProperty("startdate").GetString() != ts)
        {
            Console.WriteLine("Timestamp changed, need to download wallpaper");
            return true;
        }

        if (desktopWallpaper.GetWallpaper(null, out string currentWallpaper) == HRESULT.S_OK)
        {
            if (string.Compare(GetWallpaperPath(), currentWallpaper, StringComparison.InvariantCultureIgnoreCase) == 0)
                return false;
            
            Console.WriteLine("Current wallpaper has the wrong path, need to download wallpaper");
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
        using (FileStream filestream = new(imagePath, FileMode.Create))
        {
            HttpClient client = new();
            var stream = await client.GetStreamAsync(imageUri);
            await stream.CopyToAsync(filestream);
        }
    }

    static async Task DownloadWallpaper(string relativeUrl, string imagePath)
    {
        Uri imageUri = new(new Uri("https://www.bing.com"), relativeUrl);
        await DownloadWallpaper(imageUri, imagePath);
    }

    static async Task DownloadWallpaper(JsonElement image, string[] wantedResolutions, string imagePath)
    {
        foreach (string resolution in wantedResolutions)
        {
            string baseurl = image.GetProperty("urlbase").GetString();

            try
            {
                await DownloadWallpaper(baseurl + resolution, imagePath);
                return;
            }
            catch (HttpRequestException)
            {
                continue;
            }
        }

        string url = image.GetProperty("url").GetString();
        await DownloadWallpaper(url, imagePath);
    }

    static async Task UpdateTimestamp(string timestamp)
    {
        await File.WriteAllTextAsync(GetTimestampPath(), timestamp);
    }

    static async Task<int> Main()
    {
        JsonDocument doc = await BingApi();
        var firstImage = doc.RootElement.GetProperty("images")[0];
        IDesktopWallpaper desktopWallpaper = (IDesktopWallpaper)new DesktopWallpaperClass();

        if (await NeedWallpaperUpdate(firstImage, desktopWallpaper))
        {
            int primaryX = Native.GetSystemMetrics(SM.CXSCREEN);
            int primaryY = Native.GetSystemMetrics(SM.CYSCREEN);
            string wallpaperPath = GetWallpaperPath();
            await DownloadWallpaper(firstImage, new[]{$"_{primaryX}x{primaryY}", "_1920x1200.jpg"}, wallpaperPath);
            desktopWallpaper.SetWallpaper(null, wallpaperPath);

            Console.WriteLine("Updating timestamp file");
            await UpdateTimestamp(firstImage.GetProperty("startdate").GetString());
        }

        return 0;
    }
}
