using UnityEngine;

public class ApplySliderValue : MonoBehaviour
{
    [Header("PlayerPrefs Key")]
    public string loadKey = "SliderZPosition"; // 与保存时相同的键名
    
    [Header("Position Range")]
    public float minZ = 20.6f;
    public float maxZ = 21.8f;

    void Start()
    {
        ApplyZPosition();
        Debug.Log(gameObject.name);
    }

    public void ApplyZPosition()
    {
        // 从PlayerPrefs读取保存的值
        float zPosition;
        
        if (PlayerPrefs.HasKey(loadKey))
        {
            zPosition = PlayerPrefs.GetFloat(loadKey);
            
            // 确保值在有效范围内
            zPosition = Mathf.Clamp(zPosition, minZ, maxZ);
        }
        else
        {
            // 如果没有保存的值，使用最小值
            zPosition = minZ;
        }
        
        // 改变物体的z轴位置
        Vector3 newPosition = transform.position;
        newPosition.z = zPosition;
        transform.position = newPosition;
        
        Debug.Log($"应用Z轴位置: {zPosition}");
    }

    // 可选：从SliderValueRecorder直接获取值
    public void ApplyZPositionFromSlider()
    {
        float zPosition = SliderValueRecorder.GetSavedValue(loadKey);
        zPosition = Mathf.Clamp(zPosition, minZ, maxZ);
        
        Vector3 newPosition = transform.position;
        newPosition.z = zPosition;
        transform.position = newPosition;
    }
}