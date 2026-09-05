#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace Genevore.Editor
{
    public static class AndroidReleaseBuild
    {
        private const string PackageName = "com.genevore.ingram";

        [MenuItem("Genevore/Build Android Release APK")]
        public static void BuildApk()
        {
            Configure();
            string outDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");
            Directory.CreateDirectory(outDir);
            string apkPath = Path.Combine(outDir, "Genevore-Release.apk");
            var opts = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log("[Genevore Build] SUCCESS → " + apkPath);
            else
                Debug.LogError("[Genevore Build] FAILED: " + report.summary.result);
        }

        [MenuItem("Genevore/Build Android Release AAB")]
        public static void BuildAab()
        {
            Configure();
            EditorUserBuildSettings.buildAppBundle = true;
            string outDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");
            Directory.CreateDirectory(outDir);
            string aabPath = Path.Combine(outDir, "Genevore-Release.aab");
            var opts = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = aabPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            EditorUserBuildSettings.buildAppBundle = false;
            Debug.Log(report.summary.result == BuildResult.Succeeded
                ? "[Genevore Build] AAB SUCCESS → " + aabPath
                : "[Genevore Build] AAB FAILED");
        }

        private static void Configure()
        {
            PlayerSettings.companyName = "Genevore";
            PlayerSettings.productName = "Genevore";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.bundleVersion = "1.0.0";
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        }

        private static string[] GetEnabledScenes()
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var s in EditorBuildSettings.scenes)
                if (s.enabled && !string.IsNullOrEmpty(s.path)) list.Add(s.path);
            if (list.Count == 0)
            {
                foreach (var g in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
                    list.Add(AssetDatabase.GUIDToAssetPath(g));
            }
            return list.ToArray();
        }
    }
}
#endif
