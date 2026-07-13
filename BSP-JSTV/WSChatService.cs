using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Services;
using CP_SDK.UI.Modals;
using CP_SDK_WebSocketSharp.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using UnityEngine;
using WatsonWebsocket;

namespace BSP_JSTV;

internal class WSChatService : ChatServiceBase, IChatService
{
    //WebSocketServer server;

    public string DisplayName {get;}= "JSTV";

    public Color AccentColor { get; } = new Color(118, 225, 240);

    internal static WSChatChannel channel = new WSChatChannel();

    (IChatService,IChatChannel)[] chans => [(this,channel)];

    public ReadOnlyCollection<(IChatService, IChatChannel)> Channels => chans.ToList().AsReadOnly();

    public bool IsConnectedAndLive()
    {
        return true;
    }
    public bool IsInTempChannel(string p_ChannelName)
    {
        return false;
    }

    public void JoinTempChannel(string p_GroupIdentifier, string p_ChannelName, string p_Prefix, bool p_CanSendMessage)
    {
        return;
    }

    public void LeaveAllTempChannel(string p_GroupIdentifier)
    {
        return;
    }

    public void LeaveTempChannel(string p_ChannelName)
    {
        return;
    }

    public string PrimaryChannelName()
    {
        return "";
    }

    public void RecacheEmotes()
    {
        return;
    }

    public void SendTextMessage(IChatChannel p_Channel, string p_Message)
    {
        const string VNyanURL = "ws://localhost:8000/vnyan";
        WatsonWsClient wsClient = new WatsonWsClient(new Uri(VNyanURL));
        System.Threading.CancellationToken CT = new System.Threading.CancellationToken();
        wsClient.KeepAliveInterval = 1000;
        wsClient.Start();
        wsClient.SendAsync("BSPlus "+p_Message, WebSocketMessageType.Text, CT);
        wsClient.Stop();

        return;
    }

    public void Start()
    {
        JSTV.ConnectJSTV();
        //server = new WebSocketServer(9060);

        //server.AddWebSocketService<WSSocketBehaviour>("/sock", s => s.SetService(this));

        //server.Start();

    }

    public void Stop()
    {
        JSTV.DisconnectJSTV();
        //server.Stop();
    }

    public string WebPageHTML()
    {
        return "";
    }

    public string WebPageHTMLForm()
    {
        return "";
    }

    public string WebPageJS()
    {
        return "";
    }

    public string WebPageJSValidate()
    {
        return "";
    }

    public void WebPageOnPost(Dictionary<string, string> p_PostData)
    {
        return;
    }
}
