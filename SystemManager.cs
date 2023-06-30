using UnityEngine;
using BaiduTTS;
using BaiduASR;
using CrazyMinnow.SALSA;

public class SystemManager : MonoBehaviour
{
    public SpeechSynthesis speechSynthesis;
    public SpeechRecognition speechRecognition;
    public ChatGPTConnector chatgptConnector;
    public Salsa3D salsa3D;
    private bool salsa3DCanPlay;

    private void Start()
    {
        speechSynthesis = GetComponent<SpeechSynthesis>();
        speechRecognition = GetComponent<SpeechRecognition>();
        chatgptConnector = GetComponent<ChatGPTConnector>();
        salsa3DCanPlay = false;
    }

    private void Update()
    {
        if(speechRecognition.Recording())
        {
            chatgptConnector.inputField.text = speechRecognition.RecongnizedText;
            if (chatgptConnector.inputField.text != "")
            {
                StartCoroutine(chatgptConnector.SendRequestAndPlayVoice(
                    (clip) => { salsa3D.audioSrc.clip = clip; salsa3DCanPlay = true; }
                    ));
                
            }
        }

        //StartCoroutine执行完毕之后才能播放语音
        if (salsa3DCanPlay)
        {
            salsa3D.Play();
            salsa3DCanPlay = false;
        }
    }
    public void OnButtonPress()
    {
        StartCoroutine(chatgptConnector.SendRequestAndPlayVoice(
            (clip) => { salsa3D.audioSrc.clip = clip; salsa3DCanPlay = true; }
            ));
    }
}
