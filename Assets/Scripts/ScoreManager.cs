using System;
using System.IO;
using System.Text;
using UnityEngine;


public class ScoreManager : MonoBehaviour
{
    public int score;
    public static ScoreManager instance;

    void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ExportScore()
    {
         // 1. 构建文件保存目录路径
        string folderPath = Path.Combine(Application.dataPath, "../ExperimentResults");
        
        // 2. 如果目录不存在则创建
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // 3. 获取学生相关信息（与原方法保持一致）
        string studentId = PersistentObject.instance.studentId;
        string phone = PersistentObject.instance.phoneNumber;
        string group = PersistentObject.instance.group;

        // 4. 构建完整的文件路径，按照指定格式命名
        string fileName = $"{studentId}_{phone}_{group}_{"分数"}{score}.txt";
        string filePath = Path.Combine(folderPath, fileName);

        try
        {
            // 5. 创建空白文件（WriteAllText传入空字符串即可）
            File.WriteAllText(filePath, string.Empty, Encoding.UTF8);
            Debug.Log($"空白TXT文件已创建至: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("创建空白TXT文件时出错: " + e.Message);
        }
    }
}
