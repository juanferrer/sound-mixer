using SoundMixer.Core;
using SoundMixer.Core.Objects;
using SoundMixer.Core.Extensions;

using SoundMixer.Core.NAudioPlayer;

using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

public class Program
{
    public static void Main()
    {
        var logger = new SerilogLoggerFactory(new LoggerConfiguration()
            .WriteTo.Console()
            //.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateLogger()).CreateLogger("Logger");

        var workspacePath = "C:\\Users\\juanf\\Downloads\\workspace.json";

        var workspaceJson = File.ReadAllText(workspacePath);
        var workspace = workspaceJson.Deserialize<Workspace>();

        if (workspace == null)
        {
            Console.WriteLine("Unable to load workspace");

            return;
        }

        workspace.AddPlayer(new NAudioPlayer());
        workspace.ValidateWorkspace();

        workspace.LoadMood(workspace.Scenes[0].Moods[0].Guid);
        workspace.Play();
        var soundGuid = workspace.Scenes[0].Moods[0].SoundProperties[0].Guid;
        Thread.Sleep(1000);
        workspace.SetMuted(soundGuid, true);
        Thread.Sleep(1000);
        workspace.SetMuted(soundGuid, false);
        Thread.Sleep(1000);
        workspace.SetMuted(soundGuid, true);
        Thread.Sleep(1000);
        workspace.SetMuted(soundGuid, false);
        Console.ReadKey();
    }
}