using UnityEngine;
using UnityEngine.UI;

public class SliderValueRecorder : MonoBehaviour
{
    [Header("Slider Settings")]
    public Slider slider; // 在Inspector中分配滑块组件
    
    [Header("Value Range")]
    public float minValue = 20.6f;
    public float maxValue = 21.8f;
    
    [Header("PlayerPrefs Key")]
    public string saveKey = "SliderZPosition"; // 存储的键名

    void Start()
    {
        // 确保滑块存在
        if (slider != null)
        {
            // 设置滑块范围
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            
            // 添加值改变监听
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            
            // 加载保存的值（如果有）
            if (PlayerPrefs.HasKey(saveKey))
            {
                float savedValue = PlayerPrefs.GetFloat(saveKey);
                slider.value = savedValue;
            }
            else
            {
                // 设置默认值
                slider.value = minValue;
            }
        }
    }

    // 当滑块值改变时调用
    private void OnSliderValueChanged(float value)
    {
        // 保存值到PlayerPrefs
        PlayerPrefs.SetFloat(saveKey, value);
        PlayerPrefs.Save();
        
        Debug.Log($"滑块值已保存: {value}");
    }

    // 可选：提供静态方法供其他脚本获取保存的值
    public static float GetSavedValue(string key = "SliderZPosition")
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetFloat(key);
        }
        return 20.6f; // 默认值
    }
}