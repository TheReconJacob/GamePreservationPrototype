# Dedicated Server Setup Guide

## Overview
This guide explains how to set up and run a dedicated server for local network multiplayer. A dedicated server hosts the game without a local player, managing game state for all connected clients.

## Configuration Files

### ServerConfig ScriptableObject
Location: `Assets/Scripts/Systems/ServerConfig.cs`

**Settings:**
- **Headless Mode**: Disables graphics rendering (recommended for servers)
- **Server Tick Rate**: Frame rate for server (30 recommended for balance)
- **Server Port**: Network port (default 7777)
- **Max Players**: Maximum concurrent clients (2-16)
- **Verbose Logging**: Enable detailed server logs
- **Log File Path**: Location for server log output

### DedicatedServerBootstrap
Location: `Assets/Scripts/Systems/DedicatedServerBootstrap.cs`

Automatically detects server mode via:
- Command-line argument: `-server` or `--server`
- Unity's batch mode: `-batchmode -nographics`

## Unity Editor Setup

### 1. Create ServerConfig Asset
1. Right-click in Project window → Create → Game → Server Configuration
2. Name it "ServerConfig"
3. Configure settings:
   - Headless Mode: ✅ (true)
   - Server Tick Rate: 30
   - Server Port: 7777
   - Max Players: 4 (adjust as needed)
   - Verbose Logging: ✅ (true)

### 2. Add Bootstrap to Scene
1. Open GameScene
2. Create empty GameObject named "DedicatedServerBootstrap"
3. Add DedicatedServerBootstrap component
4. Assign references:
   - Server Config: Drag ServerConfig asset
   - Network Manager: Drag LocalNetworkManager GameObject

### 3. Build Settings Configuration

#### Option A: Single Build (Runtime Detection)
**Recommended for simplicity**

1. File → Build Settings
2. Add GameScene to build
3. Platform: Windows/Mac/Linux
4. Build normally
5. Run with command-line arguments for server mode

**Pros:**
- One build for both client and server
- Easy distribution
- Simpler maintenance

**Cons:**
- Larger file size (includes client assets)
- No build-time optimizations

#### Option B: Separate Server Build (Advanced)
**Recommended for production**

1. Create `Editor/ServerBuildScript.cs`:
```csharp
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ServerBuildScript
{
    [MenuItem("Build/Build Dedicated Server")]
    public static void BuildServer()
    {
        // Define DEDICATED_SERVER symbol
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines + ";DEDICATED_SERVER");
        
        // Configure build options
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/GameScene.unity" },
            locationPathName = "Builds/Server/GameServer.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.EnableHeadlessMode
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(options);
        
        // Remove DEDICATED_SERVER symbol after build
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Server build succeeded: {report.summary.totalSize} bytes");
        }
        else
        {
            Debug.LogError($"Server build failed: {report.summary.totalErrors} errors");
        }
    }
}
```

2. Build → Build Dedicated Server
3. Output: `Builds/Server/GameServer.exe`

**Pros:**
- Smaller file size (no client assets)
- Headless mode built-in
- Build-time optimizations

**Cons:**
- Requires separate builds
- More complex CI/CD

## Running the Server

### Windows

#### Command Line (Headless Mode)
```bash
cd "Builds/Server"
GameServer.exe -batchmode -nographics -server
```

#### With Logging
```bash
GameServer.exe -batchmode -nographics -server -logFile server_log.txt
```

#### Custom Port
```bash
GameServer.exe -batchmode -nographics -server
# Note: Port configured in ServerConfig asset
```

### Linux

```bash
cd Builds/Server
chmod +x GameServer.x86_64
./GameServer.x86_64 -batchmode -nographics -server
```

### macOS

```bash
cd Builds/Server
./GameServer.app/Contents/MacOS/GameServer -batchmode -nographics -server
```

## Testing Server Functionality

### Test 1: Server Startup
1. Run server with command above
2. Check console output:
   ```
   [DedicatedServer] Server mode detected via command-line argument
   [ServerConfig] Headless Mode: True
   [ServerConfig] Server Tick Rate: 30
   [LocalNetworkManager] Started as Dedicated Server
   [LocalNetworkManager] Listening on port: 7777
   ```

### Test 2: Client Connection
1. Start server: `GameServer.exe -batchmode -nographics -server`
2. Launch client build (or Unity Editor)
3. Enter server IP: `127.0.0.1` (localhost) or LAN IP
4. Click "Client" button
5. Verify connection in server logs:
   ```
   [LocalNetworkManager] Client connected: ID 1
   [LocalNetworkManager] Spawned player for Client 1
   ```

