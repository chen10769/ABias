using UnityEngine;
using System.Diagnostics;
using System.IO;

public class OpenFolderUtility : MonoBehaviour
{
    [ContextMenu("打开 TestImg 文件夹")]
    public void OpenTestImgFolder()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, "TestImg");

        if (Directory.Exists(folderPath))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true // 让系统用资源管理器打开路径
            };
            Process.Start(startInfo);
        }
        else
        {
            UnityEngine.Debug.LogWarning("文件夹不存在：" + folderPath);
        }
    }
}
