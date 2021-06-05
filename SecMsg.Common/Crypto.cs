using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace SecMsg.Common
{
    public class Crypto
    {
        RSACryptoServiceProvider rsa = null;
        RSAParameters privKey = default(RSAParameters);
        RSAParameters pubKey = default(RSAParameters);
        public Crypto()
        {
            rsa = new RSACryptoServiceProvider();
            privKey = rsa.ExportParameters(true);
            pubKey = rsa.ExportParameters(false);
        }

        public PublicKey GetPubKey()
        {
            return new PublicKey { Exp = pubKey.Exponent, Mod = pubKey.Modulus };
        }

        public static byte[] Encrypt(PublicKey key, string text)
        {
            var publicKey = new RSAParameters { Exponent = key.Exp, Modulus = key.Mod };
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);

            var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(text);

            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
            return bytesCypherText;
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
        }

        public byte[] Sign(string source)
        {
            var data = GetHashValue(source);

            //Create an RSAPKCS1SignatureFormatter object and pass it the
            //RSA instance to transfer the private key.
            RSAPKCS1SignatureFormatter rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);

            //Set the hash algorithm to SHA1.
            rsaFormatter.SetHashAlgorithm("SHA256");

            //Create a signature for hashValue and assign it to
            //signedHashValue.
            var signedHashValue = rsaFormatter.CreateSignature(data);
            return signedHashValue;
        }

        private static byte[] GetHashValue(string source)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
            }
        }

        public static bool VerifySignature(string source, byte[] signedHashValue, PublicKey key)
        {
            RSA rsa = RSA.Create();
            var publicKey = new RSAParameters { Exponent = key.Exp, Modulus = key.Mod };
            rsa.ImportParameters(publicKey);
            RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm("SHA256");
            var hashValue = GetHashValue(source);
            return rsaDeformatter.VerifySignature(hashValue, signedHashValue);            
        }
       
    }
}
