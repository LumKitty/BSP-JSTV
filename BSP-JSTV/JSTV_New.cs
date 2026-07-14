using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CP_SDK_WebSocketSharp;
using System.Threading.Tasks;
using CP_SDK.Chat;

namespace BSP_JSTV
{
    internal class JSTV_New
    {

        internal static string EncodedAuth = "null-encodedauth";
        internal static bool ConnectionWanted = true;

        internal static bool BotConnected = false;
        private static CP_SDK_WebSocketSharp.WebSocket wsClient;

        private int wsRetryCount = 0;


        internal static void Log(string Message)
        {
            Plugin.Log.Info(Message);
        }

        internal void ConnectWebSocket_New()
        {
            EncodedAuth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(PluginConfig.Instance.ClientID + ":" + PluginConfig.Instance.ClientSecret));

            ConnectionWanted = true;

            wsClient = new CP_SDK_WebSocketSharp.WebSocket("wss://api.joystick.tv/cable?token=" + EncodedAuth, "actioncable-v1-json");
            wsClient.OnOpen += ServerConnected_New;
            wsClient.OnClose += ServerDisconnected_New;
            wsClient.OnError += ServerError_New;
            wsClient.OnMessage += ServerMsg_New;
            wsClient.OnMessage += JSMessage.MessageReceived;
            wsClient.Connect();
        }

