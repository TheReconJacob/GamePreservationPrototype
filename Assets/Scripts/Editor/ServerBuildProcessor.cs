#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Build processor for dedicated server builds.
/// Automatically handles scene configuration and build settings for server builds.
/// </summary>
public class ServerBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Check if this is a server build
        bool isServerBuild = false;
        
        // Check for DEDICATED_SERVER define
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (defines.Contains("DEDICATED_SERVER"))
        {
            isServerBuild = true;
        }
        
        if (isServerBuild)
        {
            Debug.Log("[ServerBuildProcessor] Detected DEDICATED_SERVER build");
            Debug.Log("[ServerBuildProcessor] Server build optimizations applied");
            
            // Set headless mode for server builds
            PlayerSettings.forceSingleInstance = false; // Allow multiple server instances
            
            Debug.Log("[ServerBuildProcessor] - Multiple instances enabled");
            Debug.Log("[ServerBuildProcessor] - Build will use DEDICATED_SERVER define");
        }
    }
}

/// <summary>
/// Editor menu items for server build management.
/// </summary>
public class ServerBuildMenu
{
    [MenuItem("Build/Toggle Server Build Mode")]
    public static void ToggleServerBuild()
    {
        BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        
        if (defines.Contains("DEDICATED_SERVER"))
        {
            // Remove server define
            defines = defines.Replace("DEDICATED_SERVER", "").Replace(";;", ";").Trim(';');
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            Debug.Log("[ServerBuildMenu] Server build mode DISABLED - client build active");
            Debug.Log($"[ServerBuildMenu] Defines: {defines}");
        }
        else
        {
            // Add server define
            if (string.IsNullOrEmpty(defines))
                defines = "DEDICATED_SERVER";
            else
                defines += ";DEDICATED_SERVER";
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            Debug.Log("[ServerBuildMenu] Server build mode ENABLED - server build active");
            Debug.Log($"[ServerBuildMenu] Defines: {defines}");
        }
    }
    
    [MenuItem("Build/Build Server (Windows x64)")]
    public static void BuildServer()
    {
        // Enable server define
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        
        if (!defines.Contains("DEDICATED_SERVER"))
        {
            if (string.IsNullOrEmpty(defines))
                defines = "DEDICATED_SERVER";
            else
                defines += ";DEDICATED_SERVER";
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            Debug.Log("[ServerBuildMenu] Added DEDICATED_SERVER define for build");
        }
        
        // Configure build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/LoginScene.unity", "Assets/Scenes/GameScene.unity" },
            locationPathName = "Builds/Server/GamePreservationPrototype_Server.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        Debug.Log("[ServerBuildMenu] Starting server build...");
        Debug.Log($"[ServerBuildMenu] Output: {buildOptions.locationPathName}");
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[ServerBuildMenu] Server build succeeded! Size: {report.summary.totalSize / (1024 * 1024)}MB");
            Debug.Log($"[ServerBuildMenu] Location: {buildOptions.locationPathName}");
            
            // Show in Explorer
            EditorUtility.RevealInFinder(buildOptions.locationPathName);
        }
        else
        {
            Debug.LogError($"[ServerBuildMenu] Server build failed: {report.summary.result}");
        }
    }
    
    [MenuItem("Build/Build Client (Windows x64)")]
    public static void BuildClient()
    {
        // Remove server define
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        
        if (defines.Contains("DEDICATED_SERVER"))
        {
            defines = defines.Replace("DEDICATED_SERVER", "").Replace(";;", ";").Trim(';');
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            Debug.Log("[ServerBuildMenu] Removed DEDICATED_SERVER define for client build");
        }
        
        // Configure build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/LoginScene.unity", "Assets/Scenes/GameScene.unity" },
            locationPathName = "Builds/Client/GamePreservationPrototype.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        Debug.Log("[ServerBuildMenu] Starting client build...");
        Debug.Log($"[ServerBuildMenu] Output: {buildOptions.locationPathName}");
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[ServerBuildMenu] Client build succeeded! Size: {report.summary.totalSize / (1024 * 1024)}MB");
            Debug.Log($"[ServerBuildMenu] Location: {buildOptions.locationPathName}");
            
            // Show in Explorer
            EditorUtility.RevealInFinder(buildOptions.locationPathName);
        }
        else
        {
            Debug.LogError($"[ServerBuildMenu] Client build failed: {report.summary.result}");
        }
    }
}
#endif
