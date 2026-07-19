using CP_SDK.Chat.Interfaces;
using CP_SDK_WebSocketSharp;

namespace BSP_JSTV;

internal class WSChatChannel : IChatChannel {
    private string _Name = "NULLCHANNEL";
    internal string _id = "";
    public string Id { get {
            if (_id.IsNullOrEmpty()) {
                return "lkjstv_"+_Name;
            }
            else {
                return "lkjstv_"+_id;
            }
        }
    }
    public string Name { get { return _Name; } set { _Name = value; } }
    public bool IsTemp => false;
    public string Prefix => "";
    public bool CanSendMessages => true;
    public bool Live => true;
    public int ViewerCount { get; internal set; } = 0;
    public WSChatChannel(string __Name) {
        _Name = __Name;
    }
}