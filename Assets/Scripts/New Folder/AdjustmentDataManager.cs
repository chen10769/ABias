using System;
using UnityEngine;

[Serializable]
public class AdjustmentData
{
    //图片位置和大小设置
    public float a1Size = 1f;
    public float a2Size = 1f;
    public float aYSpacing = 2f;
    public float aXPosition = 0f;
    
    // 敌人位置设置
    public float bXSpacing = 3f;
    public float bZPosition = 0f;
    
    // C 相关数据
    public float cSize = 1f;
    
    // P 相关数据
    public float pZPosition = 0f;
}

public class AdjustmentDataManager : MonoBehaviour
{
    public static AdjustmentDataManager Instance;
    
    private const string SAVE_KEY = "AdjustmentData";
    public AdjustmentData CurrentData { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadData()
    {
        string jsonData = PlayerPrefs.GetString(SAVE_KEY, "");
        if (!string.IsNullOrEmpty(jsonData))
        {
            CurrentData = JsonUtility.FromJson<AdjustmentData>(jsonData);
        }
        else
        {
            CurrentData = new AdjustmentData();
        }
    }
    
    public void SaveData()
    {
        string jsonData = JsonUtility.ToJson(CurrentData);
        PlayerPrefs.SetString(SAVE_KEY, jsonData);
        PlayerPrefs.Save();
    }
    
    public void UpdateDataAndSave(AdjustmentData newData)
    {
        CurrentData = newData;
        SaveData();
    }
}