using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Development.Editor {
    public static class PackageExporter
    {
        [MenuItem("Tools/Export Dll Unitypackage")]
        public static void ExportDll()
        {
            var version = Environment.GetEnvironmentVariable("UNITY_PACKAGE_VERSION");

            // configure
            var root = "UMDEBridge";
            var fileName = string.IsNullOrEmpty(version) ? "UMDEBridge.dll.unitypackage" : $"UMDEBridge.dll.{version}.unitypackage";
            var exportPath = "Assets/UMDEBridge/unitypackages/" + fileName;

            var path = Path.Combine(Application.dataPath, root);
            // var assets = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            //     .Where(x => Path.GetExtension(x) == ".cs" || Path.GetExtension(x) == ".meta" || Path.GetExtension(x) == ".asmdef" || Path.GetExtension(x) == ".json")
            //     .Where(x => Path.GetFileNameWithoutExtension(x) != "_InternalVisibleTo")
            //     .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
            //     .ToArray();

            var netStandardsAsset = Directory.EnumerateFiles(Path.Combine(Application.dataPath, "Plugins/UMDEBridge/"), "*", SearchOption.AllDirectories)
                .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
                .ToArray();
        
            // assets = assets.Concat(netStandardsAsset).ToArray();
            var assets = netStandardsAsset;
        
            UnityEngine.Debug.Log("Export below files" + Environment.NewLine + string.Join(Environment.NewLine, assets));

            AssetDatabase.ExportPackage(
                assets,
                exportPath,
                ExportPackageOptions.Default);

            UnityEngine.Debug.Log("Export complete: " + Path.GetFullPath(exportPath));
        }
    
        [MenuItem("Tools/Export Demo Unitypackage")]
        public static void ExportDemo()
        {
            var version = Environment.GetEnvironmentVariable("UNITY_PACKAGE_VERSION");

            // configure
            var root = "Demo";
            var fileName = string.IsNullOrEmpty(version) ? "UMDEBridge.Demo.unitypackage" : $"UMDEBridge.Demo.{version}.unitypackage";
            var exportPath = "Assets/UMDEBridge/unitypackages/" + fileName;

            var path = Path.Combine(Application.dataPath, root);
            var assets = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                .Where(x => Path.GetExtension(x) == ".cs" || Path.GetExtension(x) == ".meta" || Path.GetExtension(x) == ".asmdef" || Path.GetExtension(x) == ".json"|| Path.GetExtension(x) == ".asset"|| Path.GetExtension(x) == ".bytes")
                .Where(x => Path.GetFileNameWithoutExtension(x) != "_InternalVisibleTo")
                .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
                .ToArray();
        
            UnityEngine.Debug.Log("Export below files" + Environment.NewLine + string.Join(Environment.NewLine, assets));

            AssetDatabase.ExportPackage(
                assets,
                exportPath,
                ExportPackageOptions.Default);

            UnityEngine.Debug.Log("Export complete: " + Path.GetFullPath(exportPath));
        }
    }
}
