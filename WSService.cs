using System.Collections.Generic;
using System.Collections.ObjectModel;
using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Services;
using UnityEngine;

namespace BSPWS;

internal class WSService : ChatServiceBase, IChatService
{
    public string DisplayName {get;}= "BSPWS";

    public Color AccentColor {get;} = Color.magenta;

    public ReadOnlyCollection<(IChatService, IChatChannel)> Channels => throw new System.NotImplementedException();

    public bool IsConnectedAndLive()
    {
        throw new System.NotImplementedException();
    }

    public bool IsInTempChannel(string p_ChannelName)
    {
        throw new System.NotImplementedException();
    }

    public void JoinTempChannel(string p_GroupIdentifier, string p_ChannelName, string p_Prefix, bool p_CanSendMessage)
    {
        throw new System.NotImplementedException();
    }

    public void LeaveAllTempChannel(string p_GroupIdentifier)
    {
        throw new System.NotImplementedException();
    }

    public void LeaveTempChannel(string p_ChannelName)
    {
        throw new System.NotImplementedException();
    }

    public string PrimaryChannelName()
    {
        throw new System.NotImplementedException();
    }

    public void RecacheEmotes()
    {
        throw new System.NotImplementedException();
    }

    public void SendTextMessage(IChatChannel p_Channel, string p_Message)
    {
        throw new System.NotImplementedException();
    }

    public void Start()
    {
        throw new System.NotImplementedException();
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public string WebPageHTML()
    {
        throw new System.NotImplementedException();
    }

    public string WebPageHTMLForm()
    {
        throw new System.NotImplementedException();
    }

    public string WebPageJS()
    {
        throw new System.NotImplementedException();
    }

    public string WebPageJSValidate()
    {
        throw new System.NotImplementedException();
    }

    public void WebPageOnPost(Dictionary<string, string> p_PostData)
    {
        throw new System.NotImplementedException();
    }
}