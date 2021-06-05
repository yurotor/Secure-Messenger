using Alchemy;
using Alchemy.Classes;
using Alchemy.Handlers.WebSocket.hybi10;
using Newtonsoft.Json;
using SecMsg.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecMsg.Client
{
    public partial class Form1 : Form
    {
        public class ChatState
        {
            public string SessionKey { get; set; }
            public PublicKey PubKey { get; set; }
            public List<ChatMessage> Messages { get; set; }
        }
        WebSocketClient client = null;
        string myName = null;
        Crypto crypto = new Crypto();
        TEA tea = new TEA();
        Dictionary<string, ChatState> otherClients = new Dictionary<string, ChatState>();
        UserContext userContext = null;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            client = new WebSocketClient("ws://127.0.0.1:8100/chat")
            {
                OnConnect = OnConnect,
                OnConnected = OnConneced,
                OnReceive = OnReceive
            };
            client.Connect();
        }

        public void OnReceive(UserContext aContext)
        {
            var msg = aContext.DataFrame.ToString().DeserializeAnyMessage();
            HandleMessage(msg);
        }

        public void OnConnect(UserContext aContext)
        {
            userContext = aContext;
            Console.WriteLine("{0},\t Client Connected From : {1}", DateTime.Now.ToString(), aContext.ClientAddress.ToString());
        }

        public void OnConneced(UserContext aContext)
        {
            Console.WriteLine("{0},\t Client Connected From : {1}", DateTime.Now.ToString(), aContext.ClientAddress.ToString());
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            var client = GetCurrentState();
            if (client == null)
            {
                MessageBox.Show("Select chat partner");
                return;
            }
            var message = tea.EncryptString(textBoxSend.Text, client.SessionKey);
            var msg = new ChatMessage
            {
                From = myName,
                To = listBox1.SelectedItem.ToString(),
                Message = message, 
                Signature = Sig(textBoxSend.Text)
            };
            Write(msg.ToDisplay(m => textBoxSend.Text));
            userContext?.Send(msg.SerializeMessage());
            textBoxSend.Text = string.Empty;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Write("Connected to Secure Chat Server...");

                myName = textBoxName.Text;
                var outStream = new ConnectMessage { Name = textBoxName.Text, PubKey = crypto.GetPubKey() }.SerializeMessage();

                userContext?.Send(outStream);

                textBoxName.Enabled = false;
                buttonConnect.Enabled = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }

        }

        private byte[] Sig(string msg)
        {
            return crypto.Sign(msg);
        }

        private void HandleMessage(IMessage msg)
        {
            if (msg is ClientJoined j)
                AddClient(j);
            else if (msg is ChatMessage c)
                AddMessage(c);
            else if (msg is CreateSecureChannel s)
                InitSecureChannel(s);
        }

        private void Log(string msg)
        {
            Invoke(new Action(delegate ()
            {
                textBoxLog.Text = textBoxLog.Text + Environment.NewLine + msg;
            }));
        }

        private void InitSecureChannel(CreateSecureChannel msg)
        {
            var client = otherClients[msg.From];
            if (client == null)
                return;
            client.SessionKey = crypto.Decrypt(msg.SessionKey);
            Log("Accepting secure channel with " + msg.From + " using session key: " + client.SessionKey);
            EnableChat();
        }

        private void EnableChat()
        {
            Invoke(new Action(delegate ()
            {
                linkLabel1.Enabled = false;
                buttonSend.Enabled = true;
            }));
        }

        private void AddMessage(ChatMessage msg)
        {
            var client = otherClients[msg.From];
            if (client == null)
                return;
            client.Messages.Add(msg);
            var plaintext = tea.Decrypt(msg.Message, client.SessionKey);
            if (Crypto.VerifySignature(plaintext, msg.Signature, client.PubKey))
            {
                Log("The signature is valid.");
            }
            else
            {
                Log("The signature is not valid.");
                return;
            }
            Log(msg.ToString());
            Invoke(new Action(delegate ()
            {
                if (listBox1.SelectedItem != null && listBox1.SelectedItem.ToString() == msg.From)
                    Write(msg.ToDisplay(m => tea.Decrypt(m, client.SessionKey)));
            }));
        }

        private void AddClient(ClientJoined msg)
        {
            try
            {
                if (msg.Name != myName && !otherClients.ContainsKey(msg.Name))
                {
                    otherClients.Add(msg.Name, new ChatState { PubKey = msg.Key, Messages = new List<ChatMessage>(), SessionKey = null });
                    Invoke(new Action(delegate ()
                    {
                        listBox1.Items.Add(msg.Name);
                    }));
                    Log(msg.Name + " has joined");
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        private void Write(string data)
        {
            if (InvokeRequired)
                Invoke(new Action<string>(Write), data);
            else
                textBoxChat.Text = textBoxChat.Text + Environment.NewLine + " >> " + data;
        }

        private void Clear()
        {
            Invoke(new Action(delegate ()
            {
                textBoxChat.Text = string.Empty;
            }));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var client = GetCurrentState();
            if (client != null)
            {
                Clear();
                foreach (var item in client.Messages)
                {
                    Write(item.ToDisplay(m => tea.Decrypt(m, client.SessionKey)));
                }
                Invoke(new Action(delegate ()
                {
                    buttonSend.Enabled = client.SessionKey != null;
                    linkLabel1.Enabled = client.SessionKey == null;
                }));
            }
        }

        private ChatState GetCurrentState()
        {
            if (listBox1.SelectedItem == null)
                return null;

            return otherClients[listBox1.SelectedItem.ToString()];
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var client = GetCurrentState();
            if (client != null)
            {
                if (client.SessionKey == null)
                {
                    client.SessionKey = TEA.GenerateKey();
                    var outStream = new CreateSecureChannel
                    {
                        From = myName,
                        To = listBox1.SelectedItem.ToString(),
                        SessionKey = Crypto.Encrypt(client.PubKey, client.SessionKey)
                    }.SerializeMessage();
                    userContext?.Send(outStream);
                    Log("Initiating secure channel with " + listBox1.SelectedItem.ToString() + " using session key: " + client.SessionKey);
                }
                EnableChat();
            }
        }

    }
}
