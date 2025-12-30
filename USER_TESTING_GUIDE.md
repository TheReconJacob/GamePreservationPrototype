# User Testing Guide: Game Preservation Prototype

**Read this guide before starting the survey.**

**Time Required:** 20-30 minutes

---

## Before You Begin

### What You Need
- Game executable: `GamePreservationPrototype.exe`
- Test login: **Email:** `test@test.com` **Password:** `testtest`
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

## Part 2: Test Offline Mode (5 minutes)

**What you're testing:** Does the game work without internet?

### Steps:
1. **Turn off your internet** (disconnect Wi-Fi)
2. **Launch the game again**
3. **Expected result:**
   - Game opens directly (skips login)
   - You see "OFFLINE MODE" text (top-right corner)
   - Game is playable
4. **Play for 1-2 minutes** - confirm you can move, shoot, earn points
5. **Close the game**
6. **Turn internet back on**

**✅ You're done when:** You've confirmed offline mode works

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

**Local Multiplayer:**
- Was the setup straightforward?
- Was the lobby system easy to understand?
- Did the gameplay feel synchronized between windows?

**Overall:**
- What was confusing or frustrating?
- What worked well?
- Would this change how you think about game preservation?

---

## Done!

Close all game windows and complete the survey.

**Thank you for testing!**
