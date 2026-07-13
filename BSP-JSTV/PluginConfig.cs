using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSP_JSTV {
    public class PluginConfig {
        public static PluginConfig Instance { get; set; }
        public string ApplicationID { get; set; } = "";
        public string ClientID { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public int Port { get; set; } = 6970;

        public string UserName { get; set; }
        public string ChannelID { get; set; }
        public string UserAccessToken { get; set; }
        public string UserRefreshToken { get; set; }

        public virtual void Changed() { }
        public virtual void OnReload() { }
    }
}
