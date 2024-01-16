using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Services;

namespace HardHatCore.TeamServer.Utilities
{
    public class Encryption
    {
        public static string UniversialMetadataKey = ""; // used to encrypt the metadata used in a header, which then can let you find the key for that implants metadata
        public static string UniversialMessageKey = ""; // used to encrypt / decrypt path message info for C2 tasks
        public static string UniversalTaskEncryptionKey = ""; // used to encrypt / decrypt the task encryption key for C2 tasks, only for the first task/ check-in

       public static Dictionary<string, string> UniqueTaskEncryptionKey = new Dictionary<string, string>(); // key is the implant id, value is the encrypted task encryption key

        public static List<string> FirstTimeEncryptionKeys = new List<string>(); // list of keys that have been used to encrypt the first time message

        
        //a function that takes in a string and xor's it with the UniversialMetadataKey and returns the encrypted string this string is the implant type 
        public static string EncryptImplantName(string implant_type)
        {
            //Console.WriteLine($"Encrypting implant name string with metadata key {UniversialMetadataKey}");
            string encrypted_implant_type = "";
            for (int i = 0; i < implant_type.Length; i++)
            {
                encrypted_implant_type += (char)(implant_type[i] ^ UniversialMetadataKey[i % UniversialMetadataKey.Length]);
            }

            // Encode the encrypted string using Base64
            byte[] encryptedBytes = Encoding.UTF8.GetBytes(encrypted_implant_type);
            string base64Encrypted = Convert.ToBase64String(encryptedBytes);

            return base64Encrypted;
        }
        
        //a function to get the implant type from the encrypted string
        public static string DecryptImplantName(string base64Encrypted)
        {
            try
            {
                //Console.WriteLine($"Decrypting implant name string with metadata key {UniversialMetadataKey}");
                if (UniversialMetadataKey.Length == 0)
                {
                    Console.WriteLine("Error: Metadata key is empty");
                    return "";
                }
                // Decode the Base64 encoded string
                byte[] encryptedBytes = Convert.FromBase64String(base64Encrypted);
                string encrypted_implant_type = Encoding.UTF8.GetString(encryptedBytes);

                string implant_type = "";
                for (int i = 0; i < encrypted_implant_type.Length; i++)
                {
                    implant_type += (char)(encrypted_implant_type[i] ^ UniversialMetadataKey[i % UniversialMetadataKey.Length]);
                }
                return implant_type;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        //takes in a byte array and a key to xor the data with, typically used for C2Messages but can be used for anything
        public static byte[] XorMessage(byte[] message, string key)
        {
            byte[] output = new byte[message.Length];
            for (int i = 0; i < message.Length; i++)
            {
                output[i] = (byte)(message[i] ^ key[i % key.Length]);
            }
            return output;
        }

        public static byte[] GeneratePasswordBytes(string password)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(password));
        }

        public static string GenerateRandomString(int v)
        {
            //create a random string with a character length to match the v variable 
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*-+=?";
            return new string(Enumerable.Repeat(chars, v).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void GenerateUniversialKeys()
        {
            // generate a random key for the metadata id
            UniversialMetadataKey = GenerateRandomString(32);
            // generate a random key for the path message
            UniversialMessageKey = GenerateRandomString(32);
            //task encryption key used during first check in 
            UniversalTaskEncryptionKey = GenerateRandomString(32);
            
            if(DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync(new EncryptionKeys_DAO(){ItemID = "UniversialMetadataKey", Key = UniversialMetadataKey});
            DatabaseService.AsyncConnection.InsertAsync(new EncryptionKeys_DAO(){ItemID = "UniversialMessageKey", Key = UniversialMessageKey});
            DatabaseService.AsyncConnection.InsertAsync(new EncryptionKeys_DAO(){ItemID = "UniversalTaskEncryptionKey", Key = UniversalTaskEncryptionKey});
            
        }

        //generate all Unique Keys for the Implants 
        public static void GenerateUniqueKeys(string implantId)
        {

            // generate a random key for the task encryption key
            string taskEncryptionKey = GenerateRandomString(32);
            //taskEncryptionKey = Convert.ToBase64String(GeneratePasswordBytes(taskEncryptionKey));
            if (UniqueTaskEncryptionKey.ContainsKey(implantId) == false)
            {
                UniqueTaskEncryptionKey.Add(implantId, taskEncryptionKey);
            }
            else
            {
                UniqueTaskEncryptionKey[implantId] = taskEncryptionKey;
            }
            if(DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync(new EncryptionKeys_DAO(){ItemID = implantId, Key = taskEncryptionKey});
        }
    }
}
