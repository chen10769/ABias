using UnityEngine;
using UnityEngine.UI;

public class TableRowVsUI : TableRowUI
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

    private TableEditorVsUI parent;

    [HideInInspector] public string ltPath;
    [HideInInspector] public string rtPath;
    [HideInInspector] public string lbPath;
    [HideInInspector] public string rbPath;

    public void Init(string tableName, TableEditorVsUI parent)
    {
        base.Init(tableName);
        this.parent = parent;

        ltBtn.onClick.AddListener(() => SelectAndCopyImage(ref ltPath, ltImage));
        rtBtn.onClick.AddListener(() => SelectAndCopyImage(ref rtPath, rtImage));
        lbBtn.onClick.AddListener(() => SelectAndCopyImage(ref lbPath, lbImage));
        rbBtn.onClick.AddListener(() => SelectAndCopyImage(ref rbPath, rbImage));

        correctFacePosInput.onValueChanged.AddListener(OnCorrectFacePosChanged);
    }

    private void OnCorrectFacePosChanged(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 1 || !"1234".Contains(value))
        {
            correctFacePosInput.text = "";
            TipManager.instance.ToShowTip("请输入1~4的数字(1表示左上，2是右上，3是左下，4是右下)");
        }
    }

    public override string[] GetRowData()
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

    protected override void OnDelete()
    {
        parent?.RemoveRow(this);
    }
}
