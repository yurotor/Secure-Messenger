using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Common
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

        public static T DeserializeMessage<T>(this string message) where T : IMessage
        {
            var obj = JsonConvert.DeserializeObject(message, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            if (obj is T t)
                return t;

            throw new Exception();
        }

        public static IMessage DeserializeAnyMessage(this string message)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(message, new JsonSerializerSettings
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
                return "Chat: " + decrypt(m.Message);
            return string.Empty;
        }
    }
}
