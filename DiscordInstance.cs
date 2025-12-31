using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Pussycord;

public static class DiscordInstance
{
    private const string PATH_CONFIG_FILE = @"C:\pcord\discord_canary_directory";
    private const int DEBUG_PORT = 9222;

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    private const uint WM_SETICON = 0x80;
    private const uint IMAGE_ICON = 1;
    private const uint LR_LOADFROMFILE = 0x10;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;

    private static Process _process;
    private static CdpHelper _cdp;
    private static bool _restartRequested = false;
    private static AppSettings _currentSettings;

    public static async Task RunLoop(AppSettings initialSettings)
    {
        ConfigManager.OnSettingsChanged += HandleSettingsChanged;
        _currentSettings = initialSettings;

        while (true)
        {
            _restartRequested = false;
            await StartProcess(_currentSettings);
            
            if (!_restartRequested) break;
            
            await Task.Delay(1000);
        }

        ConfigManager.OnSettingsChanged -= HandleSettingsChanged;
    }

    private static string ResolveExecutablePath()
    {
        if (File.Exists(PATH_CONFIG_FILE))
        {
            try
            {
                string path = File.ReadAllText(PATH_CONFIG_FILE).Trim();
                if (File.Exists(path)) return path;
            }
            catch { }
        }

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string baseDir = Path.Combine(localAppData, "DiscordCanary");

        if (Directory.Exists(baseDir))
        {
            var directories = Directory.GetDirectories(baseDir, "app-*");
            if (directories.Length > 0)
            {
                Array.Sort(directories);
                string latestVersion = directories[directories.Length - 1];
                string exePath = Path.Combine(latestVersion, "DiscordCanary.exe");
                if (File.Exists(exePath)) return exePath;
            }
        }

        return string.Empty;
    }

    private static string ResolveIconPath(string exePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(exePath); 
            var versionDir = Directory.GetParent(directory); 
            if (versionDir != null)
            {
                string iconPath = Path.Combine(versionDir.FullName, "app.ico");
                if (File.Exists(iconPath)) return iconPath;
            }
        }
        catch { }

        return string.Empty;
    }

    private static async Task StartProcess(AppSettings settings)
    {
        string exePath = ResolveExecutablePath();
        if (!File.Exists(exePath)) return;

        foreach (var p in Process.GetProcessesByName("DiscordCanary")) p.Kill();

        var args = new StringBuilder();
        args.Append($"--remote-debugging-port={DEBUG_PORT} ");
        
        if (settings.blockCamera)
        {
            args.Append("--disable-video-capture --disable-webcam ");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            Arguments = args.ToString()
        };

        if (settings.sandboxMode) Sandbox.Initialize();

        _process = Process.Start(startInfo);
        if (_process == null) return;

        if (settings.sandboxMode) Sandbox.Attach(_process);

        string iconPath = ResolveIconPath(exePath);
        if (!string.IsNullOrEmpty(iconPath))
        {
            _ = Task.Run(() => InjectIconLoop(_process, iconPath));
        }
        
        await Task.Delay(3000);
        await InitializeCdp(settings);

        await _process.WaitForExitAsync();
        
        if (_cdp != null)
        {
            _cdp.Dispose();
            _cdp = null;
        }
    }

    private static async Task InitializeCdp(AppSettings settings)
    {
        _cdp = new CdpHelper();
        if (await _cdp.ConnectAsync(DEBUG_PORT))
        {
            await ApplyRuntimeSettings(settings);
        }
    }

    private static void HandleSettingsChanged(AppSettings newSettings)
    {
        bool needsRestart = false;

        if (newSettings.sandboxMode != _currentSettings.sandboxMode) needsRestart = true;
        if (newSettings.blockCamera != _currentSettings.blockCamera) needsRestart = true;
        if (newSettings.blockMic != _currentSettings.blockMic) needsRestart = true;

        _currentSettings = newSettings;

        if (needsRestart)
        {
            _restartRequested = true;
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }
        else
        {
            if (_cdp != null) _ = ApplyRuntimeSettings(newSettings);
        }
    }

    private static async Task ApplyRuntimeSettings(AppSettings settings)
    {
        if (_cdp == null) return;

        var blockedUrls = new List<string>();
        if (settings.blockScience)
        {
            blockedUrls.Add("*discord.com*api*science*");
            blockedUrls.Add("*discord.com*api*telemetry*");
        }

        await _cdp.SendCommandAsync("Network.enable");
        await _cdp.SendCommandAsync("Network.setBlockedURLs", new { urls = blockedUrls });

        string camState = settings.blockCamera ? "denied" : "granted";
        string micState = settings.blockMic ? "denied" : "granted";

        await _cdp.SendCommandAsync("Browser.setPermission", new { 
            permission = new { name = "videoCapture" }, 
            setting = camState,
            origin = "https://discord.com"
        });

        await _cdp.SendCommandAsync("Browser.setPermission", new { 
            permission = new { name = "audioCapture" }, 
            setting = micState,
            origin = "https://discord.com"
        });
    }

    private static async Task InjectIconLoop(Process process, string iconPath)
    {
        IntPtr hIconSmall = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
        IntPtr hIconBig = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 32, 32, LR_LOADFROMFILE);

        while (!process.HasExited)
        {
            process.Refresh();
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                SendMessage(process.MainWindowHandle, WM_SETICON, (IntPtr)ICON_SMALL, hIconSmall);
                SendMessage(process.MainWindowHandle, WM_SETICON, (IntPtr)ICON_BIG, hIconBig);
            }
            await Task.Delay(1000); 
        }
    }
}