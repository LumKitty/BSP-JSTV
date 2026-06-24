using CP_SDK.Chat.Interfaces;

namespace BSPWS;

internal class WSChatChannel : IChatChannel
{
    public string Id => "mdws_"+Name;

    public string Name {get; internal set;} = "NULLCHANNEL";

    public bool IsTemp => false;

    public string Prefix => "";

    public bool CanSendMessages => false;

    public bool Live => true;

    public int ViewerCount => 100;
}