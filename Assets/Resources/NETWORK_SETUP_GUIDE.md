# Sprint 3 Phase 1: Local Network Multiplayer Setup Guide

## Overview
This guide covers setting up local network multiplayer using Unity Netcode for GameObjects. Players can connect over LAN for preserved multiplayer functionality that doesn't depend on external servers.

## Prerequisites Completed
✅ **ParrelSync installed** - Allows testing multiplayer in multiple Unity Editor instances
✅ **Netcode for GameObjects (v2.5.1)** - Already in package manifest
✅ **Unity Transport** - Included with Netcode package

---

## Unity Editor Setup Steps

### Step 1: Add NetworkManager to Scene

1. **Create NetworkManager GameObject:**
   - In GameScene, right-click Hierarchy → `Create Empty`
   - Name it `NetworkManager`

2. **Add NetworkManager Component:**
   - With NetworkManager selected, click `Add Component`
   - Search for `NetworkManager` (Unity.Netcode)
   - Click to add it

3. **Add Unity Transport:**
   - Click `Add Component` again
   - Search for `UnityTransport`
   - Add it

4. **Add LocalNetworkManager Script:**
   - Click `Add Component`
   - Search for `LocalNetworkManager`
   - Add your custom script

5. **Configure NetworkManager:**
   - In Inspector, find `NetworkManager` component
   - Under `NetworkTransport`, drag the `UnityTransport` component into the field
   - Leave other settings as default for now

### Step 2: Make Player Prefab Networked

1. **Open Player Prefab:**
   - Navigate to `Assets/Prefabs/`
   - Double-click `Player.prefab` to open in Prefab mode

2. **Add NetworkObject Component:**
   - Select root GameObject in prefab
   - Click `Add Component`
   - Search for `NetworkObject`
   - Add it

3. **Verify PlayerMovement:**
   - PlayerMovement should now inherit from `NetworkBehaviour` (already updated)
   - Component should show no errors

4. **Save Prefab:**
   - File → Save (or Ctrl+S)
   - Exit Prefab mode

### Step 3: Configure Player Prefab in NetworkManager

1. **Select NetworkManager in Hierarchy**

2. **Add Player Prefab to Network Prefabs List:**
   - In `NetworkManager` component
   - Find `NetworkPrefabs` section
   - Click `+` to add a new slot
   - Drag `Player.prefab` from Project into the new slot

3. **Set Default Player Prefab:**
   - In `NetworkManager` component
   - Find `PlayerPrefab` field
   - Drag `Player.prefab` into this field
   - This spawns automatically when clients connect

### Step 4: Create Network UI (Optional but Recommended)

1. **Create UI Canvas:**
   - Right-click Hierarchy → `UI → Canvas`
   - Name it `NetworkUI`

2. **Add Host Button:**
   - Right-click Canvas → `UI → Button - TextMeshPro`
   - Name it `HostButton`
   - Change text to "Start Host"

3. **Add Client Button:**
   - Duplicate HostButton (Ctrl+D)
   - Name it `ClientButton`
   - Change text to "Join as Client"
   - Move below HostButton

4. **Add IP Input Field:**
   - Right-click Canvas → `UI → Input Field - TextMeshPro`
   - Name it `IPInputField`
   - Set placeholder text to "127.0.0.1"

5. **Add Status Text:**
   - Right-click Canvas → `UI → Text - TextMeshPro`
   - Name it `StatusText`
   - Set text to "Ready to Connect"

6. **Add NetworkUI Script:**
   - Select Canvas
   - Add Component → `NetworkUI`
   - Drag components into corresponding fields:
     - HostButton → Host Button field
     - ClientButton → Client Button field
     - IPInputField → IP Input Field field
     - StatusText → Status Text field
     - NetworkManager → Local Network Manager field

### Step 5: Configure Spawn Points (Optional)

1. **Create Spawn Points:**
   - Create Empty GameObject, name `SpawnPoint1`
   - Position at (0, 1, 0)
   - Duplicate for more spawn points (e.g., `SpawnPoint2` at (5, 1, 0))

2. **Assign to LocalNetworkManager:**
   - Select NetworkManager GameObject
   - In `LocalNetworkManager` component
   - Set `Spawn Points` array size to number of spawn points
   - Drag each spawn point into array slots

---

## Testing with ParrelSync

