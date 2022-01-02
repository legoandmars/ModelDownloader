using IPA.Loader;
using System.Linq;

namespace ModelDownloader.Utils
{
    public static class ModUtils
    {
        public static bool CustomSabersInstalled;
        public static bool SaberFactoryInstalled;
        public static bool CustomNotesInstalled;
        public static bool CustomPlatformsInstalled;
        public static bool CustomAvatarsInstalled;

        public static void CheckInstalledMods()
        {
            CustomSabersInstalled = CheckIfModInstalled("Custom Sabers");
            SaberFactoryInstalled = CheckIfModInstalled("Saber Factory");
            CustomNotesInstalled = CheckIfModInstalled("CustomNotes");
            CustomPlatformsInstalled = CheckIfModInstalled("Custom Platforms");
            CustomAvatarsInstalled = CheckIfModInstalled("Custom Avatars");
        }

        public static bool CheckIfModInstalled(string modName) => PluginManager.EnabledPlugins.Where(x => x.Name == modName).Any();

        //TODO: Refresh models on exit on a per-mod basis. This might be a pain without depending on the proper mods
    }
}