using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;

public class TableRowVisualSearchUI : MonoBehaviour
{
    [Header("Images")]
    public Button ltBtn;
    public Button rtBtn;
    public Button lbBtn;
    public Button rbBtn;

    public Image ltImage;
    public Image rtImage;
    public Image lbImage;
    public Image rbImage;

    public Dropdown ltDropdown;
    public Dropdown rtDropdown;
    public Dropdown lbDropdown;
    public Dropdown rbDropdown;

    [Header("Inputs")]
    public InputField type1Input;
    public InputField type2Input;
    public InputField type3Input;
    public InputField correctFacePosInput;

    public Button deleteBtn;

    [HideInInspector] public string ltPath;
    [HideInInspector] public string rtPath;
    [HideInInspector] public string lbPath;
    [HideInInspector] public string rbPath;

    private string tableName;
    private TableEditorVisualSearchUI parent;

    public void Init(string tableName, TableEditorVisualSearchUI parent)
    {
        this.tableName = tableName;
        this.parent = parent;

        ltBtn.onClick.AddListener(() => SelectImage(ref ltPath, ltImage));
        rtBtn.onClick.AddListener(() => SelectImage(ref rtPath, rtImage));
        lbBtn.onClick.AddListener(() => SelectImage(ref lbPath, lbImage));
        rbBtn.onClick.AddListener(() => SelectImage(ref rbPath, rbImage));
        correctFacePosInput.onValueChanged.AddListener(OnCorrectFacePosChanged);

        deleteBtn.onClick.AddListener(OnDelete);
    }

    private void SelectImage(ref string savePath, Image target)
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "选择图片", "", new[] { new ExtensionFilter("Image", "png", "jpg", "jpeg") }, false);

        if (paths.Length == 0) return;

        string src = paths[0];
        string dest = CopyToTableFolder(src);
        savePath = dest;

        byte[] data = File.ReadAllBytes(src);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        target.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
    }

    private string CopyToTableFolder(string srcPath)
    {
        string folder = Path.Combine(Application.dataPath, "StreamingAssets", "TestImg", tableName);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string file = Path.GetFileName(srcPath);
        string dest = Path.Combine(folder, file);
        File.Copy(srcPath, dest, true);

        return $"{tableName}/{file}";
    }

    private string PackPath(string path, Dropdown dd)
    {
        if (string.IsNullOrEmpty(path)) return "";
        string tag = dd.options[dd.value].text;
        return $"({tag}){path}";
    }

    public string[] GetRowData()
    {
        return new string[]
        {
            PackPath(ltPath, ltDropdown),
            PackPath(rtPath, rtDropdown),
            PackPath(lbPath, lbDropdown),
            PackPath(rbPath, rbDropdown),
            type1Input.text,
            type2Input.text,
            type3Input.text,
            correctFacePosInput.text
        };
    }
    private void OnCorrectFacePosChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            TipManager.instance.ToShowTip("请输入1~4的数字(1表示左上，2是右上，3是左下，4是右下)");
            return;
        }


        // 只允许单个字符
        if (value.Length > 1)
        {
            correctFacePosInput.text = "";
            TipManager.instance.ToShowTip("请输入1~4的数字(1表示左上，2是右上，3是左下，4是右下)");
            return;
        }

        // 只允许 1~4
        if (value != "1" && value != "2" && value != "3" && value != "4")
        {
            correctFacePosInput.text = "";
            TipManager.instance.ToShowTip("请输入1~4的数字(1表示左上，2是右上，3是左下，4是右下)");
        }
        
    }


    public void OnDelete()
    {
        parent.RemoveRow(this);
    }
}
