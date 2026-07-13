using IPA;
using IPA.Loader;
using IpaLogger = IPA.Logging.Logger;
using IPA.Config;
using IPA.Config.Stores;


namespace BSP_JSTV;

[Plugin(RuntimeOptions.SingleStartInit)]
internal class Plugin
{
    internal static IpaLogger Log { get; private set; } = null!;

    internal static Config cfg { get; private set; }

    internal static WSChatService service = new WSChatService();

    [Init]
    public Plugin(IpaLogger ipaLogger,Config config, PluginMetadata pluginMetadata)
    {
        Log = ipaLogger;
        cfg = config;
        Log.Info($"{pluginMetadata.Name} {pluginMetadata.HVersion} initialized.");
    }
        
    [OnStart]
    public void OnApplicationStart()
    {
        Log.Debug("OnApplicationStart");
        JSTV.UserName = "LumKitty";
        JSTV.ApplicationID = "416a24b3-267b-48f8-a591-74f8eed3b60f";
        JSTV.ClientID = "04174c34-8a60-4a5f-bc67-cebfe8aa7399";
        JSTV.ClientSecret = ShitSettings.ClientSecret;
        JSTV.Port = 6970;

        CP_SDK.Chat.Service.RegisterExternalService(service);
    }

    [OnExit]
    public void OnApplicationQuit()
    {
        Log.Debug("OnApplicationQuit");
    }
}
