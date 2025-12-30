# Login Instructions

## Test Account Credentials

When launching the game, use the following test account to log in:

**Email:** `test@test.com`  
**Password:** `testtest`

---

## How to Log In

1. Launch the game executable or press Play in Unity Editor
2. You will see the login screen with two input fields
3. Enter the email: `test@test.com`
4. Enter the password: `testtest`
5. Click the **Login** button
6. The game will authenticate with PlayFab and load the game scene

---

## Offline Mode

If you start the game without an internet connection:
- The game will automatically bypass the login screen
- You can play in offline mode
- Progress will be saved locally (not synced to cloud)
- When internet is restored, you will be redirected to the login screen

---

## Troubleshooting

### "Login Failed" Error
- Verify you entered the credentials exactly: `test@test.com` / `testtest`
- Check your internet connection
- Ensure PlayFab services are accessible

### Stuck on Login Screen
- Check the status text below the login button for error messages
- Check Unity Console logs for PlayFab API errors
- Verify PlayFab Title ID is set correctly in LoginManager (1042B5)

### Bypassing Authentication for Testing
The game automatically bypasses authentication if:
- No internet connection is detected at startup
- You can test offline functionality this way

---

## For Developers

### PlayFab Configuration
- **Title ID:** 1042B5
- **Test Account:** test@test.com
- **Configured in:** `Assets/Scripts/Systems/LoginManager.cs`

### Login Flow
1. User enters credentials
2. LoginManager attempts username login first via `LoginWithPlayFabRequest`
3. If username fails, tries email login via `LoginWithEmailAddressRequest`
4. On success: Loads GameScene
5. On failure: Displays error message

### Scene Flow
```
LoginScene → (Authentication) → GameScene → (Network Selection)
```
