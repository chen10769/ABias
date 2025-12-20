using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;

public class TableRowUI : MonoBehaviour
{
    public Button leftImageBtn;
    public Button rightImageBtn;
    public Image leftImage;
    public Image rightImage;

    // 探测点 Toggle
    public Toggle probeLeftToggle;
    public Toggle probeRightToggle;

    // 新增 Dropdown
    public Dropdown leftDropdown;
    public Dropdown rightDropdown;

    public InputField genderInput;
    public InputField emotionTypeInput;
    public InputField facePosInput;
    public InputField imageLabelInput;
    public InputField probeConsistencyInput;
    public InputField controlCondInput;

    public Button deleteBtn;

    private string tableName;
    public string leftImagePath;
    public string rightImagePath;
    private TableEditorUI parentEditor;

    public void Init(string tableName, TableEditorUI editor)
    {
        this.tableName = tableName;
        this.parentEditor = editor;
        // 绑定事件
        probeLeftToggle.onValueChanged.AddListener(_ => UpdateProbeConsistency());
        probeRightToggle.onValueChanged.AddListener(_ => UpdateProbeConsistency());
        leftDropdown.onValueChanged.AddListener(_ => UpdateProbeConsistency());
        rightDropdown.onValueChanged.AddListener(_ => UpdateProbeConsistency());
    }
    private void UpdateProbeConsistency()
    {
        string leftType = leftDropdown.options[leftDropdown.value].text;
        string rightType = rightDropdown.options[rightDropdown.value].text;

        bool probeLeft = probeLeftToggle.isOn;
        bool probeRight = probeRightToggle.isOn;

        string result = "";

        // 规则 3：如果两边都是中性
        if (leftType == "中性" && rightType == "中性")
        {
            result = "3";
        }
        else if (probeLeft && leftType == "中性") // 规则 1
        {
            result = "2";
        }
        else if (probeRight && rightType == "中性") // 规则 1
        {
            result = "2";
        }
        else if (probeLeft && leftType != "中性") // 规则 2
        {
            result = "1";
        }
        else if (probeRight && rightType != "中性") // 规则 2
        {
            result = "1";
        }
        Debug.Log(result);
        probeConsistencyInput.text = result;
    }

    public void OnClickImageLeft() => SelectAndSetImage(true, leftImage);
    public void OnClickImageRight() => SelectAndSetImage(false, rightImage);

    private void SelectAndSetImage(bool isLeft, Image target)
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "选择图片",
            "",
            new ExtensionFilter[] {
                new ExtensionFilter("图片文件", "png", "jpg", "jpeg")
            },
            false
        );

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string srcPath = paths[0];
            string destPath = CopyToTableFolder(srcPath);

            if (isLeft) leftImagePath = destPath;
            else rightImagePath = destPath;

            StartCoroutine(LoadImage(srcPath, target));
        }
    }

    private System.Collections.IEnumerator LoadImage(string path, Image target)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                         new Vector2(0.5f, 0.5f));
        target.sprite = newSprite;
        yield return null;
    }

    private string CopyToTableFolder(string srcPath)
    {
        string tableFolderPath = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg", tableName);
        if (!Directory.Exists(tableFolderPath))
            Directory.CreateDirectory(tableFolderPath);

        string fileName = Path.GetFileName(srcPath);
        string destPath = Path.Combine(tableFolderPath, fileName);
        File.Copy(srcPath, destPath, true);

        return $"{tableName}/{fileName}";
    }

    public string[] GetRowData()
{
    // 探测点
    string probePos = "";
    if (probeLeftToggle.isOn) probePos += "左";
    if (probeRightToggle.isOn) probePos += "右";
    if (string.IsNullOrEmpty(probePos)) probePos = "无";

    // 左图路径：保存时加上 Dropdown 显示文本，但不改变 leftImagePath
    string leftText = leftImagePath;
    if (leftDropdown != null && leftDropdown.options.Count > 0)
    {
        string dropdownText = leftDropdown.options[leftDropdown.value].text;
        leftText = $"({dropdownText}){leftImagePath}";
    }

    string rightText = rightImagePath;
    if (rightDropdown != null && rightDropdown.options.Count > 0)
    {
        string dropdownText = rightDropdown.options[rightDropdown.value].text;
        rightText = $"({dropdownText}){rightImagePath}";
    }

    return new string[] {
        leftText,
        rightText,
        probePos,
        genderInput.text,
        emotionTypeInput.text,
        facePosInput.text,
        imageLabelInput.text,
        probeConsistencyInput.text,
        controlCondInput.text
    };
}


    public void OnDeleteRow()
    {
        parentEditor?.RemoveRow(this);
    }
}
