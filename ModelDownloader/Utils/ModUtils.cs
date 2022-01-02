using IPA.Loader;
using System.Linq;
using Zenject;

namespace ModelDownloader.Utils
{
    internal class ModUtils : IInitializable
    {
        public bool CustomSabersInstalled { get; private set; }
        public bool SaberFactoryInstalled { get; private set; }
        public bool CustomNotesInstalled { get; private set; }
        public bool CustomPlatformsInstalled { get; private set; }
        public bool CustomAvatarsInstalled { get; private set; }

        public void Initialize()
        {
            // Check status of installed mods
            CustomSabersInstalled = CheckIfModInstalled("Custom Sabers");
            SaberFactoryInstalled = CheckIfModInstalled("Saber Factory");
            CustomNotesInstalled = CheckIfModInstalled("CustomNotes");
            CustomPlatformsInstalled = CheckIfModInstalled("Custom Platforms");
            CustomAvatarsInstalled = CheckIfModInstalled("Custom Avatars");
        }

        // ReSharper disable once ReplaceWithSingleCallToAny
        public static bool CheckIfModInstalled(string modName) => PluginManager.EnabledPlugins.Where(x => x.Name == modName).Any();

        //TODO: Refresh models on exit on a per-mod basis. This might be a pain without depending on the proper mods
    }
}