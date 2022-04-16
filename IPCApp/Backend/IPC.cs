using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dec.DiscordIPC;
using Dec.DiscordIPC.Core;
using Dec.DiscordIPC.Commands;
using Dec.DiscordIPC.Events;
using Dec.DiscordIPC.Entities;

using RestSharp;
using RestSharp.Authenticators.OAuth2;
using Newtonsoft.Json;

namespace Speedberg.Discord
{
    public static class IPC
    {
        //The number of pipelines to attempt to connect to
        public const int CLIENT_LIMIT = 5;
        private const string apiEndpoint = "https://discord.com/api";
        private const string tokenEndpoint = "oauth2/token";
        private const string redirectUri = "http://127.0.0.1";

        public static bool INITIALIZED {get; private set;}
        public static DiscordIPC m_client;

        public static string CLIENT_ID;
        private static string CLIENT_SECRET;

        private static bool _authorized;

        public static bool LoggedIn {get; private set;}
        public static Client CurrentClient {get; private set;}

        private static List<Client> _cachedClients;
        public static Client[] ActiveClients { get { return _cachedClients.ToArray();}}

        private static Ready.Data _fetchedReadyData;

        private static async Task<bool> _Initialize(string clientID, string clientSECRET)
        {
            if(IPC.INITIALIZED)
            {
                return true;
            }
            CLIENT_ID = clientID;
            CLIENT_SECRET = clientSECRET;
            LoggedIn = false;
            _cachedClients = new List<Client>();

            CreateClient();
            try
            {
                await m_client.InitAsync().TimeoutAfter(5000);
            } catch {
                m_client.Dispose();
                INITIALIZED = false;
                _authorized = false;
                return false;
            }
            return await _DetectClients();
        }

        public static async void Initialize(string clientID, string clientSECRET, Action<bool> callback)
        {
            bool result = await _Initialize(clientID,clientSECRET);
            callback.Invoke(result);
        }

        private static void CreateClient()
        {
            if(INITIALIZED) return;

            //Start client
            try
            {
                m_client = new DiscordIPC(CLIENT_ID);
                
                //Subscribe to OnReady event
                EventHandler<Ready.Data> handler = (sender, data) => {
                    _fetchedReadyData = data;
                };
                m_client.OnReady += handler;

                INITIALIZED = true;
                _authorized = false;
            } catch {
                INITIALIZED = false;
                _authorized = false;
            }
        }

        private static async Task<bool> _DetectClients()
        {
            if(!INITIALIZED) return false;

            _cachedClients.Clear();
            int pipelineCount = 0;

            for(int i = 0; i < IPC.CLIENT_LIMIT; i++)
            {
                try
                {
                    await m_client.InitAsync(i);
                    pipelineCount += 1;
                    Client client = new Client(i,_fetchedReadyData);
                    _cachedClients.Add(client);
                } catch {
                    Console.WriteLine("No Discord client on pipeline {0} detected.",i);
                }
            }

            //No Discord clients open
            if(pipelineCount <= 0)
            {
                return false;
            } else {
                return true;
            }
        }

        public static async void DetectClients(Action<bool> callback = null)
        {
            bool result = await _DetectClients();
            callback?.Invoke(result);
        }

        private static async Task<bool> _LoginAsClient(int pipelineID)
        {
            if(!INITIALIZED) return false;
            if(LoggedIn) return false;

            try
            {
                foreach(Client client in _cachedClients)
                {
                    if(client.pipelineID == pipelineID)
                    {
                        await m_client.InitAsync(pipelineID);
                        CurrentClient = client;
                        LoggedIn = true;
                        return true;
                    }
                }
                return false;
            } catch{
                return false;
            }
        }

        public static async void LoginAsClient(int pipelineID, Action<bool> callback = null)
        {
            bool result = await _LoginAsClient(pipelineID);
            callback?.Invoke(result);
        }

        private static async Task<bool> _Logout()
        {
            if(!INITIALIZED) return false;
            if(!LoggedIn) return false;
            try
            {
                m_client.Dispose();
                INITIALIZED = false;
                LoggedIn = false;
                _authorized = false;
                //2 second delay just to make sure
                await Task.Delay(10);
                CreateClient();
                return true;
            } catch {
                return false;
            }
        }

        public static async void Logout(Action<bool> callback = null)
        {
            bool result = await _Logout();
            callback?.Invoke(result);
        }

        private static async Task _UpdatePresence(Presence.Activity activity)
        {
            SetActivity.Args args = new SetActivity.Args();
            args.pid = 69420;
            args.activity = activity;
                
            await IPC.m_client.SendCommandAsync(args);
        }

        public static async void UpdatePresence(Presence.Activity activity, Action<string> callback = null)
        {
            try
            {
                await _UpdatePresence(activity);
                callback?.Invoke(null);
            }catch(Exception e)
            {
                callback?.Invoke(e.Message);
            }
        }

        public static void Dispose()
        {
            if(!INITIALIZED) return;
            m_client.Dispose();
            INITIALIZED = false;
            _authorized = false;
            LoggedIn = false;
        }

        public struct Client
        {
            public int pipelineID;
            public Ready.Data data;
            public string username;

            public Client(int pipelineID, Ready.Data data)
            {
                this.pipelineID = pipelineID;
                this.data = data;
                this.username = data.user.username + "#" + data.user.discriminator;
            }

            public string GetAvatarURL()
            {
                return $"https://cdn.discordapp.com/avatars/{data.user.id}/{data.user.avatar}.png";
            }
        }

        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
                await task;
            else
                throw new TimeoutException();
        }
    }
}