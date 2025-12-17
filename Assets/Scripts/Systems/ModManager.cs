using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Manages JSON-based mod configurations for offline modding support.
/// Players can edit ModConfig.json to customize gameplay without code changes.
/// Implements Singleton pattern for easy access throughout the game.
/// </summary>
public class ModManager : MonoBehaviour
{
    [System.Serializable]
    public class ModConfig
    {
        public string modName = "Default Configuration";
        public string author = "System";
        public string version = "1.0";
        public GameplayMods gameplayMods = new GameplayMods();
        public VisualMods visualMods = new VisualMods();
    }
    
    [System.Serializable]
    public class GameplayMods
    {
        public int[] targetPointValues = new int[] { 10, 20, 30 };
        public float playerSpeed = 5.0f;
        public float projectileSpeed = 10.0f;
        public float spawnRate = 2.0f;
        public string difficulty = "normal";
    }
    
    [System.Serializable]
    public class VisualMods
    {
        public string backgroundColor = "#1E1E1E";
        public string uiTheme = "dark";
        public bool showFpsCounter = false;
    }
    
    private static ModManager instance;
    private ModConfig currentMod;
    private string modsPath;
    private string configPath;
    
    /// <summary>
    /// Singleton instance accessor.
    /// </summary>
    public static ModManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ModManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ModManager");
                    instance = go.AddComponent<ModManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    public ModConfig CurrentMod => currentMod;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMods();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize mod system and load configuration.
    /// </summary>
    private void InitializeMods()
    {
        Debug.Log("[ModManager] Initializing mod system...");
        
        // Create Mods folder if it doesn't exist
        modsPath = Path.Combine(Application.persistentDataPath, "Mods");
        if (!Directory.Exists(modsPath))
        {
            Directory.CreateDirectory(modsPath);
            Debug.Log($"[ModManager] Created Mods folder at: {modsPath}");
        }
        
        configPath = Path.Combine(modsPath, "ModConfig.json");
        
        // Create default config if none exists
        if (!File.Exists(configPath))
        {
            CreateDefaultConfig();
        }
        
        // Load the mod config
        LoadModConfig();
    }
    
    /// <summary>
    /// Create a default mod configuration file.
    /// </summary>
    private void CreateDefaultConfig()
    {
        ModConfig defaultConfig = new ModConfig
        {
            modName = "Default Configuration",
            author = "System",
            version = "1.0",
            gameplayMods = new GameplayMods
            {
                targetPointValues = new int[] { 10, 20, 30 },
                playerSpeed = 5.0f,
                projectileSpeed = 10.0f,
                spawnRate = 2.0f,
                difficulty = "normal"
            },
            visualMods = new VisualMods
            {
                backgroundColor = "#1E1E1E",
                uiTheme = "dark",
                showFpsCounter = false
            }
        };
        
        string json = JsonUtility.ToJson(defaultConfig, true);
        File.WriteAllText(configPath, json);
        Debug.Log($"[ModManager] Created default ModConfig.json at: {configPath}");
    }
    
