### Adds joystick.tv as a possible chat service for BeatSaberPlus. Not ready for production use yet

Currently it works, but I need to implement an actual settings menu and basically every feature beyond regular chat.
If you really want to build this and try it, you'll need to:
1. Edit BSP_JSTV.csproj and change your BeatSaberDir
```
<PropertyGroup>
 <BeatSaberDir>D:\Games\Standalone\BSManager\BSInstances\1.40.8</BeatSaberDir>
</PropertyGroup>
```
2. Follow the bot setup instructions for Joystick, same as in https://github.com/LumKitty/VNyan-JSTV/
3. Edit Plugin.cs and change:
```
public void OnApplicationStart() {
    ...
    JSTV.UserName = "Your Joystick username";
    JSTV.ApplicationID = "Application ID you got when making your bot";
    JSTV.ClientID = "Client ID you got when making your bot";
    ...
}
```
3. Create a new file named ShitSettings.cs in the same directory as Plugin.cs
```
namespace BSP_JSTV {
    internal static class ShitSettings {
		    internal static string ClientSecret = "Your client secret";
    }
}
```
4. Make sure that you don't upload ShitSettings.cs anywhere, and don't share your compiled DLL with anyone!  
   If you don't understand why sharing those files will be bad, you shouldn't be building this in its current state!

Credits: This mod would not be possible were it not for https://github.com/milkydelta/BSP-WS which I based it on
