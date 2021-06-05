using System;
using System.Collections.Generic;
using Alchemy;
using Alchemy.Classes;
using Chat.Common;

namespace SecureMessenger
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
            if (msg is ConnectMessage conn)
                clients.Add(conn.Name, new Client { PubKey = conn.PubKey, UserContext = aContext });
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
    }
}
