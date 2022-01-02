using HarmonyLib;
using ModelDownloader.HarmonyPatches;

namespace ModelDownloader.Managers
{
    /// <summary>
    /// Apply and remove all of our Harmony patches through this class
    /// </summary>
    public class ModelDownloaderPatchManager
    {
        private static Harmony? _harmonyInstance;

        public const string InstanceId = "com.legoandmars.beatsaber.modeldownloader";

        internal static void ApplyHarmonyPatches()
        {
            _harmonyInstance ??= new Harmony(InstanceId);
            _harmonyInstance.CreateClassProcessor(typeof(AnimatorControllerPatch));
        }

        internal static void RemoveHarmonyPatches()
        {
            if (_harmonyInstance == null)
            {
                return;
            }

            _harmonyInstance.UnpatchSelf();
            _harmonyInstance = null;
        }
    }
}