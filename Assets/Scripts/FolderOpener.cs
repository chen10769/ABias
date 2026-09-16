// using System.IO;
// using System.Diagnostics;
// using UnityEngine;

// public class FolderOpener : MonoBehaviour
// {
//     // 打开StreamingAssets/TestImg文件夹的方法
//     public void OpenTestImgFolder()
//     {
//         // 获取StreamingAssets路径
//         string streamingAssetsPath = Application.streamingAssetsPath;
        
//         // 拼接目标文件夹路径
//         string targetFolderPath = Path.Combine(streamingAssetsPath, "TestImg");

//         // 确保目录存在
//         if (!Directory.Exists(targetFolderPath))
//         {
//             Directory.CreateDirectory(targetFolderPath);
//             UnityEngine.Debug.Log($"创建目录: {targetFolderPath}");
//         }

//         // 检查操作系统是否为Windows
//         if (Application.platform == RuntimePlatform.WindowsEditor || 
//             Application.platform == RuntimePlatform.WindowsPlayer)
//         {
//             try
//             {
//                 // 使用资源管理器打开文件夹
//                 Process.Start("explorer.exe", $"/select,\"{targetFolderPath}\"");
//             }
//             catch (System.Exception e)
//             {
//                 UnityEngine.Debug.LogError($"打开文件夹失败: {e.Message}");
//             }
//         }
//         else
//         {
//             UnityEngine.Debug.LogWarning("此功能仅支持Windows平台");
//         }
//     }
// }