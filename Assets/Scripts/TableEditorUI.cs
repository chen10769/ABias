using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using SFB;

public class TableEditorUI : MonoBehaviour
{
    public GameObject table;
    public InputField tableNameInput;
    public Button createTableBtn;
    public Button addRowBtn;
    public Button saveBtn;
    public Button readBtn; // 新增读取按钮
    public Transform rowContainer;
    public GameObject rowPrefab;

    private string currentTableName;
    private List<TableRowUI> rows = new List<TableRowUI>();

    private void Start()
    {
        createTableBtn.onClick.AddListener(OnCreateTable);
        addRowBtn.onClick.AddListener(OnAddRow);
        saveBtn.onClick.AddListener(OnSave);
        readBtn.onClick.AddListener(OnReadCsv); // 读取按钮绑定
    }

    private void OnCreateTable()
    {
        if (SettingsPanelController.instance.isVs == true)
        {
            return;
        }
        // 先清理旧表格内容
        foreach (var row in rows)
        {
            Destroy(row.gameObject);
        }
        rows.Clear();


        currentTableName = tableNameInput.text.Trim();
        if (string.IsNullOrEmpty(currentTableName))
        {
            Debug.LogError("表格名称不能为空！");
            TipManager.instance.ToShowTip("表格名称不能为空");
            return;
        }

        string folderPath = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg", currentTableName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        Debug.Log($"已创建表格文件夹: {folderPath}");
        table.SetActive(true);
        OnAddRow();
    }

    private void OnAddRow()
    {
        if (string.IsNullOrEmpty(currentTableName))
        {
            Debug.LogError("请先创建表格！");
            return;
        }

        GameObject rowGO = Instantiate(rowPrefab, rowContainer);
        TableRowUI rowUI = rowGO.GetComponent<TableRowUI>();
        rowUI.Init(currentTableName, this);
        rows.Add(rowUI);
    }

    public void RemoveRow(TableRowUI row)
    {
        if (rows.Contains(row))
        {
            rows.Remove(row);
            Destroy(row.gameObject);
        }
    }


    private void OnSave()
    {
        if (string.IsNullOrEmpty(currentTableName))
        {
            Debug.LogError("请先创建表格！");

            return;
        }

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string inputFolder = Path.Combine(projectRoot, "input");
        if (!Directory.Exists(inputFolder))
            Directory.CreateDirectory(inputFolder);

        string csvPath = Path.Combine(inputFolder, currentTableName + ".csv");

        using (StreamWriter sw = new StreamWriter(csvPath, false, System.Text.Encoding.UTF8))
        {
            sw.WriteLine("左图,右图,探测点位置,性别,情绪类型,情绪面孔位置,图片,探测点一致性,控制条件");
            foreach (var row in rows)
            {
                string[] data = row.GetRowData();
                sw.WriteLine(string.Join(",", data));
            }
        }
        TipManager.instance.ToShowTip("保存成功");
        Debug.Log($"表格已保存到: {csvPath}");
    }

    private void OnReadCsv()
    {
        if (SettingsPanelController.instance.isVs == true)
        {
            return;
        }

        // 项目根 input 文件夹
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string inputFolder = Path.Combine(projectRoot, "input");

        // 弹出文件选择器，初始目录设置为 input 文件夹
        var paths = StandaloneFileBrowser.OpenFilePanel("选择 CSV 文件", inputFolder, "csv", false);
        if (paths.Length == 0 || string.IsNullOrEmpty(paths[0])) return;
       
        string path = paths[0];
        // 清空现有行
        foreach (var row in rows)
            Destroy(row.gameObject);
        rows.Clear();

        string[] lines;

        try
        {
            lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        }
        catch (IOException e)
        {
            int code = e.HResult & 0xFFFF;

            // 32 / 33 = 文件被占用
            if (code == 32 || code == 33)
            {
                Debug.LogError("表格被占用，请关闭 Excel / WPS 后重试！");
                TipManager.instance.ToShowTip("表格被占用，请关闭 Excel / WPS 后重试！");
            }
            else
            {
                Debug.LogError("读取 CSV 失败（非占用错误）");
                Debug.LogError(e);
                TipManager.instance.ToShowTip("读取表格失败，请检查文件是否损坏或路径是否正确");
            }

            return; // ⛔ 直接中断读取
        }
         table.SetActive(true);
        if (lines.Length < 2)
        {
            Debug.LogWarning("CSV 文件没有内容或没有表头");
            return;
        }

        currentTableName = Path.GetFileNameWithoutExtension(path);
        tableNameInput.text = currentTableName;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');

            GameObject rowGO = Instantiate(rowPrefab, rowContainer);
            TableRowUI rowUI = rowGO.GetComponent<TableRowUI>();
            rowUI.Init(currentTableName, this);
            rows.Add(rowUI);

            if (cols.Length >= 9)
            {
                // --- 左图 ---
                string leftOriginal = cols[0];
                // 先提取 Dropdown 文本
                SetDropdownFromPath(leftOriginal, rowUI.leftDropdown);
                // 提取真实路径（去掉括号部分）
                string leftPathOnly = RemoveDropdownPrefix(leftOriginal);
                rowUI.leftImagePath = leftPathOnly; // 保留真实路径
                SetImage(leftPathOnly, rowUI.leftImage);

                // --- 右图 ---
                string rightOriginal = cols[1];
                SetDropdownFromPath(rightOriginal, rowUI.rightDropdown);
                string rightPathOnly = RemoveDropdownPrefix(rightOriginal);
                rowUI.rightImagePath = rightPathOnly;
                SetImage(rightPathOnly, rowUI.rightImage);
                // 探测点 Toggle
                rowUI.probeLeftToggle.isOn = cols[2].Contains("左");
                rowUI.probeRightToggle.isOn = cols[2].Contains("右");

                rowUI.genderInput.text = cols[3];
                rowUI.emotionTypeInput.text = cols[4];
                rowUI.facePosInput.text = cols[5];
                rowUI.imageLabelInput.text = cols[6];
                rowUI.probeConsistencyInput.text = cols[7];
                rowUI.controlCondInput.text = cols[8];
            }
        }


