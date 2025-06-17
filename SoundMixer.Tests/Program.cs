using SoundMixer.Core;
using SoundMixer.Core.Objects;
using SoundMixer.Core.Extensions.JSON;

using SoundMixer.Core.NAudioPlayer;

public class Program
{
    public static void Main()
    {
        var file = System.IO.File.ReadAllText("C:\\Users\\juanf\\Downloads\\workspace.json");
        var workspace = file.Deserialize<Workspace>();

        if (workspace == null)
        {
            Console.WriteLine("Unable to load workspace");

            return;
        }

        workspace.AddPlayer(new NAudioPlayer());
        workspace.ValidateWorkspace();

        workspace.LoadMood(workspace.Scenes[0].Moods[0].Guid);
        workspace.Play();
        Thread.Sleep(1000);
        workspace.SetMuted(workspace.Scenes[0].Moods[0].SoundProperties[0].Guid, true);
        Thread.Sleep(1000);
        workspace.SetMuted(workspace.Scenes[0].Moods[0].SoundProperties[0].Guid, false);
        Thread.Sleep(1000);
        workspace.SetMuted(workspace.Scenes[0].Moods[0].SoundProperties[0].Guid, true);
        Thread.Sleep(1000);
        workspace.SetMuted(workspace.Scenes[0].Moods[0].SoundProperties[0].Guid, false);
        Console.ReadKey();
    }
}