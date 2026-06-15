using CP_SDK.Chat.Interfaces;

namespace BSPWS;

internal class WSChatMessage : IChatMessage
{
    public string Id => throw new System.NotImplementedException();

    public bool IsSystemMessage => throw new System.NotImplementedException();

    public bool IsActionMessage => throw new System.NotImplementedException();

    public bool IsHighlighted => throw new System.NotImplementedException();

    public bool IsGiganticEmote => throw new System.NotImplementedException();

    public bool IsPing => throw new System.NotImplementedException();

    public string Message => throw new System.NotImplementedException();

    public IChatUser Sender => throw new System.NotImplementedException();

    public IChatChannel Channel => throw new System.NotImplementedException();

    public IChatEmote[] Emotes => throw new System.NotImplementedException();
}