# Network Player Spawning Setup Guide

## Issue Fixed
**Problem:** Host and Client were both controlling the same player GameObject instead of having separate player instances.

**Root Cause:** No player spawning logic was implemented. Both Host and Client tried to control an existing player GameObject in the scene.

## Changes Made

### 1. LocalNetworkManager.cs
- Added `SpawnPlayerForClient()` method that spawns a player instance for each connected client
- Server automatically spawns a player when a client connects
- Each player is assigned ownership to the correct client via `SpawnAsPlayerObject()`

### 2. Shooting.cs
- Converted from `MonoBehaviour` to `NetworkBehaviour`
- Added `IsOwner` check so only the local player can shoot
- Implemented `ShootServerRpc()` for server-authoritative projectile spawning
- Projectiles are now spawned on the network so all clients can see them

## Unity Editor Setup Required

### Step 1: Remove Pre-existing Player from Scene
**CRITICAL:** You need to remove any Player GameObject currently in the GameScene.

1. Open `Assets/Scenes/GameScene.unity`
2. Find the Player GameObject in the Hierarchy
3. **Delete it** (players will now spawn dynamically when connecting)

### Step 2: Configure LocalNetworkManager
1. Select the NetworkManager GameObject in the Hierarchy
2. Find the LocalNetworkManager component
3. In the Inspector, set:
   - **Player Prefab**: Drag `Assets/Prefabs/Player.prefab` here
   - **Spawn Points**: Create empty GameObjects for spawn positions

**To create spawn points:**
1. Right-click in Hierarchy → Create Empty
2. Rename to "SpawnPoint_1"
3. Position it where you want Player 1 to spawn (e.g., X: -2, Y: 1, Z: 0)
4. Duplicate it → Rename to "SpawnPoint_2"
5. Position it where you want Player 2 to spawn (e.g., X: 2, Y: 1, Z: 0)
6. Drag both spawn points into the **Spawn Points** array in LocalNetworkManager

### Step 3: Make Projectile Prefab Networkable
1. Open `Assets/Prefabs/Projectile.prefab` (or wherever your projectile prefab is)
2. Add Component → **NetworkObject**
3. In NetworkObject settings:
   - Leave all defaults
4. **Important:** Add the Projectile prefab to the NetworkManager's **Network Prefabs** list:
   - Select NetworkManager GameObject
   - Find the Unity.Netcode.NetworkManager component
   - Expand "Network Prefabs List"
   - Increase size by 1
   - Drag Projectile.prefab into the new slot

### Step 4: Verify Player Prefab Setup
Make sure your Player.prefab has:
- ✅ NetworkObject component
- ✅ NetworkTransform component (Sync Position X, Y, Z enabled)
- ✅ PlayerMovement script (NetworkBehaviour)
- ✅ Shooting script (NetworkBehaviour)

## Testing

### 1. Start Host
1. Open main Unity Editor
2. Click Play
3. Click "Host" button
4. **You should see a player spawn at SpawnPoint_1**

### 2. Start Client
1. Open ParrelSync clone
2. Click Play
3. Click "Client" button
4. **You should see a SECOND player spawn at SpawnPoint_2**

### 3. Verify Movement
- Move Host player with WASD → Client should see Host move
- Move Client player with WASD → Host should see Client move
- Both players should move independently

### 4. Verify Shooting
- Host clicks mouse → Projectile spawns, both players see it
- Client clicks mouse → Projectile spawns, both players see it

## Expected Behavior

### ✅ Correct (After Fix)
- Host starts: 1 player spawns for Host
- Client connects: SECOND player spawns for Client
- Each player controls their own character
- Both players see each other move
- Both players see all projectiles

### ❌ Incorrect (Before Fix)
- Host and Client both control the same player
- Only one player visible in scene
- Movement conflicts between Host and Client

## Troubleshooting

### "No player spawns when Host starts"
- Make sure you **deleted the Player GameObject from the scene**
- Verify **Player Prefab** is assigned in LocalNetworkManager

### "Client connects but no player spawns"
- Check Unity Console for errors
- Verify Player.prefab has NetworkObject component
- Ensure Player.prefab is in NetworkManager's **Network Prefabs** list

### "Players spawn but can't move"
- Verify PlayerMovement is NetworkBehaviour (not MonoBehaviour)
- Check that Player.prefab has NetworkTransform component
- Ensure Sync Position X/Y/Z are enabled on NetworkTransform

### "Shooting works but projectiles don't sync"
- Verify Projectile.prefab has NetworkObject component
- Ensure Projectile.prefab is in NetworkManager's **Network Prefabs** list
- Check Shooting script is NetworkBehaviour (not MonoBehaviour)

### "ERROR: Only the server can spawn NetworkObjects"
- This means the Client is trying to spawn something directly
- Shooting.cs now uses ServerRpc to spawn on server
- Make sure you're using the updated Shooting.cs code

## Technical Details

### How Player Spawning Works
1. Client connects → `OnClientConnected` callback fires on server
2. Server calls `SpawnPlayerForClient(clientId)`
3. Server instantiates Player prefab at spawn position
4. Server calls `networkObject.SpawnAsPlayerObject(clientId, true)`
5. Netcode automatically replicates player to all clients
6. Player ownership is assigned to the correct client

### How Shooting Works
1. Local player presses mouse button
2. `Shooting.Update()` checks `IsOwner` → only local player continues
3. Local player calls `ShootServerRpc()` (sends message to server)
4. Server receives RPC, instantiates projectile
5. Server calls `networkObject.Spawn()` on projectile
6. Netcode replicates projectile to all clients
7. All players see the projectile

## Next Steps
After verifying player spawning and shooting work correctly:
- Phase 2: Network score synchronization
- Phase 3: Target spawning synchronization
- Phase 4: Network-compatible ModManager integration
