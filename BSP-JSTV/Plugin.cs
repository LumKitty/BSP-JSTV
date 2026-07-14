using CP_SDK_WebSocketSharp;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using IpaLogger = IPA.Logging.Logger;


namespace BSP_JSTV;

[Plugin(RuntimeOptions.SingleStartInit)]
internal class Plugin
{
    internal static IpaLogger Log { get; private set; } = null!;

    internal static Config cfg { get; private set; }

    internal static WSChatService service;

    [Init]
    public Plugin(IpaLogger ipaLogger,Config config, PluginMetadata pluginMetadata)
    {
        Log = ipaLogger;
        PluginConfig.Instance = config.Generated<PluginConfig>();
        Log.Info($"{pluginMetadata.Name} {pluginMetadata.HVersion} initialized.");
    }
        
    [OnStart]
    public void OnApplicationStart()
    {
        Log.Debug("OnApplicationStart");
        if (!(PluginConfig.Instance.UserName.IsNullOrEmpty() || PluginConfig.Instance.ApplicationID.IsNullOrEmpty() || 
            PluginConfig.Instance.ClientID.IsNullOrEmpty() || PluginConfig.Instance.ClientSecret.IsNullOrEmpty())) {
            service = new WSChatService();
            //JSTV.UserName = PluginConfig.Instance.Username;
            //JSTV.ApplicationID = PluginConfig.Instance.ApplicationID;
            //JSTV.ClientID = PluginConfig.Instance.ClientID;
            //JSTV.ClientSecret = PluginConfig.Instance.ClientSecret;
            //JSTV.Port = PluginConfig.Instance.Port;
            CP_SDK.Chat.Service.RegisterExternalService(service);
        } else {
            Log.Error("BSP-JSTV: Bot credentials not configured");
        }
    }

    [OnExit]
    public void OnApplicationQuit()
    {
        Log.Debug("OnApplicationQuit");
    }
}
