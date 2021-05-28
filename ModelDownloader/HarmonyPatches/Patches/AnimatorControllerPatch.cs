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
            if (__instance.sprite.name.StartsWith("ModelDownloaderAnimation"))
            {
                var newImageList = new List<Image>();

                foreach (var activeImage in __instance.activeImages)
                {
                    if (__instance.sprites.Contains(activeImage.sprite)) newImageList.Add(activeImage);
                }
                __instance.activeImages = newImageList;
            }
        }
    }
}
