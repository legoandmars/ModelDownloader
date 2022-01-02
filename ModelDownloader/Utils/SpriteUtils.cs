using BeatSaberMarkupLanguage.Animations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ModelDownloader.Utils
{
    /// <remark>
    ///  Image helpers from BeatSaverDownloader
    /// </remark>
    internal static class SpriteUtils
    {
        public static Texture2D? LoadTextureRaw(byte[] file)
        {
            if (!file.Any())
            {
                return null;
            }

            var tex2d = new Texture2D(2, 2);
            return tex2d.LoadImage(file) ? tex2d : null;
        }

        public static Texture2D? LoadTextureFromFile(string filePath)
        {
            return File.Exists(filePath) ? LoadTextureRaw(File.ReadAllBytes(filePath)) : null;
        }

        public static Sprite? LoadSpriteRaw(byte[] image, float pixelsPerUnit = 100.0f)
        {
            return LoadSpriteFromTexture(LoadTextureRaw(image), pixelsPerUnit);
        }

        public static Sprite? LoadSpriteFromTexture(Texture2D? spriteTexture, float pixelsPerUnit = 100.0f)
        {
            return spriteTexture != null ? Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit) : null;
        }

        public static async Task<AnimationControllerData> LoadSpriteRawAnimated(byte[] image, string identifier, float pixelsPerUnit = 100.0f)
        {
            return await Task.Run(() =>
            {
                TaskCompletionSource<AnimationControllerData> source = new TaskCompletionSource<AnimationControllerData>();
                AnimationLoader.Process(AnimationType.GIF, image, (tex, uvs, delays, width, height) =>
                {
                    AnimationControllerData registered = AnimationController.instance.Register(identifier, tex, uvs, delays);
                    registered.sprite.name = "ModelDownloaderAnimation" + identifier;
                    source.SetResult(registered);

                    /* Rect firstUV = uvs[0];
                     Plugin.Log.Info(firstUV.ToString());
                     // Why in the world are these textures not readable
                     // BSML PLS
                     // Some hacky magic to make it work
 
                     //Texture2D bufferTexture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
                     //Sprite.Create(tex, rect)
                     //Texture2D tempTex = new Texture2D((int)(tex.width * firstUV.width), (int)(tex.height * firstUV.height));
                     //tempTex.SetPixels(tex.GetPixels((int)firstUV.x, (int)firstUV.y, (int)(tex.width * firstUV.width), (int)(tex.height * firstUV.height)));
                     Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, (int)(tex.width * firstUV.width), (int)(tex.height * firstUV.height)), new Vector2(0, 0), PixelsPerUnit, 100, SpriteMeshType.Tight, new Vector4(0, 0, 0, 0));
                     Plugin.Log.Info(newSprite.border.sqrMagnitude.ToString());
                     registered.activeImages.Add(newSprite);
                    */
                    // absurdly long sprite creation statement because we need to define the borders
                    /*Texture2D duplicatedTexture = new Texture2D(tex.width, tex.height, tex.format, false);
                    Plugin.Log.Info(duplicatedTexture.width.ToString());
                    Graphics.CopyTexture(tex, duplicatedTexture);
                    Color[] pixels = duplicatedTexture.GetPixels();

                    // Iterate through each pixel and set them.
                    Texture2D tempTex = new Texture2D((int)(tex.width * firstUV.width), (int)(tex.height * firstUV.height), tex.format, false);
                    tempTex.Resize(256, 256);

                    Sprite newSprite = Sprite.Create(tempTex, new Rect(firstUV.x * tex.width, firstUV.y * tex.height, firstUV.width * tex.width, firstUV.height * tex.height), new Vector2(0, 0), 2f);*/
                    // source.SetResult(newSprite);
                    /*Sprite newSprite = registered.sprites[0];
                    newSprite.border.sqrMagnitude = 0;
                        source.SetResult();*/

                    // Texture2D duplicatedTexture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
                    // source.SetResult(LoadSpriteFromTexture(tex, PixelsPerUnit));
                    //source.SetResult(duplicatedTexture.EncodeToPNG());
                });

                return source.Task;
            });
        }
    }
}