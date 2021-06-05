using System;
using System.Collections.Generic;

namespace Chat.Common
{
    public interface IMessage { }
    public interface IMessageTo : IMessage { string To { get; set; } }
    public class Message
    {
        public MessageType Type { get; set; }
        public string Payload { get; set; }
    }

    public enum MessageType
    {
        Connect,
        Joined,
        Say
    }

    public class PublicKey
    {
        public string PubKey { get; set; }
        public byte[] IV { get; set; }
    }

    public class ConnectMessage : IMessage
    {
        public string Name { get; set; }
        public PublicKey PubKey { get; set; }

        public override string ToString()
        {
            return "Connect " + Name + " using pubkey " + string.Join(" ", PubKey.PubKey);
        }
    }

    public class ClientJoined : IMessage
    {
        public Dictionary<string, PublicKey> AllClients { get; set; }

        public override string ToString()
        {
            return AllClients.Count.ToString() + " clients online";
        }
    }

    public class CreateSecureChannel : IMessageTo
    {
        public string From { get; set; }
        public string To { get; set; }
        public byte[] SessionKey { get; set; }

        public override string ToString()
        {
            return "Create secure channel with " + From + " using session key " + System.Text.Encoding.ASCII.GetString(SessionKey);
        }
    }

    public class AcceptSecureChannel : IMessageTo
    {
        public string From { get; set; }
        public string To { get; set; }

        public override string ToString()
        {
            return "Secure channel with " + From + " is created";
        }
    }

    public class ChatMessage : IMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public byte[] Signature { get; set; }

        public override string ToString()
        {
            return From + " said " + Message + " to " + To;
        }
    }
}
