# Lobby System Usage Guide

## Overview
This guide explains how to use the lobby system for local multiplayer gameplay. The lobby allows the host to wait for players to connect before starting the game.

---

## How the Lobby Works

### Connection Flow
1. **Host starts** → Lobby appears immediately
2. **Clients connect** → Added to lobby list (no spawning yet)
3. **Host clicks "Start Game"** → All players spawn simultaneously
4. **Lobby hides** → Game begins
5. **Late joiners** → Spawn immediately (game already started)

### Key Features
- **True Lobby**: Players don't spawn until host starts the game
- **Host Control**: Only host can see and click "Start Game" button
- **Player List**: Shows all connected players (Host, Client 1, Client 2, etc.)
- **Status Display**: Shows your role and total player count
- **Late Join**: Players can join after game starts (spawn immediately)

---

## Testing as Host

### Step 1: Start Host
1. Click **Play** in Unity Editor
2. Click **Start Host** button in NetworkUI
3. **Expected**: Lobby appears showing "Host (Client 0)"
4. **Expected**: "Start Game" button is visible (green)
5. **Expected**: Status shows "Host - 1 player(s) connected"

### Step 2: Wait for Clients
1. Keep Unity Editor running as host
2. Launch client builds (see Client Testing below)
3. **Expected**: Lobby updates as each client connects
4. **Expected**: Player list shows "Host (Client 0)", "Client 1", "Client 2", etc.
5. **Expected**: Status text updates player count

### Step 3: Start the Game
1. When ready, click **Start Game** button
2. **Expected**: Lobby disappears on all clients
3. **Expected**: All players spawn in the game world
4. **Expected**: Gameplay begins immediately

### What You Should See
- Lobby overlay covering the entire screen (black with 80% opacity)
- Player list in center showing connected clients
- Green "Start Game" button at bottom center
- Status text at bottom showing role and player count
- Players spawn only after clicking Start Game

---

## Testing as Client

### Step 1: Start Client
1. Run a client build (or use ParrelSync clone)
2. Enter host IP address (127.0.0.1 for localhost)
3. Click **Start Client** button
4. **Expected**: Lobby appears showing your client number
5. **Expected**: Status shows "Client - X player(s) connected"

### Step 2: Wait in Lobby
1. **Expected**: You see the player list updating
2. **Expected**: You see "Host (Client 0)" at the top
3. **Expected**: You see your entry (e.g., "Client 1")
4. **Expected**: "Start Game" button is NOT visible (host-only)
5. **Expected**: You cannot start the game

### Step 3: Game Starts
1. When host clicks Start Game:
2. **Expected**: Lobby disappears automatically
3. **Expected**: You spawn in the game world
4. **Expected**: Gameplay begins

### What You Should See
- Lobby overlay covering the entire screen
- Player list showing all connected clients
- Your client number (e.g., "Client 1", "Client 2")
- NO "Start Game" button (host controls this)
- Status text showing "Client - X player(s) connected"

---

## Testing with Multiple Clients

### Option 1: ParrelSync (Recommended for Development)
1. Open ParrelSync window (Tools → ParrelSync)
2. Create clones if needed
3. Open clone projects
4. In Unity Editor (original): Click Play → Start Host
5. In Clone 1: Click Play → Start Client → Connect
6. In Clone 2: Click Play → Start Client → Connect
7. All clones see lobby with player list
8. Original (host) clicks Start Game
9. All instances spawn players and hide lobby

### Option 2: Built Clients
1. Build client using **Build → Build Client**
2. In Unity Editor: Click Play → Start Host
3. Launch `Builds\Client\GamePreservationPrototype.exe`
4. In client exe: Enter 127.0.0.1 → Start Client
5. Launch another client exe (if testing multiple clients)
6. In Unity Editor: Click Start Game when ready
7. All clients spawn players

### Option 3: Network Testing (Different Machines)
1. On Host PC: Get local IP address (run `ipconfig` in PowerShell)
2. In Unity Editor: Click Play → Start Host
3. On Client PC: Run client build
4. In client: Enter host's IP address → Start Client
5. Host sees client connect in lobby
6. Host clicks Start Game when ready

---

## Expected Behavior Checklist

### Host Checklist
- ✅ Lobby appears immediately after clicking Start Host
- ✅ "Start Game" button is visible (green, bottom center)
- ✅ Player list shows "Host (Client 0)"
- ✅ Status shows "Host - 1 player(s) connected"
- ✅ Player list updates as clients connect
- ✅ Player count increases in status text
- ✅ Clicking Start Game spawns all players
- ✅ Lobby disappears after clicking Start Game

