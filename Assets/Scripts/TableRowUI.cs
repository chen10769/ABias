using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;
using System.Collections;

public abstract class TableRowUI : MonoBehaviour
{
    [Header("通用删除按钮")]
    public Button deleteBtn;

    protected string tableName;

    // 初始化方法，由子类调用
    public virtual void Init(string tableName)
    {
        this.tableName = tableName;
        if (deleteBtn != null)
            deleteBtn.onClick.AddListener(OnDelete);
    }

    // 打开文件选择并复制到表格目录
    protected string SelectAndCopyImage(ref string path, Image target)
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "选择图片",
            "",
            new ExtensionFilter[] { new ExtensionFilter("图片文件", "png", "jpg", "jpeg") },
            false
        );

        if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
            return null;

        string srcPath = paths[0];
        string destPath = CopyToTableFolder(srcPath);
        path = destPath;

        StartCoroutine(LoadImage(srcPath, target));

        return destPath;
    }

    // 加载图片
    protected IEnumerator LoadImage(string path, Image target)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        target.sprite = newSprite;
        yield return null;
    }

    // 复制文件到表格目录
    protected string CopyToTableFolder(string srcPath)
    {
        string folder = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg", tableName);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string fileName = Path.GetFileName(srcPath);
        string destPath = Path.Combine(folder, fileName);
        File.Copy(srcPath, destPath, true);

        return $"{tableName}/{fileName}";
    }

    // 为路径加上 Dropdown 标记
    protected string PackPath(string path, Dropdown dd)
    {
        if (string.IsNullOrEmpty(path) || dd == null || dd.options.Count == 0) return path;
        return $"({dd.options[dd.value].text}){path}";
    }

    // 获取当前行数据，由子类实现
    public abstract string[] GetRowData();

    // 删除行，由子类实现
    protected abstract void OnDelete();
}
