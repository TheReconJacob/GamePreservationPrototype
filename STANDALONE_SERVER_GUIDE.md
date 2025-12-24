# Standalone Dedicated Server Guide

## What is This?

This guide explains how to run the game as a **dedicated server** - a version that runs without graphics and allows multiple players to connect and play together. The server manages the game state while clients connect to play.

## Requirements

- Built game executable: `GamePreservationPrototype.exe`
- PowerShell (comes with Windows)
- Port 7777 available (not blocked by firewall)

---

## Quick Start - Running the Server

### Option 1: Headless Server (No Graphics - Production Mode)

**Best for:** Running on a dedicated machine, minimal resource usage

1. Open PowerShell
2. Navigate to your build folder:
   ```powershell
   cd "C:\Users\JVT05\OneDrive - Sky\Documents\GamePreservationPrototype\Build"
   ```
3. Start the server:
   ```powershell
   .\GamePreservationPrototype.exe -batchmode -nographics -server -logFile server_log.txt
   ```

**Server is now running!** The console returns to normal prompt, but the server is active in the background.

**To check if it's running:**
```powershell
Get-Process | Where-Object {$_.ProcessName -like "*GamePreservation*"}
```

**To stop the server:**
```powershell
Stop-Process -Name "GamePreservationPrototype" -Force
```

### Option 2: Windowed Server (With Graphics - Testing Mode)

**Best for:** Local testing, seeing what's happening

1. Open PowerShell
2. Navigate to your build folder:
   ```powershell
   cd "C:\Users\JVT05\OneDrive - Sky\Documents\GamePreservationPrototype\Build"
   ```
3. Start the server:
   ```powershell
   .\GamePreservationPrototype.exe -server
   ```

A game window opens showing the server is running.

**To stop:** Close the window normally (X button or Alt+F4)

---

## Checking Server Logs

### Real-Time Logs (Option 2 - Windowed Mode)

Logs appear in the game window console as events happen.

### Log File (Option 1 - Headless Mode)

The server writes logs to `server_log.txt` in the same folder as the executable.

**View the log file:**
```powershell
Get-Content .\server_log.txt
```

**View last 20 lines (most recent):**
```powershell
Get-Content .\server_log.txt -Tail 20
```

**Watch logs in real-time:**
```powershell
Get-Content .\server_log.txt -Wait -Tail 20
```
(Press Ctrl+C to stop watching)

### What to Look For in Logs

**Server started successfully:**
```
[ServerSceneRedirect] Server mode detected in LoginScene - switching to GameScene
[DedicatedServer] Server mode detected via command-line argument
[ServerConfig] Server configuration applied
[LocalNetworkManager] Started as Dedicated Server
[LocalNetworkManager] Listening on port: 7777
```

**Client connected:**
```
[LocalNetworkManager] Client 1 connected!
[LocalNetworkManager] Total connected clients: 1
[LocalNetworkManager] Spawned player for Client 1 at (0.00, 1.00, 0.00)
```

**Client disconnected:**
```
[LocalNetworkManager] Client 1 disconnected!
[LocalNetworkManager] Remaining clients: 0
```

**Error - Port already in use:**
```
Socket creation failed... Address in use
Server failed to bind
```
**Fix:** Another program is using port 7777. Kill it:
```powershell
netstat -ano | findstr :7777
Stop-Process -Id [PID from above] -Force
```

---

## Connecting Clients to the Server

### On the Same Computer (Localhost Testing)

1. **Make sure the server is running** (see above)

2. **Launch the game normally** (double-click `GamePreservationPrototype.exe` or run without arguments):
   ```powershell
   .\GamePreservationPrototype.exe
   ```

3. **In the game window:**
   - Enter IP: `127.0.0.1` (localhost)
   - Click **Client** button

4. **You should see:**
   - Game loads with your player spawned
   - Server logs show: `[LocalNetworkManager] Client X connected!`

