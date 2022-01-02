using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Animations;
using HarmonyLib;
using UnityEngine.UI;

namespace ModelDownloader.HarmonyPatches
{
    [HarmonyPatch(typeof(AnimationControllerData))]
    [HarmonyPatch("CheckFrame", MethodType.Normal)]
    internal class AnimatorControllerPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Prefix(AnimationControllerData __instance)
        {
            if (__instance.sprite == null || __instance.activeImages == null || !__instance.sprite.name.StartsWith("ModelDownloaderAnimation"))
            {
                return;
            }

            var newImageList = new List<Image>();
            for (var i = 0; i < __instance.activeImages.Count; i++)
            {
                var activeImage = __instance.activeImages[i];
                if (activeImage && __instance.sprites.Contains(activeImage.sprite))
                {
                    newImageList.Add(activeImage);
                }
            }

            __instance.activeImages = newImageList;
        }
    }
}
