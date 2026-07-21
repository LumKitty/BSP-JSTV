using CP_SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSP_JSTV.JSTVEvents {
    internal class JSTVSubscriptionEvent : IChatSubscriptionEvent {
        public string DisplayName { get; set; }
        public string SubPlan { get; set; }
        public bool IsGift { get; set; }
    }
}
