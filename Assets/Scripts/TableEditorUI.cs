using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using SFB;

public abstract class TableEditorUI<TRow> : MonoBehaviour where TRow : MonoBehaviour
{
    [Header("Common UI")]
    public GameObject table;
    public InputField tableNameInput;
    public Button addRowBtn;
    public Button saveBtn;
    public Button readBtn;
    public Button createTableBtn;

    public Transform rowContainer;
    public GameObject rowPrefab;

    protected string currentTableName;
    protected readonly List<TRow> rows = new();

    protected virtual void Start()
    {
        addRowBtn.onClick.AddListener(OnAddRow);
        saveBtn.onClick.AddListener(OnSave);
        readBtn.onClick.AddListener(OnReadCsv);
        createTableBtn.onClick.AddListener(OnCreateTable);
    }
    protected virtual void OnCreateTable()
    {


        ClearRows();

        currentTableName = tableNameInput.text.Trim();
        if (string.IsNullOrEmpty(currentTableName))
        {
            TipManager.instance.ToShowTip("表格名称不能为空");
            return;
        }

        table.SetActive(true);
        OnAddRow();
    }

    protected void ClearRows()
    {
        foreach (var r in rows)
            Destroy(r.gameObject);
        rows.Clear();
    }
    public void RemoveRow(TRow row)
    {
        if (rows.Contains(row))
        {
            rows.Remove(row);
            Destroy(row.gameObject);
        }
    }


    protected void OnAddRow()
    {
        if (string.IsNullOrEmpty(currentTableName))
        {
            TipManager.instance.ToShowTip("请先创建表格");
            return;
        }

        var go = Instantiate(rowPrefab, rowContainer);
        var row = go.GetComponent<TRow>();
        InitRow(row);
        rows.Add(row);
    }

    protected string GetInputFolder()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string input = Path.Combine(root, "input");
        if (!Directory.Exists(input)) Directory.CreateDirectory(input);
        return input;
    }

    protected abstract void InitRow(TRow row);
    protected abstract string GetCsvHeader();
    protected abstract string[] GetRowData(TRow row);
    protected abstract void RestoreRow(string[] cols, TRow row);

    protected virtual void OnSave()
    {
        if (string.IsNullOrEmpty(currentTableName)) return;

        string path = Path.Combine(GetInputFolder(), currentTableName + ".csv");

        using StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.UTF8);
        sw.WriteLine(GetCsvHeader());

        foreach (var r in rows)
            sw.WriteLine(string.Join(",", GetRowData(r)));

        TipManager.instance.ToShowTip("保存成功");
    }

    protected virtual void OnReadCsv()
    {
        string input = GetInputFolder();
        var paths = StandaloneFileBrowser.OpenFilePanel("选择 CSV", input, "csv", false);
        if (paths.Length == 0) return;

        string[] lines;
        try
        {
            lines = File.ReadAllLines(paths[0], System.Text.Encoding.UTF8);
        }
        catch (IOException e)
        {
            int code = e.HResult & 0xFFFF;
            TipManager.instance.ToShowTip(
                (code == 32 || code == 33)
                ? "表格被占用，请关闭 Excel / WPS"
                : "读取表格失败"
            );
            return;
        }
        // ===== 表头校验 =====
        if (lines.Length == 0 || !CheckCsvHeader(lines[0]))
        {
            TipManager.instance.ToShowTip(
                "表头不匹配，请使用正确的实验表格模板"
            );
            return;
        }

        ClearRows();

        currentTableName = Path.GetFileNameWithoutExtension(paths[0]);
        tableNameInput.text = currentTableName;
        table.SetActive(true);

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            var go = Instantiate(rowPrefab, rowContainer);
            var row = go.GetComponent<TRow>();
            InitRow(row);
            RestoreRow(cols, row);
            rows.Add(row);
        }
    }

    // ===== 通用工具 =====

    protected string RemoveDropdownPrefix(string path)
    {
        int end = path.IndexOf(")");
        return end >= 0 ? path.Substring(end + 1) : path;
    }

    protected void SetDropdownFromPath(string path, Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options.Count == 0) return;

        int s = path.IndexOf("(");
        int e = path.IndexOf(")");
        if (s < 0 || e <= s) return;

        string tag = path.Substring(s + 1, e - s - 1);
        int idx = dropdown.options.FindIndex(o => o.text == tag);
        if (idx >= 0)
            dropdown.value = idx;
        else
        {
            dropdown.options.Add(new Dropdown.OptionData(tag));
            dropdown.value = dropdown.options.Count - 1;
        }
    }

    protected void LoadImage(string relativePath, Image img)
    {
        string full = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg", relativePath);
        if (!File.Exists(full)) return;

        byte[] data = File.ReadAllBytes(full);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
    }
    protected bool CheckCsvHeader(string headerLine)
    {
        // 去掉 BOM、首尾空格
        headerLine = headerLine.Trim('\uFEFF').Trim();

        string expected = GetCsvHeader().Trim();
        return headerLine == expected;
    }


}
