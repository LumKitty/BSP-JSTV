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

namespace BSP_JSTV {
    internal class JSTV {
        //internal static string UserName = "";
        //internal static string ChannelID = "";
        //internal static string ApplicationID = "";
        //internal static string ClientID = "";
        //internal static string ClientSecret = "null-clientsecret";
        //internal static int Port = 6969;

        internal static string RedirURL = "";
        internal static string EncodedAuth = "null-encodedauth";
        //internal static string UserAccessToken = "null-useraccesstoken";
        //internal static string UserRefreshToken = "null-refreshtoken";
        internal static bool ConnectionWanted = true;

        internal static string TempAuthCode = "";
        internal static string TempState = "";
        internal static bool UserConnected = false;
        internal static bool BotConnected = false;
        internal static string State = GenerateRandomState();
        private static CP_SDK_WebSocketSharp.WebSocket wsClient;
        private static System.Threading.CancellationToken CT = new System.Threading.CancellationToken();


        internal static void Log(string Message) {
            Plugin.Log.Debug(Message);
        }
        internal static void ErrorHandler(Exception ex) { 
            Plugin.Log.Error(ex.ToString()); 
        }

        internal static async void ConnectJSTV() {
            try {
                AuthoriseUser();
                while (!UserConnected) { System.Threading.Thread.Sleep(100); }

                Log("Authorised user. Connecting bot");
                ConnectionWanted = true;
                wsClient = new CP_SDK_WebSocketSharp.WebSocket("wss://api.joystick.tv/cable?token=" + EncodedAuth, "actioncable-v1-json");
                wsClient.OnOpen += ServerConnected;
                wsClient.OnClose += ServerDisconnected;
                wsClient.OnError += ServerDisconnected;
                wsClient.OnMessage += JSMessage.MessageReceived;
                wsClient.Connect();


                while (!BotConnected) {
                    //Console.Write(".");
                    System.Threading.Thread.Sleep(100);
                }

                Log("Bot connected. Sending subscribe message");

                JObject Message = new JObject(
                    new JProperty("command", "subscribe"),
                    new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}")
                );
                WSSend(ref Message);
            } catch (Exception ex) {
                ErrorHandler(ex);
            }
        }

        internal static async void DisconnectJSTV() {
            ConnectionWanted = false;
            wsClient.Close();
        }

        static async void ServerConnected(object sender, EventArgs args) {
            Log("Server connected");
            BotConnected = true;
            JSMessage.ServerConnected();
        }

        static async void ServerDisconnected(object sender, EventArgs args) {
            Log("Server disconnected");
            if (ConnectionWanted) { ConnectJSTV(); }
            Log(args.ToString());
            BotConnected = false;
            JSMessage.ServerDisconnected();
        }

        internal static void WSSend(ref JObject json) {
            string data = JsonConvert.SerializeObject(json);
            Log("WS Sending: " + data);
            wsClient.Send(data);
        }

        private static string GenerateRandomState() {
            //TODO: Actually make random
            return "piss";
        }

        private static HttpResponseMessage? MakeHttpRequest(HttpRequestMessage requestMessage) {
            try {
                System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();
                Log("Sending");
                Task<HttpResponseMessage> authCodeTask = Http.SendAsync(requestMessage);
                Log("Waiting");
                if (authCodeTask.Wait(5000)) {
                    Log("Got Result");
                    return authCodeTask.Result;
                } else {
                    Log("Failed to get server response code in time");
                    return null;
                }
            } catch (Exception e) {
                Log(e.ToString());
                return null;
            }
        }

        private static string? HttpResponseToContent(HttpResponseMessage response) {
            Task<String> authCodeReader = response.Content.ReadAsStringAsync();
            if (authCodeReader.Wait(5000)) {
                return authCodeReader.Result;
            } else {
                Log("Failed to read server in time");
                return null;
            }
        }

        private static string? MakeHttpRequestString(HttpRequestMessage requestMessage) { 
            return HttpResponseToContent(MakeHttpRequest(requestMessage));
        }

