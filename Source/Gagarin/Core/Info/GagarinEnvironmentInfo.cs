﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using RocketMan;
using Verse;

namespace Gagarin
{
    public class GagarinEnvironmentInfo
    {
        private const string CacheFolderName = "Cache";

        private const string UnifiedXmlFileName = "unified.xml";

        private const string UnifiedPatchedOriginalXmlFileName = "unified_original_dump.xml";

        private const string ModListFileName = "mods.xml";

        private const string HashFileName = "hash.xml";


        private static string _cacheFolderPath;

        public static string CacheFolderPath
        {
            get => _cacheFolderPath ?? (_cacheFolderPath = Path.Combine(RocketEnvironmentInfo.ConfigFolderPath, CacheFolderName));
        }

        private static string _unifiedXmlPath;

        public static string UnifiedXmlFilePath
        {
            get => _unifiedXmlPath ?? (_unifiedXmlPath = Path.Combine(CacheFolderPath, UnifiedXmlFileName));
        }

        private static string _unifiedPatchedOriginalXmlPath;

        public static string UnifiedPatchedOriginalXmlPath
        {
            get => _unifiedPatchedOriginalXmlPath ?? (_unifiedPatchedOriginalXmlPath = Path.Combine(CacheFolderPath, UnifiedPatchedOriginalXmlFileName));
        }


        private static string _modListPath;

        public static string ModListFilePath
        {
            get => _modListPath ?? (_modListPath = Path.Combine(CacheFolderPath, ModListFileName));
        }


        private static string _hashFilePath;

        public static string HashFilePath
        {
            get => _hashFilePath ?? (_hashFilePath = Path.Combine(CacheFolderPath, HashFileName));
        }


        public static bool CacheExists
        {
            get => File.Exists(UnifiedXmlFilePath) && File.Exists(HashFilePath) && Directory.Exists(CacheFolderPath);
        }


        private static bool _modListChangedInt;

        private static bool _modListChanged;

        public static bool ModListChanged
        {
            get
            {
                if (_modListChangedInt)
                    return _modListChanged;

                _modListChangedInt = true;
                _modListChanged = RunningModsSetUtility.Changed(
                    Context.RunningMods.Select(m => m.PackageId).ToHashSet(), ModListFilePath);
                return _modListChanged;
            }
        }
    }
}
