using PinionCore.Consoles.Chat1.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
namespace PinionCore.NetSync.Samples.Chat
{
    public interface IConnect
    {
        void Connect(string endpoint,bool gate);
    }
    public class Controller : MonoBehaviour
    {
        public UnityEngine.GameObject ConnectPanel;
        public UnityEngine.UI.Toggle GateToggle;
        
        
        public TMPro.TMP_InputField EndpointText;
        public UnityEngine.UI.Button ConnectButton;
        public UnityEngine.UI.Button ConnectToDemoButton;

        public UnityEngine.GameObject LoginPanel;
        public TMPro.TMP_InputField NameText;
        public UnityEngine.UI.Button LoginButton;

        public UnityEngine.GameObject ChatPanel;
        public TMPro.TMP_InputField PrivateNameText;
        public TMPro.TMP_Text ChatMessagesText;
        public TMPro.TMP_InputField SendMessageText;
        public UnityEngine.UI.Button SendButton;

        public TMPro.TMP_Text Ping;


        public UnityEngine.GameObject MessagePanel;
        public TMPro.TMP_Text MessageText;
        public UnityEngine.UI.Button MessageButton;
        




        readonly System.Collections.Generic.List<UnityAction> _Releases;
        public Controller()
        {
            _Releases = new System.Collections.Generic.List<UnityAction>();
        }

        public void Start()
        {

            ConnectPanel.SetActive(false);
            LoginPanel.SetActive(false);
            ChatPanel.SetActive(false);
        }

        public void OnPing(float seconds)
        {
            Ping.text = $"{seconds}";
        }
        public void OnConnectSupply(IConnect connect)
        {
            UnityAction call = () =>
            {
                connect.Connect(EndpointText.text, GateToggle.isOn);
            };

            ConnectButton.onClick.AddListener(call);

            UnityAction demoCall = () =>
            {
                connect.Connect("wss://ws.pinioncore.dpdns.org", true);
            };

            ConnectToDemoButton.onClick.AddListener(demoCall);
            _Releases.Add(() => {
                ConnectToDemoButton.onClick.RemoveListener(demoCall);
                ConnectButton.onClick.RemoveListener(call);
            });
            ConnectPanel.SetActive(true);
        }



        public void OnConnectUnsupply(IConnect connect)
        {
            ConnectPanel.SetActive(false);
            foreach (var release in _Releases)
            {
                release();
            }
            _Releases.Clear();
        }



        public void OnLoginSupply(ILogin login)
        {
            UnityAction call = () =>
            {
                login.Login(NameText.text);
            };
            
            LoginButton.onClick.AddListener(call);
            LoginPanel.SetActive(true);

            _Releases.Add(() => { 
                LoginButton.onClick.RemoveListener(call);
            });
        }
        public void OnLoginUnsupply(ILogin login)
        {

            LoginPanel.SetActive(false);
            foreach (var release in _Releases)
            {
                release();
            }
            _Releases.Clear();
        }

        public void OnPlayerUnsupply(IPlayer player)
        {
            ChatPanel.SetActive(false);
            foreach (var release in _Releases)
            {
                release();
            }
            _Releases.Clear();
        }
        public void OnPlayerAdded(IPlayer player)
        {
            var chatMessages = new Stack<string>();
            void announceMessageHandler(Message message)
            {
                _PushMessage(chatMessages , ChatMessagesText.text , $"[Announce]{message.Name}:{message.Context}\n");
                
            }
            player.AnnounceEvent += announceMessageHandler;

            void publicMessageHandler(Message message)
            {
                _PushMessage(chatMessages, ChatMessagesText.text, $"[Public]{message.Name}:{message.Context}\n");
            }
            player.PublicMessageEvent += publicMessageHandler;
            void privateMessageHandler(Message message)
            {
                _PushMessage(chatMessages, ChatMessagesText.text, $"[Private]{message.Name}:{message.Context}\n");
            }
            player.PrivateMessageEvent += privateMessageHandler;

            var chatters= new System.Collections.Generic.Dictionary<string, IChatter>();
            void chatterSupply(IChatter chatter)
            {
                chatters.Add(chatter.Name.Value, chatter);
            }

            player.Chatters.Base.Supply += chatterSupply;

            void chatterUnsupply(IChatter chatter)
            {
                if (chatters.ContainsKey(chatter.Name.Value))
                {
                    chatters.Remove(chatter.Name.Value);
                }
            }
            player.Chatters.Base.Unsupply += chatterUnsupply;

            UnityAction call = () =>
            {
                if(string.IsNullOrEmpty(PrivateNameText.text))
                {
                    _SendPublic(player, SendMessageText.text);
                }
                else
                {
                    _SendPrivate(SendMessageText.text , PrivateNameText.text , chatters);
                }
                SendMessageText.text = string.Empty;
            };
            
            SendButton.onClick.AddListener(call);
            ChatPanel.SetActive(true);

            _Releases.Add(() => { 
                SendButton.onClick.RemoveListener(call);
                player.Chatters.Base.Supply -= chatterSupply;
                player.Chatters.Base.Unsupply -= chatterUnsupply;
                
                player.PrivateMessageEvent -= privateMessageHandler;
                player.PublicMessageEvent -= publicMessageHandler;
                player.AnnounceEvent -= announceMessageHandler;

            });
        }

        private void _PushMessage(Stack<string> chatMessages, string text, string v)
        {
            // 100 lines max
            chatMessages.Push(v);
            while(chatMessages.Count > 100)
            {
                chatMessages.Pop();
            }
            
            ChatMessagesText.text = string.Join("\r\n", chatMessages);
        }

        private void _SendPrivate(string sendMessage, string name, Dictionary<string, IChatter> chatters)
        {
            if (chatters.ContainsKey(name))
            {
                chatters[name].Whisper(sendMessage);
            } 

        }

        private void _SendPublic(IPlayer player,string message)
        {
            player.Send(message);
        }
    }

}
