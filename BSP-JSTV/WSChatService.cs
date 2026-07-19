using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Services;
using CP_SDK_WebSocketSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace BSP_JSTV;

internal class WSChatService : ChatServiceBase, IChatService {
    //WebSocketServer server;
    
    public string DisplayName {get;}= "JSTV";
    public Color AccentColor { get; } = new Color((118/256f), (225/265f), (240/256f));
    internal static WSChatChannel channel = new WSChatChannel(PluginConfig.Instance.UserName);

    //string key is jstv channelId
    Dictionary<string, WSChatChannel> channelsDict = new Dictionary<string, WSChatChannel>();

    JSTV_New jstv = new JSTV_New();

    public ReadOnlyCollection<(IChatService, IChatChannel)> Channels { get {
            List<(IChatService, IChatChannel)> tempList = new List<(IChatService, IChatChannel)>();
            foreach (var item in channelsDict.Values) {
                tempList.Add((this, item));
            }
            return tempList.AsReadOnly();
        }
    }


    public bool IsConnectedAndLive() {
        return JSTV_New.BotConnected;
    }
    public bool IsInTempChannel(string p_ChannelName) {
        return false;
    }

    public void JoinTempChannel(string p_GroupIdentifier, string p_ChannelName, string p_Prefix, bool p_CanSendMessage) {
        return;
    }

    public void LeaveAllTempChannel(string p_GroupIdentifier) {
        return;
    }

    public void LeaveTempChannel(string p_ChannelName) {
        return;
    }

    public string PrimaryChannelName() {
        return PluginConfig.Instance.UserName;
    }

    public void RecacheEmotes() {
        return;
    }

    public void SendTextMessage(IChatChannel p_Channel, string p_Message) {
        if (PluginConfig.Instance.FilterHTTP) {
            p_Message = p_Message.Replace("https://", "").Replace("http://", ""); // Joystick disallow most external links, including beatsaver.com
        }
        if (channel is WSChatChannel) {
            WSChatChannel w_Channel = p_Channel as WSChatChannel;
            jstv.SendChatMessage_New(p_Message, w_Channel._id);  
        }
        //JSTV.SendChatMessage(p_Message);
        /*const string VNyanURL = "ws://localhost:8000/vnyan";
        WatsonWsClient wsClient = new WatsonWsClient(new Uri(VNyanURL));
        System.Threading.CancellationToken CT = new System.Threading.CancellationToken();
        wsClient.KeepAliveInterval = 1000;
        wsClient.Start();
        wsClient.SendAsync("BSPlus "+p_Message, WebSocketMessageType.Text, CT);
        wsClient.Stop();*/

        return;
    }

    public void Start() {
        try {
            jstv.ConnectWebSocket_New();

            if (!PluginConfig.Instance.UserRefreshToken.IsNullOrEmpty() && PluginConfig.Instance.UserRefreshToken != "null-refreshtoken") {
                Plugin.Log.Info("Attempting to use stored refresh token");
                if (jstv.ExchangeRefreshForTokens_New(PluginConfig.Instance.UserRefreshToken, out string aT, out string nRT)) {
                    PluginConfig.Instance.UserRefreshToken = nRT;
                    if(jstv.FetchStreamerSettings_New(aT, out string uN, out string cId)) {
                        WSChatChannel newChannel = new WSChatChannel(uN);
                        newChannel._id = cId;
                        channelsDict.Add(cId, newChannel);
                        //Do we need to call the OnLogin callback here?
                    }
                }
            }
        } catch (Exception ex) {
            Plugin.Log.Error(ex.ToString());
        }

    }

    public void Stop() {
        jstv.DisconnectWebSocket_New();
        //JSTV.DisconnectJSTV();
    }

    public string WebPageHTML() {
        return "";
    }

    public string WebPageHTMLForm() {
        return """
        <div class="col-lg-4 card text-center md mx-auto">
            <b class="card-header">BSP-JSTV</b>
            <div class="card-body">
                <div class="input-group">
                    <span class="input-group-text">THING!</span>
                    <input class="form-control" type="text" name="jstv-thing" id="jstv-thing">
                </div>
                <p>This empty field is used to send a short-lived code. Once obtained, "Save" it quickly.</p>
                <div class="input-group">
                    <span class="input-group-text">Code</span>
                    <input class="form-control" type="password" name="jstv-code" id="jstv-code">
                </div>
                <a href="https://joystick.tv/api/oauth/authorize?response_type=code&client_id=@CLIENT_ID@&scope=bot"
                class="btn btn-primary btn-lg" style="background-color: rgb(24, 42, 54);color: rgb(10, 255, 255)">Authorize Bot (Obtain Code)</a>
            </div>
        </div>
        """.Replace("@CLIENT_ID@", PluginConfig.Instance.ClientID);
    }

    public string WebPageJS() {
        return """
        {
            let params = new URLSearchParams(window.location.search);
            if (params.has("code")){
                let code_input_box = document.getElementById("jstv-code");
                code_input_box.value = params.get("code");
            }
        }
        """;
    }

    public string WebPageJSValidate() {
        return "";
    }

    public void WebPageOnPost(Dictionary<string, string> p_PostData) {
        if (p_PostData.ContainsKey("jstv-thing")) {
            Plugin.Log.Notice("Post Thing: "+ p_PostData["jstv-thing"]);
        }
        if (p_PostData.ContainsKey("jstv-code")) {
            Plugin.Log.Notice("Post Code: " + p_PostData["jstv-code"]);
            if (p_PostData["jstv-code"] != "") {
                if (jstv.ExchangeCodeForTokens_New(p_PostData["jstv-code"], out string aT, out string nRT)) {
                    PluginConfig.Instance.UserRefreshToken = nRT;
                    if (jstv.FetchStreamerSettings_New(aT, out string uN, out string cId)) {
                        WSChatChannel newChannel = new WSChatChannel(uN);
                        newChannel._id = cId;
                        channelsDict.Add(cId, newChannel);
                        //Do we need to call the OnLogin callback here?
                    }
                }
            }
        }
        return;
    }
}
