//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Baidu;
//using Baidu.Aip.Speech;

//public class SpeechSynthesis : MonoBehaviour
//{
//    // 设置APPID/AK/SK
//    private string APP_ID = "34757302";
//    private string API_KEY = "PWDT42UR106XBYULXH7PRfiq";
//    private string SECRET_KEY = "BSwT72xSlE61cEcbIBPo36IzSkZ0f8tO";

//    //Baidu.Aip.Speech.Tts client = new Baidu.Aip.Speech.Tts(API_KEY, SECRET_KEY);
//    //client.Timeout = 60000;  // 修改超时时间
//    // 合成
//    public void Tts()
//    {
//        Baidu.Aip.Speech.Tts client = new Baidu.Aip.Speech.Tts(API_KEY, SECRET_KEY);
//        client.Timeout = 60000;
//        // 可选参数
//        var option = new Dictionary<string, object>()
//        {
//            {"spd", 5}, // 语速
//            {"vol", 7}, // 音量
//            {"per", 4}  // 发音人，4：情感度丫丫童声
//        };
//        var result = client.Synthesis("众里寻他千百度", option);

//        if (result.ErrorCode == 0)  // 或 result.Success
//        {
//            File.WriteAllBytes("合成的语音文件本地存储地址.mp3", result.Data);
//        }
//    }

//    private void Start()
//    {
//        APP_ID = "34757302";
//        API_KEY = "PWDT42UR106XBYULXH7PRfiq";
//        SECRET_KEY = "BSwT72xSlE61cEcbIBPo36IzSkZ0f8tO";
//        Tts();
//    }
//}


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;
//using System.Net.Http;
//using System.Threading.Tasks;

//public class SpeechSynthesis : MonoBehaviour
//{
//    private static readonly HttpClient client = new HttpClient();

//    public string apiKey = "PWDT42UR106XBYULXH7PRfiq";
//    public string secretKey = "BSwT72xSlE61cEcbIBPo36IzSkZ0f8tO";

//    public async Task SynthesizeSpeech(string text, string outputPath)
//    {
//        // 设置请求的URL
//        string url = "http://tsn.baidu.com/text2audio";

//        // 设置请求参数
//        var content = new FormUrlEncodedContent(new[]
//        {
//            new KeyValuePair<string, string>("tex", text),
//            new KeyValuePair<string, string>("lan", "zh"),
//            new KeyValuePair<string, string>("cuid", "Your CUID"),
//            new KeyValuePair<string, string>("ctp", "1"),
//            new KeyValuePair<string, string>("tok", await GetAccessToken())
//        });

//        // 发送POST请求
//        var response = await client.PostAsync(url, content);

//        // 检查请求是否成功
//        if (response.IsSuccessStatusCode)
//        {
//            // 将返回的语音保存到文件
//            using (var fileStream = File.Create(outputPath))
//            {
//                await response.Content.CopyToAsync(fileStream);
//            }
//        }
//        else
//        {
//            Debug.Log("Failed to synthesize speech. Error: " + response.StatusCode);
//        }
//    }

//    private async Task<string> GetAccessToken()
//    {
//        // 设置获取AccessToken的URL
//        string url = "https://openapi.baidu.com/oauth/2.0/token";

//        // 设置请求参数
//        var content = new FormUrlEncodedContent(new[]
//        {
//            new KeyValuePair<string, string>("grant_type", "client_credentials"),
//            new KeyValuePair<string, string>("client_id", apiKey),
//            new KeyValuePair<string, string>("client_secret", secretKey)
//        });

//        // 发送POST请求
//        var response = await client.PostAsync(url, content);

//        // 检查请求是否成功
//        if (response.IsSuccessStatusCode)
//        {
//            // 解析返回的JSON数据并获取AccessToken
//            var json = await response.Content.ReadAsStringAsync();
//            var token = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenResponse>(json);
//            return token.access_token;
//        }
//        else
//        {
//            Debug.Log("Failed to get access token. Error: " + response.StatusCode);
//            return null;
//        }
//    }

//    // 定义用于解析获取AccessToken返回的JSON数据的类
//    public class AccessTokenResponse
//    {
//        public string access_token { get; set; }
//        public int expires_in { get; set; }
//        public string scope { get; set; }
//    }

//    private async Task Start()
//    {
//        await SynthesizeSpeech("这个是", "output.mp3");
//    }
//}
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using System;


namespace BaiduTTS
{
    public class SpeechSynthesis : MonoBehaviour
    {

        string API_KEY;
        string SECRET_KEY;
        string C_UID;
        public AudioClip audioClip {  get; set; }

        // 初始化 Keys
        private void Awake()
        {
            API_KEY = API_KEYManager.Instance.GetBaiduAPIKey();
            SECRET_KEY = API_KEYManager.Instance.GetBaiduSecretKey();
            C_UID = API_KEYManager.Instance.GetBaiduCUID();
        }

        public IEnumerator GetAudio(string tex, Action<AudioClip> callback)
        {
            string accessToken = GetAccessToken();
            string url = "https://tsn.baidu.com/text2audio";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            request.SetRequestHeader("Accept", "*/*");

            // 两次URL编码
            Debug.Log("BaiduTTS output: " + tex);
            string firstEncodedText = Uri.EscapeDataString(tex);
            string secondEncodedText = Uri.EscapeDataString(firstEncodedText);
            string postData = $"tex={secondEncodedText}&lan=zh&cuid={C_UID}&ctp=1&per=1&aue=3&tok={accessToken}";
            //Debug.Log(postData);

            byte[] postDataBytes = System.Text.Encoding.UTF8.GetBytes(postData);
            request.uploadHandler = new UploadHandlerRaw(postDataBytes);
            //request.downloadHandler = new DownloadHandlerBuffer();
            request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                audioClip = DownloadHandlerAudioClip.GetContent(request);
                Debug.Log("DownloadHandlerAudioClip Success");
                callback?.Invoke(audioClip);
            }
            else
            {
                Debug.Log("GetAudio Error: " + request.error);
            }
        }

        private string GetAccessToken()
        {
            UnityWebRequest request = new UnityWebRequest("https://aip.baidubce.com/oauth/2.0/token", "POST");
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            request.downloadHandler = new DownloadHandlerBuffer();

            string postData = $"grant_type=client_credentials&client_id={API_KEY}&client_secret={SECRET_KEY}";
            byte[] postDataBytes = System.Text.Encoding.UTF8.GetBytes(postData);
            request.uploadHandler = new UploadHandlerRaw(postDataBytes);

            request.SendWebRequest();

            while (!request.isDone)
            {
                // 等待请求完成
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                AccessTokenResponseObject result = JsonConvert.DeserializeObject<AccessTokenResponseObject>(responseText);
                Debug.Log("GetAccessToken Success: " + result.Access_token.ToString());
                return result.Access_token.ToString();
            }
            else
            {
                Debug.Log("GetAccessToken Error: " + request.error);
                return null;
            }
        }
    }
}