using System;
using System.IO;
using HarmonyLib;
using RimWorld;
using RimWorld.IO;
using UnityEngine;
using Verse;

namespace Gagarin
{
    public class ModContentLoader_Texture2D_Patch
    {
        [GagarinPatch(typeof(ModContentLoader<Texture2D>), "LoadTexture", parameters: new Type[] { typeof(VirtualFile) })]
        public static class LoadTexture_Patch
        {
            public static bool Prefix(VirtualFile file, ref Texture2D __result)
            {
                return Load(file, ref __result);
            }

            [HarmonyPriority(Priority.First)]
            private static bool Load(VirtualFile file, ref Texture2D result)
            {
                if (!file.Exists)
                {
                    return true;
                }
                string binPath = GetBinTexturePath(file);
                if (File.Exists(binPath))
                {
                    byte[] buffer = File.ReadAllBytes(binPath);
                    byte[] data = new byte[buffer.Length - (4 + 4 + 4 + 1)];
                    Array.Copy(buffer, 13, data, 0, data.Length);
                    // This is the header
                    // tex.width, tex.height, tex.format, tex.mipmapCount > 1);       
                    result = new Texture2D(
                        width: BitConverter.ToInt32(buffer, 0),
                        height: BitConverter.ToInt32(buffer, 4),
                        textureFormat: (TextureFormat)BitConverter.ToInt32(buffer, 8),
                        mipChain: BitConverter.ToBoolean(buffer, 12)
                    );
                    result.LoadRawTextureData(data);
                    result.name = Path.GetFileNameWithoutExtension(file.Name);
                    result.filterMode = FilterMode.Trilinear;
                    result.Apply();
                }
                else
                {
                    result = LoadTexture(file);
                    // This is the header
                    // tex.width, tex.height, tex.format, tex.mipmapCount > 1);                    
                    byte[] data = result.GetRawTextureData();
                    byte[] buffer = new byte[data.Length + 4 + 4 + 4 + 1];
                    BitConverter.GetBytes(result.width).CopyTo(buffer, 0);
                    BitConverter.GetBytes(result.height).CopyTo(buffer, 4);
                    BitConverter.GetBytes((int)result.format).CopyTo(buffer, 8);
                    BitConverter.GetBytes(result.mipmapCount > 1).CopyTo(buffer, 12);
                    data.CopyTo(buffer, 13);

                    File.WriteAllBytes(binPath, buffer);
                    result.Apply(updateMipmaps: false, makeNoLongerReadable: true);
                }
                return false;
            }

            private static Texture2D LoadTexture(VirtualFile file)
            {
                byte[] data;
                data = file.ReadAllBytes();
                Texture2D texture = new Texture2D(2, 2, TextureFormat.Alpha8, mipChain: true);
                texture.LoadImage(data);
                texture.Compress(highQuality: true);
                texture.name = Path.GetFileNameWithoutExtension(file.Name);
                texture.filterMode = FilterMode.Trilinear;
                texture.anisoLevel = 0;
                texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
                return texture;
            }

            private static string GetBinTexturePath(VirtualFile file)
            {
                string path = Path.Combine(GagarinEnvironmentInfo.TexturesFolderPath, "Texture_" + file.FullPath
                    .Replace('/', '_')
                    .Replace(' ', '_')
                    .Replace('$', '_')
                    .Replace('#', '_')
                    .Replace('\\', '_')
                    .Replace(':', '_')
                    .Trim() + ".bin");
                return path;
            }
        }
    }
}
