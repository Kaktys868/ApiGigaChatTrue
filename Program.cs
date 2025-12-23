using APIGigaChat_True.Models.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APIGigaChat_True
{
    public class Program
    {
        static string ClientId = "019b493f-6c86-7084-8ea6-3ca097ab1023";
        static string AuthorizationKey = "MDE5YjQ5M2YtNmM4Ni03MDg0LThlYTYtM2NhMDk3YWIxMDIzOjA4NTYyZDJiLTI5NjYtNDk4Mi1iYWNmLWY4N2I1NWYxMTlkOQ==";
        static async Task Main(string[] args)
        {
            string Token = await GetToken(ClientId, AuthorizationKey);

            if(Token == null)
            {
                Console.WriteLine("Не удалось получить токен");
                return;
            }
            while (true)
            {
                Console.Write("Сообщение: ");

                string Message = Console.ReadLine();

                ResponseMessage Answer = await GetAnswer(Token, Message);
                Console.WriteLine("Ответ: " + Answer.choices[0].message.content);
            }
        }
        public static async Task<string> GetToken(string rqUID, string bearer)
        {
            string ReturnToken = null;
            string Url = "https://ngw.devices.sberbank.ru:9WU3/api/v2/oauth";

            using (HttpClientHandler Handler = new HttpClientHandler())
            {
                Handler.ServerCertificateCustomValidationCallback = (message, cert, chain, ss1PolicyErrors) => true;

                using (HttpClient Clien = new HttpClient(Handler))
                {
                    HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, Url);

                    Request.Headers.Add("Accept", "application/json");
                    Request.Headers.Add("RqUID", rqUID);
                    Request.Headers.Add("Authorization", $"Bearer {bearer}");

                    var Data = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
                    };

                    Request.Content = new FormUrlEncodedContent(Data);

                    HttpResponseMessage Response = await Clien.SendAsync(Request);

                    if (Response.IsSuccessStatusCode)
                    {
                        string ResponseContent = await Response.Content.ReadAsStringAsync();
                        ResponseToken Token = JsonConvert.DeserializeObject<ResponseToken>(ResponseContent);
                        ReturnToken = Token.access_token;
                    }
                }
            }
            return ReturnToken;
        }
        public static async Task<ResponseMessage> GetAnswer(string token, string message)
        {
            ResponseMessage responseMessage = null;
            string Url = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

            using (HttpClientHandler Handler = new HttpClientHandler())
            {
                Handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

                using (HttpClient Client = new HttpClient(Handler))
                {
                    HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, Url);

                    Request.Headers.Add("Accept", "application/json");
                    Request.Headers.Add("Authorization", $"Bearer {token}");

                    Models.Request DataRequest = new Models.Request()
                    {
                        model = "GigaChat",
                        stream = false,
                        repetition_penalty = 1,
                        messages = new List<Models.Request.Message>
                {
                    new Models.Request.Message()
                    {
                        role = "user",
                        content = message
                    }
                }
                    };

                    string JsonContent = JsonConvert.SerializeObject(DataRequest);
                    Request.Content = new StringContent(JsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage Response = await Client.SendAsync(Request);

                    if (Response.IsSuccessStatusCode)
                    {
                        string ResponseContent = await Response.Content.ReadAsStringAsync();
                        responseMessage = JsonConvert.DeserializeObject<ResponseMessage>(ResponseContent);
                    }
                }
            }

            return responseMessage;
        }
    }
}
