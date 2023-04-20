using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoogleBardCSharp
{
    public class Chatbot
    {
        private readonly HttpClient _httpClient;
        private int _reqid;
        public string SNlM0e { get; }
        public string ConversationId { get; set; }
        public string ResponseId { get; set; }
        public string ChoiceId { get; set; }

        public Chatbot(string sessionId)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://bard.google.com")
            };
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://bard.google.com");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://bard.google.com/");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Same-Domain", "1");

            _httpClient.DefaultRequestHeaders.Add("Cookie", $"__Secure-1PSID={sessionId}");

            _reqid = new Random().Next(1000, 9999);
            ConversationId = "";
            ResponseId = "";
            ChoiceId = "";
            SNlM0e = GetSNlM0eAsync().GetAwaiter().GetResult();
        }

        private async Task<string> GetSNlM0eAsync()
        {
            var response = await _httpClient.GetAsync("/");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Could not get Google Bard");
            }

            var content = await response.Content.ReadAsStringAsync();
            var snlM0eMatch = Regex.Match(content, "SNlM0e\":\"(.*?)\"");
            return snlM0eMatch.Groups[1].Value;
        }

        public async Task<Dictionary<string, object>> AskAsync(string message)
        {
            var parameters = new Dictionary<string, string>
            {
                {"bl", "boq_assistant-bard-web-server_20230326.21_p0"},
                {"_reqid", _reqid.ToString()},
                {"rt", "c"}
            };

            var messageStruct = new object[]
            {
                new[] {message},
                null,
                new[] {ConversationId, ResponseId, ChoiceId}
            };

            var data = new Dictionary<string, string>
            {
                {"f.req", JsonConvert.SerializeObject(new[] {null, JsonConvert.SerializeObject(messageStruct)})},
                {"at", SNlM0e}
            };

            var response = await _httpClient.PostAsync("_/BardChatUi/data/assistant.lamda.BardFrontendService/StreamGenerate", new FormUrlEncodedContent(data.Concat(parameters)));

            var chatData = JsonConvert.DeserializeObject<List<object>>(await response.Content.ReadAsStringAsync().Split('\n')[3])[0][2];
            if (chatData == null)
            {
                return new Dictionary<string, object>
                {
                    {"content", $"Google Bard encountered an error: {response.Content}."}
                };
            }

            var jsonChatData = JsonConvert.DeserializeObject<List<object>>(chatData.ToString());
            var results = new Dictionary<string, object>
            {
                {"content", json                ChatData[0][0]},
                {"conversation_id", jsonChatData[1][0]},
                {"response_id", jsonChatData[1][1]},
                {"factualityQueries", jsonChatData[3]},
                {"textQuery", jsonChatData[2]?.Count > 0 ? jsonChatData[2][0] : ""},
                {"choices", jsonChatData[4].Select(i => new {id = ((List<object>)i)[0], content = ((List<object>)i)[1]}).ToList()}
            };
            ConversationId = results["conversation_id"].ToString();
            ResponseId = results["response_id"].ToString();
            ChoiceId = ((List<object>)results["choices"])[0].ToString();
            _reqid += 100000;
            return results;
        }
    }
}