5. **Test with multiple clients:**
   - Run `GamePreservationPrototype.exe` again (opens second window)
   - Enter IP `127.0.0.1`, click Client
   - Both clients should see each other's players

### On Different Computers (LAN/Network)

**On the Server Computer:**

1. **Find your local IP address:**
   ```powershell
   ipconfig
   ```
   Look for "IPv4 Address" under your active network adapter (e.g., `192.168.1.100`)

2. **Allow port 7777 through Windows Firewall:**
   ```powershell
   New-NetFirewallRule -DisplayName "Game Server" -Direction Inbound -LocalPort 7777 -Protocol UDP -Action Allow
   ```

3. **Start the server** (see Quick Start section above)

**On Client Computers:**

1. Launch `GamePreservationPrototype.exe`
2. Enter the server's IP address (e.g., `192.168.1.100`)
3. Click **Client** button

---

## Testing Server Functionality

### Test 1: Server Startup
1. Start server (headless or windowed)
2. Check logs for: `[LocalNetworkManager] Listening on port: 7777`
3. ✅ **Pass:** Server starts without errors

### Test 2: Single Client Connection
1. Server running
2. Start client, enter `127.0.0.1`, click Client
3. Check server logs for: `[LocalNetworkManager] Client 1 connected!`
4. ✅ **Pass:** Client connects and spawns in game

### Test 3: Multiple Clients
1. Server running
2. Connect Client 1 (see Test 2)
3. Connect Client 2 (same steps)
4. Check server logs for: `[LocalNetworkManager] Client 2 connected!`
5. In Client 1, press WASD to move
6. ✅ **Pass:** Client 2 sees Client 1's player moving

### Test 4: Gameplay Synchronization
1. Server + 2 clients connected
2. **Movement Test:** Move in Client 1 → visible in Client 2
3. **Shooting Test:** Press Space in Client 1 → Client 2 sees projectiles
4. **Target Test:** Shoot target in Client 1 → disappears in both clients
5. **Score Test:** Both clients show same score
6. ✅ **Pass:** All gameplay syncs correctly

### Test 5: Disconnect/Reconnect
1. Server + Client 1 connected
2. Close Client 1 window
3. Check server logs for: `[LocalNetworkManager] Client 1 disconnected!`
4. Start Client 1 again and reconnect
5. ✅ **Pass:** Client can reconnect after disconnecting

### Test 6: Server Persistence
1. Server + 2 clients connected
2. One client disconnects
3. Remaining client continues playing normally
4. ✅ **Pass:** Server keeps running for remaining clients

---

## Command Reference

### Starting the Server

| Command | Description | Use Case |
|---------|-------------|----------|
| `.\GamePreservationPrototype.exe -batchmode -nographics -server` | Headless server (no window) | Production, dedicated machine |
| `.\GamePreservationPrototype.exe -batchmode -nographics -server -logFile server.txt` | Headless with log file | Production with persistent logs |
| `.\GamePreservationPrototype.exe -server` | Windowed server (with graphics) | Local testing, debugging |

### Managing the Server

| Command | Description |
|---------|-------------|
| `Get-Process \| Where-Object {$_.ProcessName -like "*GamePreservation*"}` | Check if server is running |
| `Stop-Process -Name "GamePreservationPrototype" -Force` | Stop the server |
| `netstat -ano \| findstr :7777` | Check if port 7777 is in use |
| `Get-Content .\server_log.txt -Tail 20` | View last 20 log lines |
| `Get-Content .\server_log.txt -Wait -Tail 20` | Watch logs in real-time |

### Starting a Client

| Command | Description |
|---------|-------------|
| `.\GamePreservationPrototype.exe` | Normal client (opens game window) |
| Double-click `GamePreservationPrototype.exe` | Same as above (from File Explorer) |

---

## Troubleshooting

### Server won't start - Port 7777 already in use

