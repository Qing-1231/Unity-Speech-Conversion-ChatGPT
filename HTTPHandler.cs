using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChatGPT
{
    public class Message
    {
        [JsonProperty("role")]
        public string Role;
        [JsonProperty("content")]
        public string Content;
    }
    // 该版本的POSTData只支持发送单个句子
    public class PostData
    {
        public string model;
        public Message[] messages;
        public float temperature;

        //重载CreatePostData函数
        public PostData(string message) 
        {
            InitializePostData(message, 0.7f);
        }
        public PostData(string message, float temperature)
        {
            InitializePostData(message, temperature);
        }
        public void InitializePostData(string message, float temperature)
        {
            model = "gpt-3.5-turbo";
            messages = new Message[]
            {
                new Message
                {
                    Role = "system",
                    Content = "你现在是一个聊天机器人，说话尽量口语化，每句话尽量简短，以符合聊天对话的场景。"
                },
                new Message
                {
                    Role = "user",
                    Content = message
                }
            };
            this.temperature = temperature;
        }
    }
    public class Choice
    {
        public int Index { get; set; }
        public Message Message { get; set; }
        public string FinishReason { get; set; }
    }

    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class ResponseObject
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public int Created { get; set; }
        public List<Choice> Choices { get; set; }
        public Usage Usage { get; set; }
    }
}

namespace BaiduTTS
{
    public class AccessTokenResponseObject
    {
        public string Access_token { get; set; }
        public string Refresh_token { get; set; }
        public string Expires_in { get; set; }
        public string Scope { get; set; }
        public string Session_key { get; set; }
        public string Session_secret { get; set; }
    }
}

namespace BaiduASR
{
    public class BaiduAsrResponseObject
    {
        public string corpus_no { get; set; }
        public string err_msg { get; set; }
        public int err_no { get; set; }
        // result是存储识别文本结果的字段。
        public string[] result { get; set; }
        public string sn { get; set; }
    }

    public class BaiduAsrPostData
    {
        public string format { get; set; }
        public int rate { get; set; }
        public int channel { get; set; }
        public string cuid { get; set; }
        public string token { get; set; }
        public string speech { get; set; }
        public int len { get; set; }
    }
}