        internal static void SendChatMessage(string Message) {
            JObject MessageJSON = new JObject(
                new JProperty("command", "message"),
                new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}"),
                new JProperty("data", new JObject(
                    new JProperty("action", "send_message"),
                    new JProperty("text", Message),
                    new JProperty("channelId", PluginConfig.Instance.ChannelID)
                ).ToString())
            );
            JSTV.Log(JsonConvert.SerializeObject(MessageJSON));
            WSSend(ref MessageJSON);
        }

        internal static void SendWhisper(string Message, string UserName) {
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

        public static void AuthoriseUser() {
            HttpRequestMessage requestMessage;
            System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();
            HttpResponseMessage response;

            string content;
            int Timeout = 60 * 1000;
            int PollFrequency = 100;
            int MaxPolls = Timeout / PollFrequency;


            if (PluginConfig.Instance.UserRefreshToken == null) { PluginConfig.Instance.UserRefreshToken = "null-refreshtoken"; }
            EncodedAuth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(PluginConfig.Instance.ClientID + ":" + PluginConfig.Instance.ClientSecret));

            if (PluginConfig.Instance.UserRefreshToken != "null-refreshtoken") {
                Log("Logging in with refresh code");
                requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.joystick.tv/api/oauth/token?refresh_token=" + PluginConfig.Instance.UserRefreshToken + "&grant_type=refresh_token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodedAuth);
                requestMessage.Headers.Add("X-JOYSTICK-STATE", State);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Content = new StringContent("", Encoding.ASCII, "application/x-www-form-urlencoded");

                //make the request
                response = MakeHttpRequest(requestMessage);
                content = HttpResponseToContent(response);

                Log("Headers: " + response.Headers.ToString());
                if (!response.IsSuccessStatusCode) {
                    Log("Failed with statuscode: " + response.StatusCode.ToString());
                    UserConnected = false;
                } else {
                    JObject JsonContent = JObject.Parse(content);
                    //dynamic JsonContent = JsonConvert.DeserializeObject<dynamic>(content);

                    PluginConfig.Instance.UserAccessToken = JsonContent["access_token"].ToString();
                    PluginConfig.Instance.UserRefreshToken = JsonContent["refresh_token"].ToString();
                    Log(JsonContent.ToString());
                    UserConnected = true;
                    Log("Content: " + content);
                    JSMessage.SaveSettings();
                }
            }
            if (!UserConnected) {
                string[] args = new string[0];

                string url = "https://joystick.tv/api/oauth/authorize?response_type=code&client_id=" + PluginConfig.Instance.ClientID + "&scope=bot&state=" + State;

                Log("Starting webserver");
                HTTPServer Server = new HTTPServer(PluginConfig.Instance.Port);
                Server.Start(State);

                Log("Launching browser");
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });

                Log("Waiting for OAuth response");
                int NumPolls = 0;
                while ((TempState == "") && (TempAuthCode == "") && (NumPolls < MaxPolls)) {
                    System.Threading.Thread.Sleep(PollFrequency);
                    NumPolls++;
                }

                if (NumPolls == MaxPolls) {
                    Log("Timed out waiting for OAuth");
                    UserConnected = false;
                    return;
                }

                Log("Response received");
                //Log("Auth code: " + TempAuthCode);
                Log("State    : " + TempState);

                var Data = new StringContent("", Encoding.ASCII);
                Data.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");


                requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.joystick.tv/api/oauth/token?redirect_uri=.&code=" + TempAuthCode + "&grant_type=authorization_code");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodedAuth);
                requestMessage.Headers.Add("X-JOYSTICK-STATE", State);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Content = new StringContent("", Encoding.ASCII, "application/x-www-form-urlencoded");

                //make the request
                Log("Logging in with auth code: " + requestMessage.RequestUri);

                Log("Sending message");
                response = MakeHttpRequest(requestMessage);
                Log("Receiving message");
                content = HttpResponseToContent(response);

                Log("Headers: " + response.Headers.ToString());
                if (!response.IsSuccessStatusCode) {
                    Log("Failed with statuscode: " + response.StatusCode.ToString());
                    UserConnected = false;
                } else {
                    JObject JsonContent = JObject.Parse(content);
                    //dynamic JsonContent = JsonConvert.DeserializeObject<dynamic>(content);
                    PluginConfig.Instance.UserAccessToken = JsonContent["access_token"].ToString();
                    PluginConfig.Instance.UserRefreshToken = JsonContent["refresh_token"].ToString();
                    Log("Content: " + content);
                    UserConnected = true;
                }
            }
            Log("Requesting Streamer Settings");
            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.joystick.tv/api/users/stream-settings");
            requestMessage.Headers.Add("Authorization", "Bearer " + PluginConfig.Instance.UserAccessToken);
            requestMessage.Headers.Add("X-JOYSTICK-STATE", State);
            //requestMessage.Content = new StringContent("", Encoding.ASCII, "application/json");
            Log("Sending message");
            response = MakeHttpRequest(requestMessage);
            Log("Receiving message");
            content = HttpResponseToContent(response);
            Log("Received");
            if (!response.IsSuccessStatusCode) {
                Log("Failed with statuscode: " + response.StatusCode.ToString());
                Log(content);
                UserConnected = false;
                return;
            } else {
                Log(content);
                JObject JSON = JObject.Parse(content);
                //dynamic JSON = JsonConvert.DeserializeObject<dynamic>(content);
                PluginConfig.Instance.UserName = JSON["username"].ToString();
                PluginConfig.Instance.ChannelID = JSON["channel_id"].ToString();
                Log("Detected username: " + PluginConfig.Instance.UserName);
                Log("Detected channel ID:" + PluginConfig.Instance.ChannelID);
                JSMessage.SaveSettings();
                UserConnected = true;
            }
        }
    }
}
