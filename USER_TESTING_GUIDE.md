# User Testing Guide: Game Preservation Prototype

**Read this guide before starting the survey.**

**Time Required:**30-35 minutes

---

## Before You Begin

### What You Need
- **Game executable:** [Download from GitHub Releases](https://github.com/TheReconJacob/GamePreservationPrototype/releases/v1.0.0)  
  - **Client:** `GamePreservationPrototype.exe` (required for all tests)  
  - **Server (optional):** `GamePreservationPrototype_Server.exe` (only needed for Part 4)
- **Test login:**  
  **Email:** `test@test.com`  
  **Password:** `testtest`
- Internet connection (you'll turn it off and on during testing)
- For multiplayer test: Ability to run the game twice at the same time

---

## Part 1: Login & Play Online (5 minutes)

**What you're testing:** Normal online gameplay

### Steps:
1. **Launch** `GamePreservationPrototype.exe`
2. **Login** with: `test@test.com` / `testtest`
3. **Play the game:**
   - Move: **WASD keys**
   - Shoot: **Left click mouse**
   - Destroy targets (moving objects) to earn points
4. **Play for 3-4 minutes** until you have **50+ points**
5. **Close the game** (X button)

**✅ You're done when:** You've played and earned some points

---

## Part 2: Test Offline Mode (10 minutes)

**What you're testing:** Does the game work without internet, and does it retain progress when switching online/offline?

### Scenario 1: Start Offline, Then Go Online
1. **Turn off your internet** (disconnect Wi-Fi)
2. **Launch the game**  
   - **Expected result:** Game opens directly (skips login)  
   - You see **"OFFLINE MODE"** in the top-right corner  
   - Game is playable
3. **Play for 1-2 minutes** and earn points
4. **Turn your internet back on**
5. **Expected result:** Game shows the **login page** because it detects an online connection  
   - After logging in, your **score from offline mode should be retained**
6. **Close the game**

---

### Scenario 2: Start Online, Then Go Offline
1. **Launch the game** with internet on
2. **Login** with test credentials
3. **Play for 1-2 minutes** and earn points
4. **Turn off your internet** (disconnect Wi-Fi)
5. **Expected result:**  
   - Game automatically switches to **OFFLINE MODE**  
   - You can continue playing without interruption  
   - Your **score continues from the online session**
6. **Play for 1-2 minutes**, confirm points are added
7. **Turn internet back on**  
   - Expected result: Game returns to **online mode**, retains the updated score
8. **Close the game**

**✅ You're done when:** Both offline/online transitions work and the score is preserved correctly

**❌ If it doesn't work:** Note what happened in the survey's feedback question

---

## Part 3: Test Local Multiplayer (10-15 minutes)

**What you're testing:** Can players host their own multiplayer game?

### Steps:

#### 3a. Start as Host
1. **Launch the game** (internet should be on)
2. **Login** with test credentials
3. **Click "Start Host" button** (top of screen)
4. **Expected result:**
   - A lobby screen appears (dark overlay)
   - You see: "Host (Client 0)" and a green "Start Game" button
   - Status shows: "1 player connected"

**✅ SUCCESS:** Lobby appeared

#### 3b. Connect a Second Player
**KEEP THE FIRST GAME WINDOW OPEN**

1. **Launch the game AGAIN** (second window opens)
2. **Login** with same credentials
3. **The IP address field should already show:** `127.0.0.1` (no need to type anything)
4. **Click "Start Client" button**
5. **Expected result:**
   - **BOTH game windows** now show lobby
   - Host window shows: "2 players connected"
   - Client window shows: "Client 1"
   - Only the HOST sees the "Start Game" button

**✅ SUCCESS:** Both windows show lobby with 2 players

**❌ CAN'T CONNECT?** See troubleshooting at the end

#### 3c. Play Together
1. **In the HOST window:** Click the **"Start Game"** button
2. **Expected result:**
   - Lobby disappears in BOTH windows
   - You see 2 player characters (one in each window)
3. **Test it works:**
   - Move in one window → other window sees that player move
   - Shoot targets in either window → both see the same targets
   - Score increases on both screens

**✅ You're done when:** You've confirmed both players can play together

4. **Close both game windows**

---

## Part 4 (Optional): Test Headless Server (5 minutes)

**What you're testing:** Can a standalone server run without a player?

**Note:** This is optional and requires the **server executable** `GamePreservationPrototype_Server.exe`.

### Steps:

#### 4a. Start Headless Server
1. **Launch `GamePreservationPrototype_Server.exe`**  
   - A window may briefly appear then close (this is normal)  
   - Server runs silently in the background
2. **Check it's running:**
   - Open Task Manager (Ctrl+Shift+Esc)
   - Look for `GamePreservationPrototype_Server.exe` in processes
   - Server is listening on IP `127.0.0.1` (localhost)

**✅ SUCCESS:** Server process visible in Task Manager

#### 4b. Connect as Client
1. **Launch the normal game** `GamePreservationPrototype.exe`
2. **Login** with test credentials
3. **IP address should show:** `127.0.0.1`
4. **Click "Start Client" button**
5. **Expected result:**
   - Connection message shows: "Connecting..."
   - Then: "Connected to dedicated server at 127.0.0.1"
   - **No lobby appears** (spawns directly into game)
   - You see your player character immediately
   - **No extra "ghost" player** (server doesn't spawn itself)

**✅ SUCCESS:** Connected directly without lobby, only 1 player visible

**Difference from Host:**
- Headless Server: No lobby, immediate spawn, server has no player character
- Host: Lobby appears, must wait for "Start Game", host IS a player

#### 4c. Stop Server
1. **Open Task Manager**
2. **Find** `GamePreservationPrototype_Server.exe`
3. **Right-click** → **End Task**

**✅ You're done when:** You've tested connecting to a standalone server

---

## Quick Troubleshooting

**Can't login?**
- Check you typed: `test@test.com` / `testtest` exactly
- Make sure internet is connected

**Offline mode doesn't work?**
- Make sure internet is FULLY disconnected (try opening a website - it should fail)
- Close and restart the game AFTER disconnecting

**Can't connect second player?**
- Make sure you typed `127.0.0.1` exactly in the IP field
- Try: Temporarily disable Windows Firewall and try again
- If still fails: Note this in the survey and skip to the end

**Second game window won't open?**
- Some systems block multiple instances
- Note this in survey feedback - your feedback is still valuable!

---

## What to Think About for the Survey

As you test, consider:

**Offline Mode:**
- Was it clear when you were in offline mode?
- Did your score continue from where you left off online?
- Did everything work the same as online?
- Did switching between online/offline retain your progress correctly?

**Local Multiplayer:**
- Was the setup straightforward?
- Was the lobby system easy to understand?
- Did the gameplay feel synchronized between windows?

**Headless Server (if tested):**
- Did the server run without opening a window?
- Was connecting different from connecting to a Host?
- Did you notice the server had no player character?

**Overall:**
- What was confusing or frustrating?
- What worked well?
- Would this change how you think about game preservation?

---

## Done!

Close all game windows and complete the survey.

**Thank you for testing!**
