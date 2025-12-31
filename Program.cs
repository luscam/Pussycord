using System.Diagnostics;

namespace Pussycord;

class Program
{
    static async Task Main(string[] args)
    {
        EnsureDirectories();
        
        var cts = new CancellationTokenSource();
        var serverTask = LocalServer.StartAsync(cts.Token);
        
        var settings = ConfigManager.Load();

        try
        {
            await DiscordInstance.RunLoop(settings);
        }
        catch (Exception)
        {
        }
        finally
        {
            cts.Cancel();
            LocalServer.Stop();
        }
    }

    static void EnsureDirectories()
    {
        string path = @"C:\pcord";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }
}