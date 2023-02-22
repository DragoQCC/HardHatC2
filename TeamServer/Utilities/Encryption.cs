  using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
  using TeamServer.Models.Dbstorage;
  using TeamServer.Services;

  namespace TeamServer.Utilities
{
    public class Encryption
    {
        public static string UniversialMetadataKey = ""; // used to encrypt the metadata used in a header, which then can let you find the key for that implants metadata
        public static string UniversialMessagePathKey = ""; // used to encrypt / decrypt path message info for C2 tasks
        public static string UniversalTaskEncryptionKey = ""; // used to encrypt / decrypt the task encryption key for C2 tasks, only for the first task/ check-in
        //public static string UniqueTeamServerVerificationKey = ""; // value is the key used on the verification message for the teamserver
        //public static string UniqueTeamServerVerificationMessage = ""; // value is the decrypted verfication message for the teamserver


       // public static Dictionary<string, string> UniqueMetadataKey = new Dictionary<string, string>(); // key is implant id, value is the key used to read the metadata
       // public static Dictionary<string, string> UniqueImplantVerificationKeys = new Dictionary<string, string>(); // key is the implant id, value is the key used on the verification message
        //public static Dictionary<string, string> UniqueImplantVerificationMessage = new Dictionary<string, string>(); // key is the implant id, value is the decrypted verfication message
        public static Dictionary<string, string> UniqueTaskEncryptionKey = new Dictionary<string, string>(); // key is the implant id, value is the encrypted task encryption key

        public static List<string> FirstTimeEncryptionKeys = new List<string>(); // list of keys that have been used to encrypt the first time message


        // Aes encryption is used to encrypt the data before sending it to the implant
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, string EncodedPassword)
        {
            try
            {
                //Console.WriteLine($"encrypting {bytesToBeEncrypted.Length} bytes");
                // make passwordBytes array out of string H@rdH@tC2P@$$w0rd!
                //byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(EncodedPassword));
                byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("H@rdH@tC2P@$$w0rd!"));

                byte[] encryptedBytes = null;
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using (MemoryStream ms = new MemoryStream())
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.ANSIX923;
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }
                        aes.Clear();
                    }
                    encryptedBytes = ms.ToArray();
                }
                return encryptedBytes;

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
           
        }
        // Aes decryption is used to decrypt the data after it has been received from the implant
        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, string EncodedPassword)
        {
            try
            {
                // make passwordBytes array out of string H@rdH@tC2P@$$w0rd!
                byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("H@rdH@tC2P@$$w0rd!"));
                //byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(EncodedPassword));

                byte[] decryptedBytes = null;
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using (MemoryStream ms = new MemoryStream())
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.ANSIX923;
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        aes.Clear();
                    }
                    decryptedBytes = ms.ToArray();
                }
                return decryptedBytes;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static byte[] GeneratePasswordBytes(string password)
        {
            return SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static string GenerateRandomString(int v)
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
            UniversialMessagePathKey = GenerateRandomString(32);
            //task encryption key used during first check in 
            UniversalTaskEncryptionKey = GenerateRandomString(32);
            
            if(DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.Connection.Insert(new EncryptionKeys_DAO(){ItemID = "UniversialMetadataKey", Key = UniversialMetadataKey});
            DatabaseService.Connection.Insert(new EncryptionKeys_DAO(){ItemID = "UniversialMessagePathKey", Key = UniversialMessagePathKey});
            DatabaseService.Connection.Insert(new EncryptionKeys_DAO(){ItemID = "UniversalTaskEncryptionKey", Key = UniversalTaskEncryptionKey});
            
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
            if(DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.Connection.Insert(new EncryptionKeys_DAO(){ItemID = implantId, Key = taskEncryptionKey});
            
        }
    }
}
