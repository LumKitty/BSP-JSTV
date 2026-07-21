using CP_SDK_WebSocketSharp;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using System.Threading;
using System.Threading.Tasks;
using IpaLogger = IPA.Logging.Logger;


namespace BSP_JSTV;

[Plugin(RuntimeOptions.SingleStartInit)]
internal class Plugin {
    internal static IpaLogger Log { get; private set; } = null!;
    internal static Config cfg { get; private set; }
    internal static JSTVChatService service;
    internal static Thread AuthoriseUserThread;

    [Init]
    public Plugin(IpaLogger ipaLogger,Config config, PluginMetadata pluginMetadata) {
        Log = ipaLogger;
        PluginConfig.Instance = config.Generated<PluginConfig>();
        Log.Info($"{pluginMetadata.Name} {pluginMetadata.HVersion} initialized.");
    }
        
    [OnStart]
    public void OnApplicationStart() {
        Log.Debug("OnApplicationStart");
        if (!(PluginConfig.Instance.ApplicationID.IsNullOrEmpty() || PluginConfig.Instance.ClientID.IsNullOrEmpty() || PluginConfig.Instance.ClientSecret.IsNullOrEmpty())) {
            Log.Debug("Authenticate with joystick");
            AuthoriseUserThread = new Thread(new ThreadStart(JSTV.AuthoriseUser));
            AuthoriseUserThread.Name = "JSTV_AuthoriseUser";
            AuthoriseUserThread.Start();

            Log.Debug("Create chat service");
            service = new JSTVChatService();
            Log.Debug("Register chat service");
            CP_SDK.Chat.Service.RegisterExternalService(service);
        } else {
            Log.Error("Bot credentials not configured");
        }
    }

    [OnExit]
    public void OnApplicationQuit() {
        Log.Debug("OnApplicationQuit");
    }
}
