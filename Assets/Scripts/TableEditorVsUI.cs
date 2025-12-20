using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using SFB;

public class TableEditorVisualSearchUI : MonoBehaviour
{
    public GameObject table;
    public InputField tableNameInput;
    public Button createBtn;
    public Button addRowBtn;
    public Button saveBtn;
    public Button readBtn;

    public Transform rowRoot;
    public GameObject rowPrefab;

    private string currentTable;
    private readonly List<TableRowVisualSearchUI> rows = new();

    private void Start()
    {
        createBtn.onClick.AddListener(OnCreate);
        addRowBtn.onClick.AddListener(AddRow);
        saveBtn.onClick.AddListener(OnSave);
        readBtn.onClick.AddListener(OnRead);
    }

    private void OnCreate()
    {
        if (SettingsPanelController.instance.isVs == false)
        {
            return;
        }
        ClearRows();

        currentTable = tableNameInput.text.Trim();
        if (string.IsNullOrEmpty(currentTable))
        {
            TipManager.instance.ToShowTip("表名不能为空");
            return;
        }

        table.SetActive(true);
        AddRow();
    }

    private void AddRow()
    {
        var go = Instantiate(rowPrefab, rowRoot);
        var row = go.GetComponent<TableRowVisualSearchUI>();
        row.Init(currentTable, this);
        rows.Add(row);
    }

    public void RemoveRow(TableRowVisualSearchUI row)
    {
        rows.Remove(row);
        Destroy(row.gameObject);
    }

    private void ClearRows()
    {
        foreach (var r in rows) Destroy(r.gameObject);
        rows.Clear();
    }

    private void OnSave()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string input = Path.Combine(root, "input");
        Directory.CreateDirectory(input);

        string path = Path.Combine(input, currentTable + ".csv");

        using StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.UTF8);
        sw.WriteLine("左上图,右上图,左下图,右下图,性别,图片,情绪面孔位置,正确面孔位置");

        foreach (var r in rows)
            sw.WriteLine(string.Join(",", r.GetRowData()));

        TipManager.instance.ToShowTip("保存成功");
    }

    private void OnRead()
    {
        if (SettingsPanelController.instance.isVs == false)
        {
            return;
        }
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string input = Path.Combine(root, "input");

        var paths = StandaloneFileBrowser.OpenFilePanel("选择 CSV", input, "csv", false);
        if (paths.Length == 0) return;

        ClearRows();

        string[] lines;

        try
        {
            lines = File.ReadAllLines(paths[0], System.Text.Encoding.UTF8);
        }
        catch (IOException e)
        {
            int code = e.HResult & 0xFFFF;

            // 32 / 33 = 文件正在被占用（Excel / WPS 打开）
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

            return; // ⛔ 关键：终止读取，避免 UI 半初始化
        }

        currentTable = Path.GetFileNameWithoutExtension(paths[0]);
        tableNameInput.text = currentTable;
        table.SetActive(true);

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 8) continue;

            var go = Instantiate(rowPrefab, rowRoot);
            var row = go.GetComponent<TableRowVisualSearchUI>();
            row.Init(currentTable, this);
            rows.Add(row);

            RestoreImage(cols[0], ref row.ltPath, row.ltImage, row.ltDropdown);
            RestoreImage(cols[1], ref row.rtPath, row.rtImage, row.rtDropdown);
            RestoreImage(cols[2], ref row.lbPath, row.lbImage, row.lbDropdown);
            RestoreImage(cols[3], ref row.rbPath, row.rbImage, row.rbDropdown);

            row.type1Input.text = cols[4];
            row.type2Input.text = cols[5];
            row.type3Input.text = cols[6];
            row.correctFacePosInput.text = cols[7];
        }
    }

    private void RestoreImage(string data, ref string path, Image img, Dropdown dd)
    {
        int s = data.IndexOf("(");
        int e = data.IndexOf(")");
        if (s >= 0 && e > s)
        {
            string tag = data.Substring(s + 1, e - s - 1);
            int idx = dd.options.FindIndex(o => o.text == tag);
            if (idx >= 0) dd.value = idx;
        }

        path = data.Substring(e + 1);
        string full = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg", path);

        if (File.Exists(full))
        {
            byte[] b = File.ReadAllBytes(full);
            Texture2D t = new Texture2D(2, 2);
            t.LoadImage(b);
            img.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.one * 0.5f);
        }
    }

}