### Test 3: Multi-Client
1. Start server
2. Connect Client 1 (Unity Editor)
3. Connect Client 2 (ParrelSync clone or separate build)
4. Test gameplay synchronization:
   - Both clients should see each other's players
   - Movement syncs between clients
   - Score updates on both clients

### Test 4: Server Persistence
1. Start server
2. Connect client
3. Disconnect client
4. Reconnect client
5. Verify server still running and accepts new connections

## Server Configuration

### Port Forwarding (LAN Access)
If clients are on different networks:
1. Open router admin panel (192.168.1.1 or similar)
2. Navigate to Port Forwarding settings
3. Forward port 7777 TCP to server machine's local IP
4. Clients connect using public IP:7777

### Firewall Rules (Windows)
```powershell
# Allow inbound connections on port 7777
New-NetFirewallRule -DisplayName "Game Server" -Direction Inbound -LocalPort 7777 -Protocol TCP -Action Allow
```

### Server Performance

**Recommended Specs:**
- CPU: 2+ cores
- RAM: 2GB minimum
- Network: 10 Mbps upload per 4 players
- OS: Windows 10+, Ubuntu 20.04+, macOS 10.15+

**Performance Settings (ServerConfig):**
- Tick Rate: 30 for balanced performance
- Tick Rate: 20 for low-end hardware
- Tick Rate: 60 for competitive gameplay (higher CPU usage)

## Troubleshooting

### Server Won't Start
**Issue:** "Cannot start server - NetworkManager missing!"
**Solution:** Ensure DedicatedServerBootstrap has NetworkManager reference assigned

### Port Already in Use
**Issue:** "Failed to start Server!"
**Solution:** Change port in ServerConfig or kill process using port:
```bash
# Windows
netstat -ano | findstr :7777
taskkill /PID <pid> /F

# Linux
sudo lsof -i :7777
sudo kill <pid>
```

### Clients Can't Connect
**Issue:** Connection timeout
**Solutions:**
1. Verify server is running: Check console logs
2. Verify port: Check ServerConfig.serverPort
3. Verify firewall: Temporarily disable or add exception
4. Verify IP: Use `ipconfig` (Windows) or `ifconfig` (Linux) to confirm server IP

### High CPU Usage
**Issue:** Server using 100% CPU
**Solutions:**
1. Reduce tick rate in ServerConfig (30 → 20)
2. Disable verbose logging
3. Ensure headless mode enabled (no camera rendering)

### Memory Leaks
**Issue:** RAM usage grows over time
**Solutions:**
1. Check for undestroyed GameObjects
2. Verify NetworkObjects despawn properly
3. Monitor with Unity Profiler (attach to running server)

## Command-Line Arguments Reference

| Argument | Description | Example |
|----------|-------------|---------|
| `-batchmode` | Run without graphics | Required for headless |
| `-nographics` | Disable rendering | Improves performance |
| `-server` | Enable server mode | Custom argument |
| `-logFile <path>` | Output log file | `-logFile server.log` |
| `-port <num>` | Not implemented | Use ServerConfig instead |

## File Structure

```
GamePreservationPrototype/
├── Assets/
│   ├── Scenes/
│   │   └── GameScene.unity              # Server scene
│   ├── Scripts/
│   │   └── Systems/
│   │       ├── ServerConfig.cs          # Configuration ScriptableObject
│   │       ├── DedicatedServerBootstrap.cs  # Auto-start script
│   │       └── LocalNetworkManager.cs   # Network management
│   └── ServerConfig.asset               # Config asset (create this)
├── Builds/
│   ├── Client/                          # Client builds
│   │   └── GameClient.exe
│   └── Server/                          # Server builds
│       ├── GameServer.exe
│       ├── GameServer_Data/
│       └── server_log.txt
└── DEDICATED_SERVER_SETUP.md            # This file
```

## Next Steps

1. ✅ Create ServerConfig asset
2. ✅ Add DedicatedServerBootstrap to GameScene
3. ✅ Build server executable
4. ✅ Test with 2+ clients
5. ⬜ Configure firewall rules (if needed)
6. ⬜ Set up port forwarding (if needed)
7. ⬜ Deploy to server machine

## Preservation Benefits

- **No Cloud Dependency**: Server runs locally without internet
- **LAN-Only Operation**: Works on isolated networks
- **Manual Configuration**: All settings explicit and documented
- **Open Source**: Server code fully accessible and modifiable
- **Long-Term Viability**: Runs on any platform with Unity runtime
- **No External Services**: No authentication, matchmaking, or relay servers required

## Support

For issues or questions:
1. Check server logs: `server_log.txt`
2. Enable verbose logging in ServerConfig
3. Verify configuration in Unity Inspector
4. Test with localhost (127.0.0.1) first
5. Document error messages for troubleshooting
