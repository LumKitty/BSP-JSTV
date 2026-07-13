using System;
using CP_SDK.Chat.Interfaces;

namespace BSP_JSTV;

internal class WSChatMessage : IChatMessage
{
    public string Id {get;} = Guid.NewGuid().ToString();

    public bool IsSystemMessage => false;

    public bool IsActionMessage => false;

    public bool IsHighlighted => false;

    public bool IsGiganticEmote => false;

    public bool IsPing => false;

    public string Message {get; internal set;} = "";

    public IChatUser Sender {get; internal set;}

    public IChatChannel Channel {get; internal set;}

    public IChatEmote[] Emotes => [];
}