        Debug.Log($"已读取 CSV: {path}，共 {lines.Length - 1} 行");
    }
    private string RemoveDropdownPrefix(string path)
    {
        int end = path.IndexOf(")");
        if (end >= 0)
            return path.Substring(end + 1);
        return path;
    }


    //  // 提取括号内容并设置 Dropdown
    private void SetDropdownFromPath(string path, Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options.Count == 0) return;

        // 从括号中提取文本
        string dropdownText = "";
        int start = path.IndexOf("(");
        int end = path.IndexOf(")");
        if (start >= 0 && end > start)
            dropdownText = path.Substring(start + 1, end - start - 1);

        if (!string.IsNullOrEmpty(dropdownText))
        {
            // 查找文本在现有 Dropdown 选项里的索引
            int idx = dropdown.options.FindIndex(o => o.text == dropdownText);
            if (idx >= 0)
                dropdown.value = idx;
            else
            {
                // 如果不存在，则添加为一个新选项
                dropdown.options.Add(new Dropdown.OptionData(dropdownText));
                dropdown.value = dropdown.options.Count - 1;
            }
        }
    }

    // 仅加载图片，不修改 Dropdown
    private void SetImage(string path, Image targetImage)
    {
        string fileName = path.Replace("(", "").Replace(")", "");
        string fullPath = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg", fileName);
        fullPath = fullPath.Replace("/", @"\"); // 统一分隔符
        Debug.Log(fullPath);
        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                             new Vector2(0.5f, 0.5f));
            targetImage.sprite = newSprite;
        }
    }

    // // 去掉指定字符串前面的内容
    // private string RemoveBeforeFirstOccurrence(string fullPath, string target)
    // {
    //     if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(target))
    //         return fullPath;

    //     int idx = fullPath.IndexOf(target);
    //     if (idx >= 0)
    //         return fullPath.Substring(idx);

    //     return fullPath;
    // }


    // // 加载图片并设置 Dropdown
    // private void SetImageAndDropdown(string path, Image targetImage, Dropdown dropdown)
    // {
    //     Debug.Log(path);
    //     // 从路径中提取 Dropdown 文本：去掉括号
    //     string dropdownText = "";
    //     int start = path.IndexOf("(");
    //     int end = path.IndexOf(")");
    //     if (start >= 0 && end > start)
    //         dropdownText = path.Substring(start + 1, end - start - 1);
    //     Debug.Log(path);

    //     if (dropdown != null && !string.IsNullOrEmpty(dropdownText))
    //     {
    //         // 清空原有选项
    //         dropdown.ClearOptions();
    //         dropdown.options.Add(new Dropdown.OptionData(dropdownText));
    //         dropdown.value = 0;
    //     }

    //     // 加载图片
    //     string fullPath = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg", path.Replace("(", "").Replace(")", "")); // 去掉括号用于文件名
    //     fullPath = fullPath.Replace("/", @"\"); // 将 / 替换为 \
    //     if (File.Exists(fullPath))
    //     {
    //         byte[] fileData = File.ReadAllBytes(fullPath);
    //         Texture2D tex = new Texture2D(2, 2);
    //         tex.LoadImage(fileData);
    //         Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
    //                                          new Vector2(0.5f, 0.5f));
    //         targetImage.sprite = newSprite;
    //     }
    // }

}