### Option 1: Test on Same Machine (Localhost)

1. **Create Clone Project:**
   - Top menu: `ParrelSync → Clones Manager`
   - Click `Create new clone`
   - Wait for clone creation (this creates a linked copy of project)

2. **Start Host in Main Editor:**
   - Play the game in main Unity Editor
   - Click "Start Host" button
   - You should see "Started as Host" in Console

3. **Start Client in Clone Editor:**
   - Open clone project (it should auto-open or find in ParrelSync window)
   - Play the game in clone Editor
   - Ensure IP is `127.0.0.1` (localhost)
   - Click "Join as Client" button
   - You should see "Client connected" in both Consoles

4. **Test Movement:**
   - Move with WASD in Host editor - host player moves
   - Move with WASD in Client editor - client player moves
   - Both players should see each other moving

### Option 2: Test on Local Network (LAN)

1. **Find Host IP Address:**
   - On Host machine, open Command Prompt
   - Type: `ipconfig` (Windows) or `ifconfig` (Mac/Linux)
   - Find IPv4 Address (e.g., `192.168.1.100`)

2. **Start Host:**
   - Play game on Host machine
   - Click "Start Host"

3. **Connect Client:**
   - Play game on Client machine (different computer on same network)
   - Enter Host's IP address in input field (e.g., `192.168.1.100`)
   - Click "Join as Client"

4. **Verify Connection:**
   - Both machines should see connection logs
   - Players should see each other and movement should sync

---

## Verification Checklist

Before moving to next phase, verify:

- [ ] NetworkManager GameObject exists in scene with components:
  - [ ] NetworkManager component
  - [ ] UnityTransport component
  - [ ] LocalNetworkManager script

- [ ] Player prefab configured:
  - [ ] NetworkObject component added
  - [ ] PlayerMovement inherits from NetworkBehaviour
  - [ ] Prefab added to NetworkManager's Network Prefabs list
  - [ ] Prefab set as Player Prefab in NetworkManager

- [ ] Network UI functional:
  - [ ] Host button starts host mode
  - [ ] Client button connects to host
  - [ ] IP input field updates connection address
  - [ ] Status text shows connection state

- [ ] Movement synchronization:
  - [ ] Host player can move with WASD
  - [ ] Client player can move with WASD
  - [ ] Each player sees the other player's movement
  - [ ] No input lag or stuttering

- [ ] Console logs show:
  - [ ] "Started as Host" when hosting
  - [ ] "Client X connected" when client joins
  - [ ] "You own this player" on local player
  - [ ] "This is another player's character" on remote players

---

## Troubleshooting

**Player prefab not spawning:**
- Ensure Player.prefab is in NetworkManager's Network Prefabs list
- Check that Player Prefab field is set in NetworkManager
- Verify NetworkObject component is on root of prefab

**Movement not syncing:**
- Verify PlayerMovement inherits from NetworkBehaviour (not MonoBehaviour)
- Check that `IsOwner` checks are in Update/FixedUpdate
- Ensure NetworkObject is on same GameObject as PlayerMovement

**Client can't connect:**
- Verify IP address is correct (use `ipconfig`/`ifconfig`)
- Check firewall isn't blocking port 7777
- Ensure both machines are on same network (for LAN)
- For localhost, use `127.0.0.1`

**Multiple players spawn at same location:**
- Set up spawn points in LocalNetworkManager
- Assign spawn point transforms to Spawn Points array

**"NetworkManager is null" errors:**
- NetworkManager must exist in scene before Play mode
- Check NetworkManager.Singleton is not null in scripts

---

## Next Steps

Once local network multiplayer is working:
- ✅ Phase 1 Complete: Basic networked movement functional
- → Phase 2: Network shooting and projectiles
- → Phase 3: Network score synchronization
- → Phase 4: Dedicated server build

---

## Preservation Benefits

✅ **LAN-Based**: No internet required, works on local networks
✅ **No External Servers**: Host/Client architecture means any player can host
✅ **IP-Based Connection**: Direct connection, no matchmaking service needed
✅ **ParrelSync Testing**: Can test multiplayer without multiple machines
✅ **Future-Proof**: Local network will work indefinitely, no service shutdown risk

This multiplayer architecture ensures the game remains playable on LANs even after official server shutdown, supporting the core preservation philosophy.
