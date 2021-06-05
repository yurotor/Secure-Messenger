using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SecMsg.Common
{
    public interface IMessage { }
    public interface IMessageTo : IMessage { string To { get; set; } }
    public interface ISigned { byte[] Signature { get; set; } }
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
    [Serializable]
    public class PublicKey
    {
        public byte[] Exp { get; set; }
        public byte[] Mod { get; set; }
    }

    [Serializable]
    public class ConnectMessage : IMessage
    {
        public string Name { get; set; }
        public PublicKey PubKey { get; set; }

        public override string ToString()
        {
            return "Connect " + Name + " using pubkey " + string.Join(" ", PubKey.Mod);
        }
    }
    [Serializable]
    public class ClientJoined : IMessage
    {
        public string Name { get; set; }
        public PublicKey Key { get; set; }

        public override string ToString()
        {
            return "New client joined";
        }
    }
    [Serializable]
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

    public class ChatMessage : IMessageTo, ISigned
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
