using IPA;
using IPA.Loader;
using IpaLogger = IPA.Logging.Logger;
using IPA.Config;
using IPA.Config.Stores;


namespace BSPWS;

[Plugin(RuntimeOptions.SingleStartInit)]
internal class Plugin
{
    internal static IpaLogger Log { get; private set; } = null!;

    internal static Config cfg { get; private set; }

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
        CP_SDK.Chat.Service.RegisterExternalService(new WSChatService());
    }

    [OnExit]
    public void OnApplicationQuit()
    {
        Log.Debug("OnApplicationQuit");
    }
}