        internal void ServerConnected_New(object sender, EventArgs args)
        {
            Log("Socket connected. Sending subscribe message");

            JObject Message = new JObject(
                new JProperty("command", "subscribe"),
                new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}")
            );
            WSSend(ref Message);
            Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, "Connecting to JSTV");
        }

        internal void ServerDisconnected_New(object sender, CloseEventArgs args)
        {
            Log("WS disconnected");
            Log(args.ToString());

            Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, "Disconnected with code " + args.Code);

            BotConnected = false;
            System.Threading.Thread.Sleep(5000);
            if (ConnectionWanted && wsRetryCount <6) { ConnectWebSocket_New(); }
            wsRetryCount++;
        }

        internal void ServerError_New(object sender, ErrorEventArgs args)
        {
            Log("WS Error!");
            Log(args.ToString());

            Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, "Disconnected due to error");

            BotConnected = false;
            System.Threading.Thread.Sleep(5000);
            if (ConnectionWanted && wsRetryCount < 6) { ConnectWebSocket_New(); }
            wsRetryCount++;
        }

        internal void ServerMsg_New(object sender, MessageEventArgs args)
        {
            JObject jsonContent = JObject.Parse(args.Data);
            if (jsonContent.ContainsKey("type") && jsonContent.ContainsKey("identifier"))
            {
                if (jsonContent["type"].ToString() == "confirm_subscription")
                {
                    BotConnected = true;
                    Plugin.service.m_OnSystemMessageCallbacks?.InvokeAll(Plugin.service, "Success connecting to JSTV");
                }
            }
        }

        internal bool ExchangeCodeForTokens_New(string code, out string accessToken, out string refreshToken)
        {
            HttpResponseMessage response;
            string content;

            accessToken = "";
            refreshToken = "";


            HttpRequestMessage requestMessage;
            requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.joystick.tv/api/oauth/token?redirect_uri=.&code=" + code + "&grant_type=authorization_code");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodedAuth);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = new StringContent("", Encoding.ASCII, "application/x-www-form-urlencoded");

            Log("Sending message");
            response = MakeHttpRequest(requestMessage);
            Log("Receiving message");
            content = HttpResponseToContent(response);

            if (!response.IsSuccessStatusCode)
            {
                Log("Failed with statuscode: " + response.StatusCode.ToString());
                return false;
            }
            else
            {
                JObject JsonContent = JObject.Parse(content);
                accessToken = JsonContent["access_token"].ToString();
                refreshToken = JsonContent["refresh_token"].ToString();
                Log("Content: " + content);
                return true;
            }
        }

        internal bool ExchangeRefreshForTokens_New(string refreshToken, out string accessToken, out string newRefreshToken)
        {
            HttpResponseMessage response;
            string content;
            HttpRequestMessage requestMessage;

            accessToken = "";
            newRefreshToken = "";


            Log("Logging in with refresh code");
            requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.joystick.tv/api/oauth/token?refresh_token=" + refreshToken + "&grant_type=refresh_token");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodedAuth);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = new StringContent("", Encoding.ASCII, "application/x-www-form-urlencoded");

            //make the request
            response = MakeHttpRequest(requestMessage);
            content = HttpResponseToContent(response);

            Log("Headers: " + response.Headers.ToString());
            if (!response.IsSuccessStatusCode)
            {
                Log("Failed with statuscode: " + response.StatusCode.ToString());
                return false;
            }
            else
            {
                JObject JsonContent = JObject.Parse(content);

                accessToken = JsonContent["access_token"].ToString();
                newRefreshToken = JsonContent["refresh_token"].ToString();
                Log(JsonContent.ToString());
                Log("Content: " + content);
                return true;
            }
        }

        internal void SendChatMessage_New(string message, string channelId)
        {
            JObject MessageJSON = new JObject(
                new JProperty("command", "message"),
                new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}"),
                new JProperty("data", new JObject(
                    new JProperty("action", "send_message"),
                    new JProperty("text", message),
                    new JProperty("channelId", channelId)
                ).ToString())
            );
            Log(JsonConvert.SerializeObject(MessageJSON));
            WSSend(ref MessageJSON);
        }

        internal void SendWhisper_New(string Message, string UserName, string channelId)
        {
            JObject MessageJSON = new JObject(
                new JProperty("command", "message"),
                new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}"),
                new JProperty("data", new JObject(
                    new JProperty("action", "send_message"),
                    new JProperty("username", UserName),
                    new JProperty("text", Message),
                    new JProperty("channelId", PluginConfig.Instance.ChannelID)
                ).ToString())
            );
            Log(JsonConvert.SerializeObject(MessageJSON));
            WSSend(ref MessageJSON);
        }

        internal bool FetchStreamerSettings_New(string accessToken, out string userName, out string channelId)
        {
            HttpResponseMessage response;
            string content;
            HttpRequestMessage requestMessage;

            userName = "";
            channelId = "";

            Log("Requesting Streamer Settings");
            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.joystick.tv/api/users/stream-settings");
            requestMessage.Headers.Add("Authorization", "Bearer " + accessToken);

            Log("Sending message");
            response = MakeHttpRequest(requestMessage);
            Log("Receiving message");
            content = HttpResponseToContent(response);
            Log("Received");
            if (!response.IsSuccessStatusCode)
            {
                Log("Failed with statuscode: " + response.StatusCode.ToString());
                Log(content);
                return false;
            }
            else
            {
                Log(content);
                JObject JSON = JObject.Parse(content);

                userName = JSON["username"].ToString();
                channelId = JSON["channel_id"].ToString();
                Log("Detected username: " + userName);
                Log("Detected channel ID:" + channelId);

                return true;
            }
        }

        internal void DisconnectWebSocket_New()
        {
            ConnectionWanted = false;
            wsClient.Close();
            wsRetryCount = 0;
        }




        internal static void WSSend(ref JObject json)
        {
            string data = JsonConvert.SerializeObject(json);
            Log("WS Sending: " + data);
            wsClient.Send(data);
        }

        private static HttpResponseMessage? MakeHttpRequest(HttpRequestMessage requestMessage)
        {
            try
            {
                System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();
                Log("Sending");
                Task<HttpResponseMessage> authCodeTask = Http.SendAsync(requestMessage);
                Log("Waiting");
                if (authCodeTask.Wait(5000))
                {
                    Log("Got Result");
                    return authCodeTask.Result;
                }
                else
                {
                    Log("Failed to get server response code in time");
                    return null;
                }
            }
            catch (Exception e)
            {
                Log(e.ToString());
                return null;
            }
        }

        private static string? HttpResponseToContent(HttpResponseMessage response)
        {
            Task<String> authCodeReader = response.Content.ReadAsStringAsync();
            if (authCodeReader.Wait(5000))
            {
                return authCodeReader.Result;
            }
            else
            {
                Log("Failed to read server in time");
                return null;
            }
        }

        private static string? MakeHttpRequestString(HttpRequestMessage requestMessage)
        {
            return HttpResponseToContent(MakeHttpRequest(requestMessage));
        }



    }
}