    /// <summary>
    /// Load mod configuration from JSON file.
    /// </summary>
    public void LoadModConfig()
    {
        if (!File.Exists(configPath))
        {
            Debug.LogError($"[ModManager] Config file not found at: {configPath}");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(configPath);
            currentMod = JsonUtility.FromJson<ModConfig>(json);
            
            if (currentMod == null)
            {
                Debug.LogError("[ModManager] Failed to parse ModConfig.json - creating default");
                CreateDefaultConfig();
                currentMod = JsonUtility.FromJson<ModConfig>(File.ReadAllText(configPath));
            }
            
            Debug.Log($"[ModManager] Loaded mod: '{currentMod.modName}' by {currentMod.author} v{currentMod.version}");
            ApplyMods();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ModManager] Error loading config: {ex.Message}");
            CreateDefaultConfig();
        }
    }
    
    /// <summary>
    /// Apply loaded mod configuration to game systems.
    /// </summary>
    private void ApplyMods()
    {
        Debug.Log("[ModManager] Applying mod configuration...");
        
        // Apply visual mods
        ApplyVisualMods();
        
        Debug.Log("[ModManager] Mods applied successfully");
    }
    
    /// <summary>
    /// Apply visual mod configuration.
    /// </summary>
    private void ApplyVisualMods()
    {
        // Apply background color
        if (Camera.main != null && ColorUtility.TryParseHtmlString(currentMod.visualMods.backgroundColor, out Color bgColor))
        {
            Camera.main.backgroundColor = bgColor;
            Debug.Log($"[ModManager] Applied background color: {currentMod.visualMods.backgroundColor}");
        }
        
        // FPS counter can be enabled here if you have a display system
        if (currentMod.visualMods.showFpsCounter)
        {
            Debug.Log("[ModManager] FPS counter enabled (implement FPS display as needed)");
        }
    }
    
    /// <summary>
    /// Get point value for a target type from mod configuration.
    /// Falls back to ContentManager if mod not available.
    /// </summary>
    public int GetTargetPointValue(int targetTypeIndex)
    {
        if (currentMod != null && currentMod.gameplayMods != null && currentMod.gameplayMods.targetPointValues != null)
        {
            if (targetTypeIndex >= 0 && targetTypeIndex < currentMod.gameplayMods.targetPointValues.Length)
            {
                return currentMod.gameplayMods.targetPointValues[targetTypeIndex];
            }
        }
        
        // Fallback to ContentManager
        if (ContentManager.Instance != null && ContentManager.Instance.IsContentLoaded())
        {
            return ContentManager.Instance.GetPointValue(targetTypeIndex);
        }
        
        return 10; // Safe default
    }
    
    /// <summary>
    /// Get spawn rate modifier from mod configuration.
    /// </summary>
    public float GetSpawnRateModifier()
    {
        return currentMod?.gameplayMods?.spawnRate ?? 2.0f;
    }
    
    /// <summary>
    /// Get player speed modifier from mod configuration.
    /// </summary>
    public float GetPlayerSpeedModifier()
    {
        return currentMod?.gameplayMods?.playerSpeed ?? 5.0f;
    }
    
    /// <summary>
    /// Get projectile speed modifier from mod configuration.
    /// </summary>
    public float GetProjectileSpeedModifier()
    {
        return currentMod?.gameplayMods?.projectileSpeed ?? 10.0f;
    }
    
    /// <summary>
    /// Get difficulty setting from mod configuration.
    /// </summary>
    public string GetDifficulty()
    {
        return currentMod?.gameplayMods?.difficulty ?? "normal";
    }
    
    /// <summary>
    /// Reload mods from disk at runtime.
    /// Useful for live editing without restarting game.
    /// </summary>
    public void ReloadMods()
    {
        Debug.Log("[ModManager] Reloading mods from disk...");
        LoadModConfig();
        Debug.Log("[ModManager] Mods reloaded successfully");
    }
    
    /// <summary>
    /// Get the full path to the mods folder.
    /// </summary>
    public string GetModsFolderPath()
    {
        return modsPath;
    }
    
    /// <summary>
    /// Reset to default configuration.
    /// </summary>
    public void ResetToDefaults()
    {
        Debug.Log("[ModManager] Resetting to default configuration...");
        CreateDefaultConfig();
        LoadModConfig();
    }
    
    /// <summary>
    /// Log current mod configuration for debugging.
    /// </summary>
    public void LogModConfiguration()
    {
        if (currentMod == null)
        {
            Debug.Log("[ModManager] No mod configuration loaded");
            return;
        }
        
        Debug.Log("=== Current Mod Configuration ===");
        Debug.Log($"Name: {currentMod.modName}");
        Debug.Log($"Author: {currentMod.author}");
        Debug.Log($"Version: {currentMod.version}");
        Debug.Log("--- Gameplay Settings ---");
        Debug.Log($"Player Speed: {currentMod.gameplayMods.playerSpeed}");
        Debug.Log($"Projectile Speed: {currentMod.gameplayMods.projectileSpeed}");
        Debug.Log($"Spawn Rate: {currentMod.gameplayMods.spawnRate}");
        Debug.Log($"Difficulty: {currentMod.gameplayMods.difficulty}");
        Debug.Log("--- Visual Settings ---");
        Debug.Log($"Background Color: {currentMod.visualMods.backgroundColor}");
        Debug.Log($"UI Theme: {currentMod.visualMods.uiTheme}");
        Debug.Log($"Show FPS: {currentMod.visualMods.showFpsCounter}");
        Debug.Log("================================");
    }
}
