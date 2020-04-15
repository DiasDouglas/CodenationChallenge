using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CodenationChallenge
{
    class Program
    {
        static string Decrypt(string message, int offset)
        {
            var validChars = Enumerable.Range('\x61', 26).Select(c => (char)c).ToArray();

            string decryptedMessage = "";
            
            foreach (var character in message)
            {
                if (validChars.Contains(character))
                {
                    var charIndex = Array.IndexOf(validChars, character);

                    var newChar = charIndex - offset >= 0 ? validChars[charIndex - offset] : validChars[validChars.Length - Math.Abs((charIndex - offset))];

                    decryptedMessage += newChar;
                }
                else
                {
                    decryptedMessage += character;
                }
            }

            return decryptedMessage;
        }

        static void WriteToFile(ResponseFormat responseFormat)
        {
            File.WriteAllText(@"C:\Users\dougl\Desktop\answer.json", JsonConvert.SerializeObject(responseFormat));
        }

        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient();
            ResponseFormat responseFormat = new ResponseFormat();

            HttpResponseMessage response = await client.GetAsync("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=" + Configuration.GetToken());
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();

                responseFormat = JsonConvert.DeserializeObject<ResponseFormat>(res);
            }
            else
            {
                throw new Exception("A resposta da requisição foi inválida.");
            }

            responseFormat.decifrado = Decrypt(responseFormat.cifrado, responseFormat.numero_casas);

            byte[] buffer = Encoding.Default.GetBytes(responseFormat.decifrado);
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            responseFormat.resumo_criptografico = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "").ToLower();

            WriteToFile(responseFormat);

            // TODO - Send the file via multi-form POST data request 
        }
    }
}
