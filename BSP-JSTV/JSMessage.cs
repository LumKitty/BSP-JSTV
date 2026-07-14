using CP_SDK.Chat;
using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.SimpleJSON;
using CP_SDK_WebSocketSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace BSP_JSTV {

    internal class JSMessage {
        
        internal static void Log(string Message) { }

        internal static void MessageReceived(object sender, MessageEventArgs args) {
            try {
                JObject Results = JObject.Parse(args.Data);
                if (Results.ContainsKey("identifier")) {
                    if (Results.ContainsKey("message")) {
                        //VNyan_JSTV.Log("Message subkey:" + Results["message"].ToString());
                        JObject Message = (JObject)Results["message"];
                        if (Message.ContainsKey("event")) {
                            string EventClass = Message["event"].ToString().ToLower();

                            WSChatUser usr = new WSChatUser();
                            WSChatChannel chan = new WSChatChannel(PluginConfig.Instance.UserName);
                            //chan.Name = PluginConfig.Instance.UserName; ;

                            switch (EventClass) {
                                case "chatmessage":

                                    WSChatMessage msg = new WSChatMessage();
                                    

                                    usr.UserName = Message["author"]["username"].ToString();
                                    usr.IsSubscriber = bool.Parse(Message["author"]["isSubscriber"].ToString());
                                    usr.IsBroadcaster = bool.Parse(Message["author"]["isStreamer"].ToString());
                                    usr.IsModerator = bool.Parse(Message["author"]["isModerator"].ToString());

                                    msg.Message = Message["text"].ToString();
                                    msg.Sender = usr;
                                    msg.Channel = WSChatService.channel;

                                    string TempMessage = " " + msg.Message + " ";

                                    if (PluginConfig.Instance.ShowEmotes) {
                                        if (Message.ContainsKey("emotesUsed")) {
                                            WebClient wc = new WebClient();
                                            Plugin.Log.Debug("Emote: Parsing: |" + TempMessage + "|");
                                            JArray JEmotes = (JArray)Message["emotesUsed"];
                                            List<WSChatEmote> ChatEmotes = new List<WSChatEmote>();
                                            foreach (JObject JEmote in JEmotes) {
                                                string URL = JEmote["url"].ToString();
                                                string Name = JEmote["code"].ToString();
                                                Plugin.Log.Debug("Emote: " + Name + ", URL: " + URL);
                                                List<int> indices = TempMessage.AllIndexesOf(" " + Name + " ");
                                                int NameLen = Name.Length;
                                                foreach (int Index in indices) {
                                                    Plugin.Log.Debug("Emote: |" + Name + "| Found: " + TempMessage.Substring(Index + 1, NameLen));
                                                    ChatEmotes.Add(new WSChatEmote(Name, URL, Index, Index + NameLen - 1));
                                                }
                                            }
                                            ChatEmotes.Sort((x, y) => y.StartIndex.CompareTo(x.StartIndex));
                                            msg.Emotes = ChatEmotes.ToArray();
                                        }
                                    }

                                    Plugin.service.m_OnTextMessageReceivedCallbacks?.InvokeAll(Plugin.service, msg);

                                    break;
                                case "streamevent":
                                    string UserName = "";
                                    string Item = "";
                                    int Value1 = 0;
                                    int Value2 = 0;
                                    int Value3 = 0;
                                    string TempTime = "";
                                    string EventType = Message["type"].ToString();
                                    Log("Received stream event of type: " + EventType);
                                    if (Message.ContainsKey("metadata")) {
                                        //VNyan_JSTV.Log("Metadata found. Parsing");
                                        //VNyan_JSTV.Log(Message["metadata"].GetType().ToString());
                                        //;VNyan_JSTV.Log(Message["metadata"].ToString());
                                        JObject Metadata = JObject.Parse(Message["metadata"].ToString()); // I hate this
                                                                                                            //VNyan_JSTV.Log(Metadata.ToString());
                                        switch (EventType) {
                                            case "FollowerCountUpdated":
                                                //ProcessJObject(Metadata, "number_of_followers", ref Value1);
                                                break;
                                            case "SubscriberCountUpdated":
                                                //ProcessJObject(Metadata, "number_of_subscribers", ref Value1);
                                                break;
                                            case "ViewerCountUpdated":
                                                ProcessJObject(Metadata, "number_of_viewers", ref Value1);
                                                WSChatService.channel.ViewerCount = Value1;
                                                break;
                                            case "DropinStream":  // raid out
                                                //ProcessJObject(Metadata, "destination", ref UserName);
                                                //ProcessJObject(Metadata, "number_of_viewers", ref Value1);
                                                break;
                                            case "TipGoalDeleted":
                                            case "TipGoalUpdated":
                                            case "TipMenuItemUnlocked":
                                            case "TipMenuItemLocked":
                                            case "TipGoalCreated":
                                                //ProcessJObject(Metadata, "title", ref Item);
                                                //ProcessJObject(Metadata, "amount", ref Value1);
                                                break;
                                            case "TipGoalIncreased":
                                                //ProcessJObject(Metadata, "title", ref Item);
                                                //ProcessJObject(Metadata, "amount", ref Value1);
                                                //ProcessJObject(Metadata, "current", ref Value2);
                                                //ProcessJObject(Metadata, "previous", ref Value3);
                                                break;
                                            case "Followed":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                usr.UserName = UserName;
                                                Plugin.service.m_OnChannelFollowCallbacks?.InvokeAll(Plugin.service, chan, usr);
                                                break;
                                            case "UserMuted":
                                            case "UserUnmuted":
                                            case "ChatTimersCleared":
                                            case "Ended":
                                            case "StreamEnding":
                                            case "StreamModeUpdated":
                                            case "StreamResuming":
                                            case "Started":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                break;
                                            case "StreamDroppedIn":  // raid in
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                ProcessJObject(Metadata, "number_of_viewers", ref Value1);
                                                usr.UserName = UserName;
                                                Plugin.service.m_OnChannelRaidCallbacks?.InvokeAll(Plugin.service, chan, usr, Value1);
                                                break;
                                            case "GiftedSubscriptions":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                ProcessJObject(Metadata, "how_much", ref Value1);
                                                Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, $"{UserName} gifted {Value1} subscriptions");
                                                break;
                                            case "MilestoneCompleted":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                ProcessJObject(Metadata, "title", ref Item);
                                                ProcessJObject(Metadata, "amount", ref Value1);
                                                Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, $"{UserName} met milestone {Value1} ({Item})");
                                                break;
                                            case "Resubscribed":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                ProcessJObject(Metadata, "how_much", ref Value1);
                                                ProcessJObject(Metadata, "how_long", ref Value2);
                                                usr.UserName = UserName;
                                                //Plugin.service.m_OnChannelSubscriptionCallbacks?.InvokeAll(Plugin.service, chan, usr); //TODO: What is IChatSubscriptionEvent?
                                                Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, $"{UserName} resubscribed {Value1} ({Value2})");
                                                break;
                                            case "Subscribed":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                ProcessJObject(Metadata, "how_much", ref Value1);
                                                usr.UserName = UserName;
                                                Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, $"{UserName} subscribed {Value1}");
                                                //Plugin.service.m_OnChannelSubscriptionCallbacks?.InvokeAll(Plugin.service, chan, usr); //TODO: What is IChatSubscriptionEvent?
                                                break;
                                            case "TipGoalMet":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                ProcessJObject(Metadata, "title", ref Item);
                                                ProcessJObject(Metadata, "amount", ref Value1);
                                                Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, $"{UserName} met tip goal {Value1} ({Item})");
                                                break;
                                            case "Tipped":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                ProcessJObject(Metadata, "tip_menu_item", ref Item);
                                                ProcessJObject(Metadata, "how_much", ref Value1);
                                                Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, $"{UserName} tipped {Value1} ({Item})");
                                                //Plugin.service.m_OnChannelBitsCallbacks?.InvokeAll(Plugin.service, chan, usr, Value1);
                                                break;
                                            case "WheelSpinClaimed":
                                                ProcessJObject(Metadata, "who", ref UserName);
                                                ProcessJObject(Metadata, "prize", ref Item);
                                                ProcessJObject(Metadata, "how_much", ref Value1);
                                                Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, $"{UserName} spun for {Value1} ({Item})");
                                                break;
                                            case "ChatTimerStarted":
                                                ProcessJObject(Metadata, "name", ref Item);
                                                ProcessJObject(Metadata, "endsAt", ref TempTime);
                                                Value1 = ISO8601toMilisecondTimespan(TempTime);
                                                break;
                                            case "PvpSessionRequested":
                                            case "PvpSessionReady":
                                            case "PvpSessionEnded":
                                            case "PvpSessionEnding":
                                            case "PvpSessionStarted":
                                                //ProcessJObject(Metadata, "where", ref UserName);
                                                //ProcessJObject(Metadata, "when", ref TempTime);
                                                //Value1 = ISO8601toMilisecondTimespan(TempTime);
                                                break;
                                                //These events exist, but have no data so are handled automatically
                                                //DeviceConnected
                                                //DeviceDisconnected
                                                //DeviceSettingsUpdated
                                                //SettingsUpdated
                                        }
                                    }
                                   
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Plugin.Log.Debug("ERROR: " + ex.Message);
                Plugin.Log.Debug("ERROR: " + ex.InnerException);
            }
        }

        private static int ISO8601toMilisecondTimespan(string TimeStamp) {
            DateTime EndTime = DateTime.Parse(TimeStamp, null, DateTimeStyles.RoundtripKind);
            TimeSpan TimerDuration = EndTime - DateTime.UtcNow;
            return (int)TimerDuration.TotalMilliseconds;
        }

        private static void ProcessJObject(JObject jObject, string KeyName, ref int Result) {
            if (jObject.ContainsKey(KeyName)) {
                //VNyan_JSTV.Log("Parsing int: " + KeyName);
                int.TryParse(jObject[KeyName].ToString(), out Result);
            }
        }
        private static void ProcessJObject(JObject jObject, string KeyName, ref string Result) {
            if (jObject.ContainsKey(KeyName)) {
                //VNyan_JSTV.Log("Parsing string: " + KeyName);
                Result = jObject[KeyName].ToString();
            }
        }

        internal static void ServerConnected() {
            //VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("_lum_jstv_connected", 1);
            //VNyan_JSTV.CallVNyan("_lum_jstv_connected", 0, 0, 0, "", "", "");
        }

        internal static void ServerDisconnected() {
            //VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("_lum_jstv_connected", 0);
            //VNyan_JSTV.CallVNyan("_lum_jstv_disconnected", 0, 0, 0, "", "", "");
        }

        internal static void SaveSettings() {
            //VNyan_JSTV.SaveSettings();
        }
    }
}