### Client Checklist
- ✅ Lobby appears after successful connection
- ✅ Player list shows "Host (Client 0)" and your client number
- ✅ Status shows "Client - X player(s) connected"
- ✅ "Start Game" button is NOT visible
- ✅ Player list updates as other clients connect
- ✅ Lobby disappears when host starts game
- ✅ You spawn in game world when game starts

### Gameplay Checklist
- ✅ Players spawn at spawn points when game starts
- ✅ Movement works after spawning
- ✅ Camera follows local player
- ✅ Other players visible and synchronized
- ✅ Shooting and scoring work
- ✅ Targets spawn and synchronize

---

## Late Join Testing

### How to Test Late Join
1. Start host and 1-2 clients
2. Host clicks Start Game
3. Game begins, lobby hidden
4. Launch another client build
5. New client connects
6. **Expected**: New client spawns immediately (no lobby)
7. **Expected**: Existing players see new player spawn
8. **Expected**: New player can play immediately

---

## Common Issues and Solutions

### Issue: Lobby doesn't appear after clicking Start Host/Client
**Cause**: LobbyCanvas reference not assigned in LocalNetworkManager
**Solution**: 
1. Select NetworkManager GameObject
2. In LocalNetworkManager component
3. Drag LobbyCanvas to "Lobby UI" field

### Issue: Player list is empty
**Cause**: PlayerListContainer or PlayerEntryPrefab not assigned in LobbyUI
**Solution**:
1. Select LobbyCanvas
2. In LobbyUI component
3. Assign Player List Container (PlayerListContainer GameObject)
4. Assign Player Entry Prefab (from Assets/Prefabs/)

### Issue: All entries show "Client 0"
**Cause**: ClientIDText not assigned in PlayerEntry prefab
**Solution**:
1. Open PlayerEntry prefab (Assets/Prefabs/)
2. In LobbyPlayerEntry component
3. Drag ClientIDText to "Client Id Text" field
4. Save prefab

### Issue: Start Game button doesn't work
**Cause**: NetworkManager reference missing in LobbyUI or LobbySync not in scene
**Solution**:
1. Select LobbyCanvas
2. In LobbyUI component, assign Network Manager
3. Verify LobbySync GameObject exists in scene (root level)
4. Verify LobbySync has NetworkObject and LobbySync script

### Issue: Lobby doesn't hide on clients when host starts game
**Cause**: LobbySync missing or not configured
**Solution**:
1. Create empty GameObject at root level, name "LobbySync"
2. Add NetworkObject component
3. Add LobbySync script component
4. Ensure it's NOT a child of NetworkManager

### Issue: Players spawn immediately instead of waiting for Start Game
**Cause**: Code reverted to old behavior
**Solution**: Verify LocalNetworkManager has:
- `gameStarted` flag
- `pendingClients` list
- OnClientConnected checks `if (gameStarted)` before spawning

### Issue: Lobby appears in dedicated server builds
**Cause**: LobbyUI not excluded from server builds
**Solution**: Add `#if !DEDICATED_SERVER` guard around LobbyUI class (or leave as-is - null checks handle this gracefully)

---

## Performance Notes

### Spawning Delay
- Players spawn 0.1 seconds after HideLobbyAndStartGame() is called
- This ensures network is fully ready before spawning
- Prevents race conditions with network synchronization

### Player List Updates
- Updates whenever a client connects or disconnects
- Uses dynamic instantiation (not pooling)
- Efficient for small player counts (2-8 players)

### Network Traffic
- ClientRpc to hide lobby is a single network message
- Minimal bandwidth usage
- No continuous synchronization needed

---

## Advanced Usage

### Customizing Lobby Appearance
1. Select LobbyPanel in Hierarchy
2. Modify Image color/opacity
3. Adjust Rect Transform size/position
4. Modify TitleText font size/color

### Changing Player Entry Display
1. Open PlayerEntry prefab
2. Modify ClientIDText font size/alignment
3. Change panel background color
4. Adjust Rect Transform dimensions

### Adding Countdown Timer
1. Add timer field to LobbyUI.cs
2. Create coroutine for countdown
3. Update status text with remaining time
4. Auto-start when countdown reaches 0

### Adding Player Limit
1. Add maxPlayers field to LocalNetworkManager
2. In OnClientConnected, check count vs limit
3. Reject connection if full (modify ApprovalCheck)
4. Update lobby UI to show "X/Y players"

---

## Summary

The lobby system provides a true multiplayer lobby experience:
- ✅ Players wait in lobby (no spawning)
- ✅ Host controls game start
- ✅ All players spawn simultaneously
- ✅ Network synchronized across all clients
- ✅ Late join supported
- ✅ Simple and functional

**Estimated setup time**: 15-20 minutes in Unity Editor
**Estimated testing time**: 5-10 minutes per test session

For Unity Editor setup instructions, see **LOBBY_SETUP.md**
