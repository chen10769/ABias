using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using Fungus;

public class RuntimeDialogueLoader : MonoBehaviour
{
    [Header("配置参数")]
    public Flowchart targetFlowchart;

    [Tooltip("要把对话添加到的Block的名称")]
    public string targetBlockName = "DynamicDialogueBlock";

    public static RuntimeDialogueLoader instance;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 读取外部文件，生成命令，并执行该 Block
    /// </summary>
    public void LoadAndExecuteDialogue(string fileName, string targetBlockName)
    {
        if (targetFlowchart == null)
        {
            Debug.LogError("未指定 Flowchart！");
            return;
        }

        // 1. 获取游戏根目录路径
        string filePath = Path.Combine(GetGameRootPath(), fileName + ".txt");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"找不到对话文件！请确保文件存在于: {filePath}");
            // TipManager.instance?.ToShowTip($"找不到对话文件！"); // 如果有报错可以取消注释
            return;
        }

        // 2. 读取文本内容 (默认使用UTF-8编码)
        string rawText = File.ReadAllText(filePath);

        // 3. 解析文本：忽略 /* */ 注释，忽略回车，用 | 分割
        string noCommentText = Regex.Replace(rawText, @"/\*.*?\*/", "", RegexOptions.Singleline);
        string noNewlineText = noCommentText.Replace("\r", "").Replace("\n", "");
        string[] dialogues = noNewlineText.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);

        // 4. 获取目标 Block
        Block block = targetFlowchart.FindBlock(targetBlockName);

        if (block == null)
        {
            Debug.LogError($"在 Flowchart 中找不到名为 '{targetBlockName}' 的 Block！");
            return;
        }

        // 确保只在第一次加载时插入，防止重复加载
        if (block.CommandList.Count <= 1)
        {
            int insertIndex = 0; 

            foreach (string dlg in dialogues)
            {
                string cleanDialogue = dlg.Trim();
                if (string.IsNullOrEmpty(cleanDialogue)) continue;

                // 动态挂载 Say 组件
                Say newSayCommand = targetFlowchart.gameObject.AddComponent<Say>();
                newSayCommand.SetStandardText(cleanDialogue); // 设置文本内容
                newSayCommand.ParentBlock = block;
                newSayCommand.ItemId = targetFlowchart.NextItemId(); // 分配唯一ID

                // 插入到指定位置
                block.CommandList.Insert(insertIndex, newSayCommand);
                insertIndex++;
            }

            // ==========================================
            // 【最核心的修复】：重建所有命令的内部索引！
            // 解决打包后无限循环、卡死、乱跳的终极方案
            // ==========================================
            for (int i = 0; i < block.CommandList.Count; i++)
            {
                block.CommandList[i].CommandIndex = i;
            }
        }

        Debug.Log($"成功从外部加载了 {dialogues.Length} 条对话，已插入到最前方！");

        // 6. 执行该 Block
        // 【注意】：如果你是通过 Fungus 内部的 Invoke Method 调用的这个脚本，
        // 并且调用的就是当前这个 Block，这里强行 ExecuteBlock 可能会打断底层协程！
        // 如果发现依然有问题，请参考下方【重要架构建议】。
        targetFlowchart.ExecuteBlock(block);
    }

    /// <summary>
    /// 获取游戏根目录（兼容编辑器和打包后的路径）
    /// </summary>
    private string GetGameRootPath()
    {
#if UNITY_EDITOR
        // 在 Unity 编辑器中，根目录是项目文件夹（Assets 的上一级）
        return Directory.GetParent(Application.dataPath).FullName;
#else
        // 打包后根目录
        return Directory.GetParent(Application.dataPath).FullName;
#endif
    }
}
