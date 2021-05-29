using BeatSaberMarkupLanguage.Animations;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ModelDownloader.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(AnimationControllerData))]
    [HarmonyPatch("CheckFrame", MethodType.Normal)]

    internal class AnimatorControllerPatch
    {
        private static void Prefix(AnimationControllerData __instance)
        {
            if (__instance != null && __instance.sprite != null && __instance.activeImages != null && __instance.sprite.name.StartsWith("ModelDownloaderAnimation"))
            {
                var newImageList = new List<Image>();
                for (int i = 0; i < __instance.activeImages.Count; i++)
                {
                    var activeImage = __instance.activeImages[i];
                    if (activeImage && __instance.sprites.Contains(activeImage.sprite)) newImageList.Add(activeImage);
                }
                __instance.activeImages = newImageList;
            }
        }
    }
}
