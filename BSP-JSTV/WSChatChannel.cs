using CP_SDK.Chat.Interfaces;

namespace BSP_JSTV;

internal class WSChatChannel : IChatChannel
{
    private string _Name = "NULLCHANNEL";
    
    public string Id => "mdws_"+Name;

    public string Name { get { return _Name; } set { _Name = value; } }

    public bool IsTemp => false;

    public string Prefix => "";

    public bool CanSendMessages => true;

    public bool Live => true;

    public int ViewerCount { get; internal set; } = 69;

    public WSChatChannel(string __Name) {
        _Name = __Name;
    }
}