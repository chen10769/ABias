// using UnityEngine;
// using UnityEngine.UI;
// using System.IO;
// using UnityEditor; // 注意：只能在编辑器里用，打包后不可用

// public class ImageLoader : MonoBehaviour
// {
//     public Image targetImage; // 在 Inspector 拖拽 UI Image 组件

//     // 点击按钮时调用
//     public void OnClickLoadImage()
//     {
//         // 打开文件选择对话框
//         string path = EditorUtility.OpenFilePanel("选择一张图片", "", "png,jpg,jpeg");
//         if (!string.IsNullOrEmpty(path))
//         {
//             StartCoroutine(LoadImage(path));

//             // 复制文件到 StreamingAssets\TestImg
//             CopyToStreamingAssets(path);
//         }
//     }

//     private System.Collections.IEnumerator LoadImage(string path)
//     {
//         // 读取图片字节
//         byte[] fileData = File.ReadAllBytes(path);

//         // 加载成 Texture2D
//         Texture2D tex = new Texture2D(2, 2);
//         tex.LoadImage(fileData);

//         // 转换成 Sprite
//         Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
//                                          new Vector2(0.5f, 0.5f));

//         // 应用到 Image 组件
//         targetImage.sprite = newSprite;

//         yield return null;
//     }

//     private void CopyToStreamingAssets(string srcPath)
//     {
//         // 确保 StreamingAssets/TestImg 文件夹存在
//         string destFolder = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg");
//         if (!Directory.Exists(destFolder))
//         {
//             Directory.CreateDirectory(destFolder);
//         }

//         // 获取文件名并拼接目标路径
//         string fileName = Path.GetFileName(srcPath);
//         string destPath = Path.Combine(destFolder, fileName);

//         // 如果已经存在则覆盖
//         File.Copy(srcPath, destPath, true);

//         Debug.Log($"图片已复制到: {destPath}");
//     }
// }
