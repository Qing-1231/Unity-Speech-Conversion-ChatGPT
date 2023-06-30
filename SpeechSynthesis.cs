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