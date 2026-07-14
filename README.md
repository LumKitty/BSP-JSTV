### Adds joystick.tv as a possible chat service for BeatSaberPlus.

Early alpha software. It seems to work, but no promises

<img width="741" height="342" alt="image" src="https://github.com/user-attachments/assets/8c72df0d-2b21-4a42-a368-3f6d2fbd39cb" />

### Setup - JSTV website:
1. Visit https://joystick.tv/applications
2. Create a new bot
3. Fill in a username. This will be displayed in chat and should be specific to you, not something generic like "VNyan"
4. Make a note of the Application ID, Client ID and Client Secret
   <img width="870" height="1065" alt="image" src="https://github.com/user-attachments/assets/0769e318-3d1c-4b94-8d54-9804ae5ae308" />
5. Give it all the permissions for now, I'll figure out what it actually needs later!
6. Set the OAuth Redirect URL to http://localhost:6970

### Setup - Beat Saber
7. Have BeatSaberPlus configured and working: https://github.com/hardcpp/BeatSaberPlus
8. Copy BSP_JSTV.dll into the Plugins directory of your Beat Saber install
9. Make sure you unblocked it (right click -> properties -> unblock)
10. Start Beat Saber, then close it
11. Open BSP-JSTV.json in your BeatSaber\UserData directory
12. Fill in the Application ID, Client ID and Client Secret you noted at step 4
13. Start Beat Saber - do not wear your VR headset for this part!
14. The app will appear to hang (I'm working on this) and a webpage will open asking you to grant permissions, allow it and Beat Saber will load normally from now on

If successful you should see Joystick messages alongside Twitch ones, chat should be relayed, requests should work etc. Feature partity should be very close to that of Twitch
