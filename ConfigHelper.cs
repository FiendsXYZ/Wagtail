using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace wagtail
{
    public class ConfigData
    {
        public string api_key { get; set; }
        public string voice_id { get; set; }
    }

    public static class ConfigHelper
    {
        const string configFilePath = "config.json";
        static readonly string encryptKey = "3kpPPnBL/mtlarpXkhrHFSl13SwyYeJMuXAcvgD7f2A="; // This is a Base64 encoded 32-byte key
        static readonly byte[] keyBytes = Convert.FromBase64String(encryptKey); // Convert to byte array

        public static void SaveConfig(string apiKey, string voiceId)
        {
            Console.WriteLine("SaveConfig method called.");
            Log("SaveConfig method called.");

            // Encrypt the individual values
            string encryptedApiKey = Encrypt(apiKey, keyBytes);
            string encryptedVoiceId = Encrypt(voiceId, keyBytes);

            // Create JSON object with encrypted values
            var config = new { api_key = encryptedApiKey, voice_id = encryptedVoiceId };
            string json = JsonSerializer.Serialize(config);

            // Save JSON to file
            File.WriteAllText(configFilePath, json);
            Log("SaveConfig method succeeded.");
        }

        public static (string apiKey, string voiceId) LoadConfig()
        {
            try
            {
                Console.WriteLine("LoadConfig method called.");
                Log("LoadConfig method called.");

                if (!File.Exists(configFilePath))
                {
                    Console.WriteLine("Config file does not exist.");
                    Log("Config file does not exist.");
                    return (null, null);
                }

                if (new FileInfo(configFilePath).Length == 0)
                {
                    const string message =
                        "Config file is empty. Please run the app with --config option to create a new config file.";
                    Console.WriteLine(message);
                    Log(message);
                    return (null, null);
                }

                // Read JSON from file
                string json = File.ReadAllText(configFilePath);
                Log($"Read JSON from file: {json}");

                // Deserialize JSON into the ConfigData class
                ConfigData config = JsonSerializer.Deserialize<ConfigData>(json);

                // Use the deserialized values
                string? decryptedApiKey = Decrypt(config.api_key, keyBytes);
                string? decryptedVoiceId = Decrypt(config.voice_id, keyBytes);

                Log($"Loaded API Key: {decryptedApiKey}");
                Log($"Loaded Voice ID: {decryptedVoiceId}");

                return (decryptedApiKey, decryptedVoiceId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e}");
                Log($"An error occurred: {e}");
                return (null, null);
            }
        }

        private static string Encrypt(string text, byte[] key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key; // Use byte array
                aesAlg.IV = new byte[16];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (
                        CryptoStream csEncrypt = new CryptoStream(
                            msEncrypt,
                            encryptor,
                            CryptoStreamMode.Write
                        )
                    )
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private static string Decrypt(string cipherText, byte[] key)
        {
            Log($"Decrypting: {cipherText}");
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key; // Use byte array
                    aesAlg.IV = new byte[16];

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (
                        MemoryStream msDecrypt = new MemoryStream(
                            Convert.FromBase64String(cipherText)
                        )
                    )
                    {
                        using (
                            CryptoStream csDecrypt = new CryptoStream(
                                msDecrypt,
                                decryptor,
                                CryptoStreamMode.Read
                            )
                        )
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                string decryptedText = srDecrypt.ReadToEnd();
                                // Log the decrypted text
                                Log($"Decrypted Text: {decryptedText}");
                                return decryptedText;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log decryption errors
                Log($"Decryption Error: {ex.Message}");
                return string.Empty;
            }
        }

        private const string LogFilePath = "log.txt";

        private static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"{DateTime.Now}: {message}\n");
            }
            catch (Exception ex)
            {
                // Handle any errors that occur while logging (e.g., disk full, permissions issues)
                Console.WriteLine($"An error occurred while logging: {ex.Message}");
            }
        }
    }
}
