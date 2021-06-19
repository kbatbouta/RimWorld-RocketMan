using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using HarmonyLib;
using RimWorld.IO;
using Unity.Collections;
using UnityEngine;
using Verse;
using Verse.AI;
using Logger = RocketMan.Logger;

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

            private static bool Load(VirtualFile file, ref Texture2D result)
            {
                if (!file.Exists)
                {
                    return true;
                }
                string ddsPath = GetDDSTexturePath(file);
                if (File.Exists(ddsPath))
                {
                    result = LoadDDS(file, ddsPath);
                }
                else
                {
                    result = LoadTexture(file);
                    Color[] pixels = result.GetPixels();

                    BcEncoder encoder = new BcEncoder();
                    byte[] data = new byte[pixels.Length * 4];

                    int k = 0;
                    for (int j = 0; j < result.width; j++)
                    {
                        for (int i = 0; i < result.height; i++)
                        {
                            Color color = result.GetPixel(i, j);
                            data[k] = Convert.ToByte((int)(color.r * 255));
                            data[k + 1] = Convert.ToByte((int)(color.g * 255));
                            data[k + 2] = Convert.ToByte((int)(color.b * 255));
                            data[k + 3] = Convert.ToByte((int)(color.a * 255));
                            k += 4;
                        }
                    }

                    encoder.OutputOptions.quality = CompressionQuality.Balanced;
                    encoder.OutputOptions.format = CompressionFormat.BC3;
                    encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

                    using (FileStream fs = File.OpenWrite(ddsPath))
                    {
                        encoder.Encode(data, result.width, result.height, fs);
                    }

                    result.Apply(updateMipmaps: true, makeNoLongerReadable: true);
                }
                return false;
            }

            private static Texture2D LoadTexture(VirtualFile file)
            {
                Texture2D texture = null;

                byte[] data;
                data = file.ReadAllBytes();
                texture = new Texture2D(2, 2, TextureFormat.Alpha8, mipChain: true);
                texture.LoadImage(data);
                texture.Compress(highQuality: true);
                texture.name = Path.GetFileNameWithoutExtension(file.Name);
                texture.filterMode = FilterMode.Trilinear;
                texture.anisoLevel = 8;
                return texture;
            }

            private static Texture2D LoadDDS(VirtualFile file, string ddsPath)
            {
                Texture2D texture = TextureUtility.Load(ddsPath);
                texture.name = Path.GetFileNameWithoutExtension(file.Name);
                texture.filterMode = FilterMode.Trilinear;
                texture.Apply(true, true);
                return texture;
            }

            private static string GetDDSTexturePath(VirtualFile file)
            {
                string path = Path.Combine(GagarinEnvironmentInfo.TexturesFolderPath, "Texture_" + file.FullPath
                    .Replace('/', '_')
                    .Replace(' ', '_')
                    .Replace('$', '_')
                    .Replace('#', '_')
                    .Replace('\\', '_')
                    .Replace(':', '_')
                    .Trim() + ".dds");
                return path;
            }
        }
    }
}
