using System.Runtime.Remoting.Messaging;
using CP_SDK.Chat.Interfaces;

namespace BSP_JSTV;

internal class WSChatUser : IChatUser
{
    public string Id => "mdws_"+UserName;

    public string UserName {get; internal set;} = "NULLUSER";

    public string DisplayName => UserName;

    public string PaintedName => UserName;

    public string Color => "#FFFFFF";

    public bool IsBroadcaster => false;

    public bool IsModerator => false;

    public bool IsSubscriber => false;

    public bool IsVip => false;

    public IChatBadge[] Badges => [];
}