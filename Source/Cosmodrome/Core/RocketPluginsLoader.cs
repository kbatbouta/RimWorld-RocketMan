using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.Assertions;
using Verse;

namespace RocketMan
{
    public sealed class RocketPluginsLoader
    {
        private readonly string[] ApprovedAssemblies = new string[]
        {
            "Gagarin.dll",
            "Soyuz.dll",
            "Proton.dll"
        };

        public RocketPluginsLoader()
        {
            if (!Directory.Exists(Path.Combine(RocketEnvironmentInfo.CustomConfigFolderPath, "Logs")))
                Directory.CreateDirectory(Path.Combine(RocketEnvironmentInfo.CustomConfigFolderPath, "Logs"));
            LogWrite("ROCKETMAN: Started!", clear: true);
        }

        public IEnumerable<Assembly> LoadAll()
        {
            List<Assembly> assemblies = new List<Assembly>();
            if (RocketEnvironmentInfo.IsDevEnv)
            {
                Log.Message($"ROCKETMAN: Dev enviroment detected! Loading experimental plugins!");

                assemblies.AddRange(
                    LoadDirectory(RocketEnvironmentInfo.ExperimentalPluginsFolderPath)
                );
            }
            assemblies.AddRange(
                LoadDirectory(RocketEnvironmentInfo.PluginsFolderPath)
            );
            return assemblies;
        }

        private IEnumerable<Assembly> LoadDirectory(string directoryPath)
        {
            foreach (string filePath in Directory.GetFiles(directoryPath, "*.dll"))
            {
                string fileName = Path.GetFileName(filePath);
                string assemblyName = Path.GetFileNameWithoutExtension(filePath);
                if (!ApprovedAssemblies.Contains(fileName))
                {
                    continue;
                }
                LogWrite($"ROCKETMAN: Found assembly with name of " +
                    $"<color=red>{assemblyName}</color> and file name of " +
                    $"<color=red>{fileName}</color>");
                string symbolStorePath = filePath.Substring(0, filePath.Length - 3) + "pdb";
                if (RocketEnvironmentInfo.IsDevEnv && File.Exists(symbolStorePath))
                {
                    yield return LoadAssembly_AssemblyResolve(assemblyName, filePath, symbolStorePath);
                }
                else
                {
                    yield return LoadAssembly_AssemblyResolve(assemblyName, filePath);
                }
            }
        }

        private Assembly LoadAssembly_AssemblyResolve(string assemblyName, string assemblyPath, string symbolsPath = null)
        {
            try
            {
                if (assemblyPath.Contains(".resources"))
                {
                    return null;
                }
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
                if (assembly != null)
                {
                    return assembly;
                }
                byte[] rawAssembly = ReadAllBytes(assemblyPath);
                byte[] rawSymbolStore = symbolsPath != null ? ReadAllBytes(assemblyPath) : null;
                assembly = rawSymbolStore != null && RocketEnvironmentInfo.IsDevEnv ?
                                 AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore) :
                                 AppDomain.CurrentDomain.Load(rawAssembly);
                LogWrite($"ROCKETMAN: Resolved assembly {assembly?.GetName().FullName} and symbols state is {rawSymbolStore != null && RocketEnvironmentInfo.IsDevEnv}");
                LogWrite($"ROCKETMAN: Create resolve event handler");
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((sender, args) =>
                {
                    LogWrite($"ROCKETMAN: Assembly resolve called!");
                    LogWrite($"ROCKETMAN: Assembly resolve event. requesting: {args.RequestingAssembly.GetName().FullName }, args:{args.Name}");
                    if (args.Name == assembly.GetName().FullName)
                    {
                        return assembly;
                    }
                    return null;
                });
                assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
                LogWrite($"ROCKETMAN: Assembly is currently [valid={assembly != null }] and Named {assembly.FullName}");
                if (assembly == null)
                {
                    LogWrite($"ROCKETMAN: Preparing to throw new Exception!");
                    LogWriteAssemblies();
                    throw new Exception($"ROCKETMAN: Loaded assembly {assemblyName} not in the " +
                        $"<color=red>current app domain</color> and path fo {assemblyPath}");
                }
                return assembly;
            }
            catch (Exception er)
            {
                LogWriteAssemblies();
                LogWrite($"ROCKETMAN: ERROR loading assemlby {assemblyName} with error {er}");
                return null;
            }
        }

        private void LogWriteAssemblies()
        {
            int index = 0;
            string report = "ROCKETMAN: Assemblies report\n";
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.FullName.Contains("UnityEngine") || a.FullName.Contains("System"))
                    continue;
                report += $"{index++}. {a.FullName}\t{a.GetName().Name}\n";
            }
            LogWrite(report);
        }

        private void LogWrite(string message, bool clear = false)
        {
            string logPath = Path.Combine(RocketEnvironmentInfo.CustomConfigFolderPath, "Logs/pluginloader.log");
            if (clear && File.Exists(logPath))
                File.Delete(logPath);
            File.WriteAllText(logPath, File.Exists(logPath) ? (File.ReadAllText(logPath) + message + "\n") : message + "\n");
        }

        private byte[] ReadAllBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
    }
}
