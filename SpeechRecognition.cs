using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using System;
using BaiduTTS;
using System.IO;

namespace BaiduASR
{
    public class SpeechRecognition : MonoBehaviour
    {
        string API_KEY;
        string SECRET_KEY;
        string C_UID;
        private AudioClip recording;
        private bool isRecording = false;
        private bool recongnizedCompleted = false;
        public string RecongnizedText { get; set; }

        // 初始化 Keys
        private void Awake()
        {
            API_KEY = API_KEYManager.Instance.GetBaiduAPIKey();
            SECRET_KEY = API_KEYManager.Instance.GetBaiduSecretKey();
            C_UID = API_KEYManager.Instance.GetBaiduCUID();
        }

        public IEnumerator GetTextFromAudio(byte[] audioData, Action<string> callback)
        {
            string accessToken = GetAccessToken();
            string url = "http://vop.baidu.com/server_api";

            string base64Audio = Convert.ToBase64String(audioData);

            BaiduAsrPostData postData = new BaiduAsrPostData
            {
                format = "pcm",
                rate = 16000,
                channel = 1,
                cuid = C_UID,
                token = accessToken,
                speech = base64Audio,
                len = audioData.Length
            };

            string _jsonText = JsonConvert.SerializeObject(postData);

            byte[] postDataBytes = System.Text.Encoding.UTF8.GetBytes(_jsonText);

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(postDataBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                BaiduAsrResponseObject result = JsonConvert.DeserializeObject<BaiduAsrResponseObject>(responseText);

                if (result.err_no == 0)
                {
                    string recognizedText = result.result[0];
                    Debug.Log("Recognized Text: " + recognizedText);
                    callback?.Invoke(recognizedText);
                }
                else
                {
                    Debug.LogError("Error in speech recognition: " + result.err_msg + "\nerr_no =  " + result.err_no);
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError("SpeechRecognition Error: " + request.error);
                callback?.Invoke(null);
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
                // Wait for the request to complete
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                AccessTokenResponseObject result = JsonConvert.DeserializeObject<AccessTokenResponseObject>(responseText);
                return result.Access_token.ToString();
            }
            else
            {
                Debug.LogError("GetAccessToken Error: " + request.error);
                return null;
            }
        }

        public byte[] ConvertAudioClipDataToPCM16(AudioClip recording)
        {
            float[] audioData = new float[recording.samples * recording.channels];
            recording.GetData(audioData, 0);

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(memoryStream);

            for (int i = 0; i < audioData.Length; i++)
            {
                short s = (short)(audioData[i] * short.MaxValue);
                bw.Write(s);
            }
            bw.Close();

            return memoryStream.ToArray();
        }

        public bool Recording()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                // start recording
                recording = Microphone.Start(null, true, 10, 16000);
                isRecording = true;
                Debug.Log("录音中");
                //return false;
            }
            if (Input.GetKeyUp(KeyCode.V) && isRecording == true)
            {
                // Stop recording
                Microphone.End(null);
                isRecording = false;
                Debug.Log("结束录音");
                // Get audio data
                //float[] audioData = new float[recording.samples * recording.channels];
                //recording.GetData(audioData, 0);

                // Convert to PCM bytes
                byte[] bytes = ConvertAudioClipDataToPCM16(recording);

                // Send audio data to BaiduASR API
                StartCoroutine(GetTextFromAudio(bytes, CallBack));
                // 这里不会等待协程完毕就直接返回true
                //return true;
            }
            //return false;
            // 在CallBack函数调用之后才会返回true
            if(recongnizedCompleted == true)
            {
                recongnizedCompleted = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CallBack(string text)
        {
            // 在CallBack函数中将recongnizedCompleted改为true
            if (text != null) 
            {
                Debug.Log("Recognized Text: " + text);
                RecongnizedText = text;
                recongnizedCompleted = true;
            }
            else
            {
                Debug.Log("Recognized Text is NULL");
                recongnizedCompleted = false;
            }
        }
    }
    
}
