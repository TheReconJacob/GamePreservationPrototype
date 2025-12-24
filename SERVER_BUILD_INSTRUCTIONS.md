# Dedicated Server Build Configuration - Step-by-Step Instructions

## What Changed (Code Side - Already Done)
✅ **NetworkUI.cs** - Wrapped in `#if !DEDICATED_SERVER` (entire class excluded from server)
✅ **NetworkCameraFollow.cs** - Wrapped in `#if !DEDICATED_SERVER` (no camera on server)
✅ **PlayerMovement.cs** - Input handling wrapped in `#if !DEDICATED_SERVER` (server doesn't need input)
✅ **DedicatedServerBootstrap.cs** - Enhanced to disable AudioListeners and UI canvases
✅ **ServerBuildProcessor.cs** (NEW) - Editor script with build menu items

## What You Need to Do in Unity Editor

### Step 1: Create Editor Folder (If Needed)
1. In Unity Project window, navigate to `Assets/Scripts/`
2. Check if `Editor` folder exists
3. If not, right-click `Scripts` → Create → Folder → Name it `Editor`
4. The file `ServerBuildProcessor.cs` should already be there

### Step 2: Verify Scripts Compiled
1. Wait for Unity to finish compiling (bottom-right progress bar)
2. Check Console for any errors (there shouldn't be any)
3. You should see new menu items appear

### Step 3: Build Server Version
1. Go to Unity menu bar → **Build** → **Build Server (Windows x64)**
2. Unity will automatically:
   - Add `DEDICATED_SERVER` define to project
   - Build to `Builds/Server/GamePreservationPrototype_Server.exe`
   - Exclude NetworkUI, NetworkCameraFollow, and input handling
   - Open folder when done
3. Wait for build to complete (check Console for progress)

### Step 4: Build Client Version
1. Go to Unity menu bar → **Build** → **Build Client (Windows x64)**
2. Unity will automatically:
   - Remove `DEDICATED_SERVER` define
   - Build to `Builds/Client/GamePreservationPrototype.exe`
   - Include all UI and client features
3. Wait for build to complete

### Step 5: Test Server Build
1. Navigate to `Builds/Server/` folder
2. Open PowerShell in that folder (Shift + Right-click → "Open PowerShell window here")
3. Run server:
   ```powershell
   .\GamePreservationPrototype_Server.exe -batchmode -nographics -server -logFile server_log.txt
   ```
4. Check `server_log.txt` for startup messages

### Step 6: Test Client Build
1. Navigate to `Builds/Client/` folder
2. Double-click `GamePreservationPrototype.exe`
3. Enter IP: `127.0.0.1`
4. Click **Client** button
5. Verify connection to server

## Expected Results

### Server Build
- **File size:** ~50-100MB (vs ~500MB before)
- **Excluded code:** NetworkUI, camera systems, input handling
- **Log output:** Should show "DEDICATED_SERVER build detected" messages
- **No errors:** Server should start without UI/camera errors

### Client Build
- **File size:** ~500MB (full game)
- **Included code:** Everything (UI, camera, audio, graphics)
- **Normal operation:** Should work exactly as before

## Troubleshooting

### Problem: "Build" menu doesn't appear
**Solution:** 
1. Check `ServerBuildProcessor.cs` is in `Assets/Scripts/Editor/` folder
2. Check Console for compilation errors
3. Restart Unity if needed

### Problem: Server build same size as client
**Solution:**
1. Go to **Build** → **Toggle Server Build Mode** (should say "ENABLED")
2. Check Console: Should say "Server build mode ENABLED"
3. Rebuild server

### Problem: Server build has errors about UI components
**Solution:**
1. Check NetworkUI.cs has `#if !DEDICATED_SERVER` at top and `#endif` at bottom
2. Recompile scripts (Assets → Reimport All)
3. Rebuild

### Problem: Client build missing UI
**Solution:**
1. Go to **Build** → **Toggle Server Build Mode** (should say "DISABLED")
2. Check Console: Should say "Server build mode DISABLED"
3. Rebuild client

## Manual Method (If Build Menu Doesn't Work)

### Add Define Manually
1. Edit → Project Settings → Player
2. Click on PC icon (Windows build)
3. Scroll to "Other Settings"
4. Find "Scripting Define Symbols"
5. Add `DEDICATED_SERVER` (for server) or remove it (for client)
6. Click Apply

### Build Manually
1. File → Build Settings
2. Ensure both scenes are added:
   - `Assets/Scenes/LoginScene.unity`
   - `Assets/Scenes/GameScene.unity`
3. Select "Windows, Mac, Linux" platform
4. Select "Windows" under Target Platform
5. Select "x86_64" under Architecture
6. Click **Build** and choose output folder

## Verification Checklist

After building both versions:

### Server Build Verification
- [ ] File size is significantly smaller than client (~50-100MB vs ~500MB)
- [ ] Server starts with `-batchmode -nographics -server` arguments
- [ ] No UI-related errors in server logs
- [ ] No camera-related errors in server logs
- [ ] Clients can connect successfully
- [ ] Gameplay synchronization works

### Client Build Verification
- [ ] UI appears correctly (Host/Client/Server buttons)
- [ ] Can connect to server at 127.0.0.1
- [ ] Player spawns with camera attached
- [ ] Movement/shooting/scores sync correctly

## Next Steps After Successful Build

1. **Test both builds together:**
   - Start server: `.\Builds\Server\GamePreservationPrototype_Server.exe -batchmode -nographics -server`
   - Start 2 clients: Double-click `Builds\Client\GamePreservationPrototype.exe` twice
   - Both clients connect to 127.0.0.1
   - Verify gameplay sync

2. **Update documentation:**
   - Note new build locations in STANDALONE_SERVER_GUIDE.md
   - Document file size reduction
   - Update test procedures

3. **Archive builds:**
   - Create release folder with both builds
   - Include README with which is server vs client
   - Test from clean folder to verify no missing dependencies

## Technical Details

### Preprocessor Directives Used
- `#if !DEDICATED_SERVER` - Code only included in client builds
- `#if DEDICATED_SERVER` - Code only included in server builds
- `#endif` - Closes conditional block

### Files Modified
- `NetworkUI.cs` - 2 lines added (guards)
- `NetworkCameraFollow.cs` - 2 lines added (guards)
- `PlayerMovement.cs` - 2 lines added (Update method guard)
- `DedicatedServerBootstrap.cs` - Enhanced initialization (~15 lines)
- `ServerBuildProcessor.cs` - NEW (165 lines)

### Build Differences
| Feature | Server Build | Client Build |
|---------|-------------|--------------|
| UI Components | ❌ Excluded | ✅ Included |
| Camera System | ❌ Excluded | ✅ Included |
| Input Handling | ❌ Excluded | ✅ Included |
| Audio System | ⚠️ Disabled | ✅ Enabled |
| Network Code | ✅ Included | ✅ Included |
| Gameplay Logic | ✅ Included | ✅ Included |

## Questions?

- Check Console window for detailed logs during build
- All build menu actions log their progress
- Look for `[ServerBuildMenu]` and `[ServerBuildProcessor]` tags in Console
