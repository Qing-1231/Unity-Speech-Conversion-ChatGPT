using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ChatGPT;
using BaiduTTS;
using System;

public class ChatGPTConnector : MonoBehaviour
{
    public static string chatGPTAPIURL = "https://api.openai.com/v1/chat/completions"; // ChatGPT API��URL
    public static string apiKey;
    public InputField inputField; // �����ı���
    public Text outputText; // ����ı���
    public SpeechSynthesis speechSynthesis;
    public AudioSource audioSource;

    private void Awake()
    {
        speechSynthesis = GetComponent<SpeechSynthesis>();
        apiKey = API_KEYManager.Instance.GetChatGPTAPIKey();
    }

    public IEnumerator SendRequestAndPlayVoice(Action<AudioClip> callback)
    {
        string userInput = inputField.text;
        yield return StartCoroutine(SendRequest(userInput));

        // ����BaiduTTS���������ϳ�
        Debug.Log("����BaiduTTS���������ϳ�");
        yield return speechSynthesis.GetAudio(outputText.text, callback);
        Debug.Log("�����ϳɳɹ����ֽ���������Unity�в�����");
    }
    public IEnumerator SendRequestAndPlayVoice()
    {
        string userInput = inputField.text;
        yield return StartCoroutine(SendRequest(userInput));

        Debug.Log("����BaiduTTS���������ϳ�");
        // �ص�������lambda���ʽ��Ϊ������������ע��
        yield return speechSynthesis.GetAudio(outputText.text, (clip) => { audioSource.clip = clip; });
        Debug.Log("�����ϳɳɹ����ֽ���������Unity�в�����");
        //audioSource.clip = ResourceManager.Instance.LoadAudioClip("Audio/output");
        audioSource.Play();
    }

    private AudioClip GetAudioCallback(AudioClip audioClip)
    {
        return audioClip;
    }

    private IEnumerator SendRequest(string userInput)
    {
        var request = new UnityWebRequest(chatGPTAPIURL, "POST");
        PostData _postData = new PostData(userInput);

        string _jsonText = JsonConvert.SerializeObject(_postData);
        //Debug.Log(_jsonText);

        // Serialize _jsonText to byte[] data
        byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(data);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", string.Format("Bearer {0}", apiKey));

        //Debug.Log("Set Request complete, now SendWebRequest");

        yield return request.SendWebRequest();

        //Debug.Log("sccuess SendRequest, responseCode = " + request.responseCode);

        if (request.responseCode == 200)
        {
            string _msg = request.downloadHandler.text;
            //Debug.Log(_msg);
            ResponseObject responseObject = JsonConvert.DeserializeObject<ResponseObject>(_msg);
            if (responseObject != null && responseObject.Choices != null && responseObject.Choices.Count > 0)
            {
                string getReply = responseObject.Choices[0].Message?.Content;
                Debug.Log(getReply);

                outputText.text = getReply;
            }
        }
    }
}