using BeatSaberMarkupLanguage.Animations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ModelDownloader.Utils
{
    public static class SpriteUtils
    {
        // Image helpers from beatsaverdownloader

        public static Texture2D LoadTextureRaw(byte[] file)
        {
            if (file.Count() > 0)
            {
                Texture2D Tex2D = new Texture2D(2, 2);
                if (Tex2D.LoadImage(file))
                    return Tex2D;
            }
            return null;
        }

        public static Texture2D LoadTextureFromFile(string FilePath)
        {
            if (File.Exists(FilePath))
                return LoadTextureRaw(File.ReadAllBytes(FilePath));

            return null;
        }

        public static Sprite LoadSpriteRaw(byte[] image, float PixelsPerUnit = 100.0f)
        {
            return LoadSpriteFromTexture(LoadTextureRaw(image), PixelsPerUnit);
        }

        public static Sprite LoadSpriteFromTexture(Texture2D SpriteTexture, float PixelsPerUnit = 100.0f)
        {
            if (SpriteTexture)
                return Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
            return null;
        }

        public static async Task<AnimationControllerData> LoadSpriteRawAnimated(byte[] image, string identifier, float PixelsPerUnit = 100.0f)
        {
            return await Task.Run(() =>
            {
                TaskCompletionSource<AnimationControllerData> source = new TaskCompletionSource<AnimationControllerData>();
                AnimationLoader.Process(AnimationType.GIF, image, (Texture2D tex, Rect[] uvs, float[] delays, int width, int height) =>
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