**Symptom:** Logs show `Address in use` or `Server failed to bind`

**Fix:**
```powershell
# Find what's using port 7777
netstat -ano | findstr :7777

# Kill the process (replace XXXX with PID from above)
Stop-Process -Id XXXX -Force

# Try starting server again
```

### Client can't connect

**Symptom:** Client stuck on "Connecting..." or timeout

**Check:**
1. Server is actually running: `Get-Process | Where-Object {$_.ProcessName -like "*GamePreservation*"}`
2. Firewall allows port 7777 (see "On Different Computers" section)
3. IP address is correct (use `ipconfig` on server to verify)
4. Both server and client are on the same network

### No log output appearing

**Symptom:** `server_log.txt` is empty or doesn't exist

**Fix:**
- Make sure you're running with `-logFile server_log.txt` argument
- Check the file location - it's created in the same folder as the executable
- Wait a few seconds after server starts for logs to flush to disk

### Can't stop headless server with Ctrl+C

**This is normal behavior.** Headless servers detach from the terminal.

**Use:** `Stop-Process -Name "GamePreservationPrototype" -Force`

Or open Task Manager and end the GamePreservationPrototype.exe process.

---

## Server Configuration

Server settings are controlled by the `ServerConfig` asset in Unity:
- **Headless Mode**: Disables graphics rendering
- **Server Tick Rate**: 30 FPS (lower = less CPU usage)
- **Server Port**: 7777 (default)
- **Max Players**: 4 (can be increased)
- **Verbose Logging**: Enabled (all events logged)

To change these, modify the ServerConfig asset in Unity and rebuild.

---

## Production Deployment Tips

### Running on a Dedicated Server Machine

1. Copy entire `Build` folder to server machine
2. Install Visual C++ Redistributable (if needed)
3. Open port 7777 in firewall
4. Run headless: `.\GamePreservationPrototype.exe -batchmode -nographics -server -logFile server.txt`

### Running as a Background Service

Use a tool like **NSSM** (Non-Sucking Service Manager):

```powershell
# Install NSSM (via Chocolatey)
choco install nssm

# Create service
nssm install GameServer "C:\Path\To\GamePreservationPrototype.exe" "-batchmode -nographics -server -logFile server.txt"
nssm start GameServer
```

### Monitoring Server Health

Create a simple PowerShell script (`check_server.ps1`):

```powershell
$process = Get-Process -Name "GamePreservationPrototype" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "Server is running (PID: $($process.Id))"
} else {
    Write-Host "Server is NOT running!"
}
```

Run periodically with Task Scheduler.

---

## FAQ

**Q: Can I run multiple servers on the same machine?**  
A: Yes, but each needs a different port. Modify ServerConfig to use different ports (e.g., 7777, 7778, 7779).

**Q: How many players can connect?**  
A: Default is 4. Increase `maxPlayers` in ServerConfig asset and rebuild.

**Q: Does the server need graphics hardware?**  
A: No, headless mode works on machines without GPUs.

**Q: Can clients connect over the internet?**  
A: Yes, but you need to port forward 7777 (UDP) on your router to the server's local IP.

**Q: What happens if the server crashes?**  
A: All connected clients disconnect. Check `server_log.txt` for error details. Use a service manager (NSSM) to auto-restart.

**Q: Can I run this on Linux/Mac?**  
A: Yes, build for those platforms in Unity. Commands are similar but use `./` instead of `.\` and bash instead of PowerShell.

---

## Summary Checklist

- [ ] Server starts successfully and listens on port 7777
- [ ] Clients can connect to `127.0.0.1` (localhost)
- [ ] Multiple clients see each other's players
- [ ] Movement syncs between clients
- [ ] Shooting and targets sync between clients
- [ ] Scores sync between clients
- [ ] Server logs show all connection events
- [ ] Server can be stopped cleanly

If all checked, **standalone server functionality is working correctly!** ✅
