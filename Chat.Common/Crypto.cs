using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Chat.Common
{
    public class Crypto
    {
        //DiffieHellman diffieHellman = null;
        RSACryptoServiceProvider rsa = null;
        RSAParameters privKey = default(RSAParameters);
        RSAParameters pubKey = default(RSAParameters);
        public Crypto()
        {
            //diffieHellman = new DiffieHellman();
            rsa = new RSACryptoServiceProvider();
            privKey = rsa.ExportParameters(true);
            pubKey = rsa.ExportParameters(false);
        }
        
        public PublicKey GetPubKey()
        {
            string pubKeyString;
            {
                //we need some buffer
                var sw = new System.IO.StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, pubKey);
                //get the string from the stream
                pubKeyString = sw.ToString();
            }
            return new PublicKey { PubKey = pubKeyString, IV = pubKey.D };
        }

        public static byte[] Encrypt(PublicKey key, string text)
        {
            RSAParameters publicKey;
            //converting it back
            {
                //get a stream from the string
                var sr = new System.IO.StringReader(key.PubKey);
                //we need a deserializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //get the object back from the stream
                publicKey = (RSAParameters)xs.Deserialize(sr);
            }


            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);

            //we need some data to encrypt
            //var plainTextData = "foobar";

            //for encryption, always handle bytes...
            var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(text);

            //apply pkcs#1.5 padding and encrypt our data 
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
            return bytesCypherText;

            //we might want a string representation of our cypher text... base64 will do
            //var cypherText = Convert.ToBase64String(bytesCypherText);


            //return diffieHellman.Encrypt(key.PubKey, text);
        }

        public string Decrypt(byte[] message)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privKey);

            //decrypt and strip pkcs#1.5 padding
            var bytesPlainTextData = csp.Decrypt(message, false);

            //get our original plainText back...
            var plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);
            return plainTextData;
            //Debug.WriteLine("dec: " + string.Join(" ", key.PubKey));
            //var temp = message;//diffieHellman.Encrypt(this.diffieHellman.PublicKey, "hi");
            //var res = diffieHellman.Decrypt(key.PubKey, temp, key.IV);
            //return res;
        }
    }
}
