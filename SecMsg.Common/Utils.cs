using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SecMsg.Common
{
    public static class Utils
    {
        public static string SerializeMessage(this IMessage message)
        {
            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static IMessage DeserializeAnyMessage(this string message)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(message, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                if (obj is IMessage t)
                    return t;
            }
            catch (Exception exc)
            {

                throw;
            }

            throw new Exception();
        }

        public static string ToDisplay(this IMessage message, Func<string, string> decrypt)
        {
            if (message is ConnectMessage t)
                return "Connected: " + t.Name;
            if (message is ClientJoined j)
                return "New client joined";
            if (message is ChatMessage m)
                return m.From + " said " + decrypt(m.Message);
            return string.Empty;
        }
    }
}
