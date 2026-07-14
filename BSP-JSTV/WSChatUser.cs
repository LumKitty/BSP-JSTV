using System.Runtime.Remoting.Messaging;
using CP_SDK.Chat.Interfaces;

namespace BSP_JSTV;

internal class WSChatUser : IChatUser {
    public string Id => "lkjstv_"+UserName.ToLower();
    public string UserName {get; internal set;} = "NULLUSER";
    public string DisplayName => UserName;
    public string PaintedName => UserName;
    public string Color => "#FFFFFF";
    public bool IsBroadcaster { get; internal set; } = false;
    public bool IsModerator { get; internal set; } = false;
    public bool IsSubscriber { get; internal set; } = false;
    public bool IsVip { get; internal set; } = false;
    public IChatBadge[] Badges => [];
}