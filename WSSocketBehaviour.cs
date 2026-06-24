using System;
using CP_SDK.Chat;
using CP_SDK_WebSocketSharp;
using CP_SDK_WebSocketSharp.Server;
using Newtonsoft.Json;

namespace BSPWS;

internal class WSSocketBehaviour : WebSocketBehavior
{
    private WSChatService service;

    internal void SetService(WSChatService srv){
        service = srv;
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        JsonMessage json =JsonConvert.DeserializeObject<JsonMessage>(e.Data);

        if (json.username == "" || json.msg ==""){return;}

        WSChatMessage msg = new WSChatMessage();
        WSChatUser usr = new WSChatUser();
        WSChatChannel chan = new WSChatChannel();

        chan.Name = "WebSocketChannel";

        usr.UserName = json.username;

        msg.Message = json.msg;
        msg.Sender = usr;
        msg.Channel = WSChatService.channel;


        service.m_OnTextMessageReceivedCallbacks?.InvokeAll(service, msg);
    }
}