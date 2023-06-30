using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class API_KEYManager : MonoBehaviour
{
    private static API_KEYManager instance;
    public static API_KEYManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new GameObject("API_KEYManager").AddComponent<API_KEYManager>();
                DontDestroyOnLoad(instance);
                instance.LoadKeys();
            }
            return instance;
        }
    }

    private Dictionary<string, string> keyMap;

    private void LoadKeys()
    {
        keyMap = new Dictionary<string, string>();
        string filePath = Application.dataPath + "/Script/Keys.txt";

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // 检查每一行的数据，如果包含对应的键名，则提取对应的值
                    string[] key_value = line.Split(": ");
                    if(key_value.Length == 2)
                    {
                        string key = key_value[0];
                        string value = key_value[1];
                        keyMap[key] = value;
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError("An error occurred while read Keys.txt: " + e.Message);
        }
    }
    private string GetValue(string key)
    {
        if(keyMap.ContainsKey(key))
        {
            return keyMap[key];
        }
        return null;
    }

    public string GetChatGPTAPIKey()
    {
        return GetValue("ChatGPT_API_KEY");
    }

    public string GetBaiduAPIKey()
    {
        return GetValue("BaiduAI_API_KEY");
    }

    public string GetBaiduSecretKey()
    {
        return GetValue("BaiduAI_SECRET_KEY");
    }

    public string GetBaiduCUID()
    {
        return GetValue("BaiduAI_C_UID");
    }
}
