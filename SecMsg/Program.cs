using Alchemy;
using Alchemy.Classes;
using Newtonsoft.Json;
using SecMsg.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecMsg
{
    class Program
    {
        public class Client
        {
            public UserContext UserContext { get; set; }
            public PublicKey PubKey { get; set; }
        }
        static WebSocketServer server = null;
        static Dictionary<string, Client> clients = new Dictionary<string, Client>();

        static void Main(string[] args)
        {
            int port = 8100;
            server = new WebSocketServer(port, System.Net.IPAddress.Any)
            {
                OnReceive = OnReceive,
                OnSend = OnSend,
                OnConnected = OnConnect,
                OnDisconnect = OnDisconnect,
                TimeOut = new TimeSpan(0, 5, 0)
            };
            
            server.Start();
            Console.WriteLine("Server started on port " + port.ToString());
            Console.ReadKey();
        }

        public static void OnConnect(UserContext aContext)
        {
            Console.WriteLine("{0},\t Client Connected From : {1}", DateTime.Now.ToString(), aContext.ClientAddress.ToString());
        }

        public static void OnReceive(UserContext aContext)
        {
            try
            {
                var s = aContext.DataFrame.ToString();
                var msg = s.DeserializeAnyMessage();
                HandleMessage(msg, aContext);

            }
            catch (Exception ex)
            {
                Console.WriteLine("{0},\t {1}", DateTime.Now.ToString(), ex.Message.ToString());
            }

        }

        public static void OnSend(UserContext aContext)
        {
            Console.WriteLine("{0},\t Data Sent To : {1}", DateTime.Now.ToString(), aContext.ClientAddress.ToString());
        }
        public static void OnDisconnect(UserContext aContext)
        {
            Console.WriteLine("{0},\t Client Disconnected : {1}", DateTime.Now.ToString(), aContext.ClientAddress.ToString());
        }

        private static void HandleMessage(IMessage msg, UserContext aContext)
        {
            //if(msg is ChatMessage chat)
            //{
            //    var client = clients[chat.To];
                
            //}
            if (msg is ConnectMessage conn)
            {
                var client = new Client { PubKey = conn.PubKey, UserContext = aContext };
                clients.Add(conn.Name, client);
                Broadcast(conn.Name, client);
            }
            if (msg is IMessageTo to)
                Send(to);
        }

        public static void Send(IMessageTo msg)
        {
            try
            {
                var client = clients[msg.To];
                client.UserContext.Send(msg.SerializeMessage());
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        public static void Broadcast(string name, Client client)
        {
            try
            {
                foreach (var item in clients.ToList())
                {
                    Thread.Sleep(200);
                    var m = new ClientJoined { Name = item.Key, Key = item.Value.PubKey }.SerializeMessage();
                    client.UserContext.Send(m);
                }
                foreach (var item in clients.Values.ToList())
                {
                    Thread.Sleep(200);
                    var m = new ClientJoined { Name = name, Key = client.PubKey }.SerializeMessage();
                    item.UserContext.Send(m);
                }
                
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
    }
}
