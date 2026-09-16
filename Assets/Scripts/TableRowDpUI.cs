using UnityEngine;
using UnityEngine.UI;

public class TableRowDpUI : TableRowUI
{
    [Header("Images")]
    public Button leftImageBtn;
    public Button rightImageBtn;
    public Image leftImage;
    public Image rightImage;

    [Header("Probe Toggles")]
    public Toggle probeLeftToggle;
    public Toggle probeRightToggle;

    [Header("Dropdowns")]
    public Dropdown leftDropdown;
    public Dropdown rightDropdown;

    [Header("Inputs")]
    public InputField genderInput;
    public InputField emotionTypeInput;
    public InputField facePosInput;
    public InputField imageLabelInput;
    public InputField probeConsistencyInput;
    public InputField controlCondInput;

    private TableEditorDpUI parentEditor;

    [HideInInspector] public string leftImagePath;
    [HideInInspector] public string rightImagePath;

    public void Init(string tableName, TableEditorDpUI editor)
    {
        base.Init(tableName);
        this.parentEditor = editor;

        probeLeftToggle.onValueChanged.AddListener(_ => UpdateProbeConsistency());
        probeRightToggle.onValueChanged.AddListener(_ => UpdateProbeConsistency());
        leftDropdown.onValueChanged.AddListener(_ => UpdateProbeConsistency());
        rightDropdown.onValueChanged.AddListener(_ => UpdateProbeConsistency());

        leftImageBtn.onClick.AddListener(() => SelectAndCopyImage(ref leftImagePath, leftImage));
        rightImageBtn.onClick.AddListener(() => SelectAndCopyImage(ref rightImagePath, rightImage));
    }

    private void UpdateProbeConsistency()
    {
        string leftType = leftDropdown.options[leftDropdown.value].text;
        string rightType = rightDropdown.options[rightDropdown.value].text;

        bool probeLeft = probeLeftToggle.isOn;
        bool probeRight = probeRightToggle.isOn;

        string result = "";

        if (leftType == "中性" && rightType == "中性")
            result = "3";
        else if ((probeLeft && leftType == "中性") || (probeRight && rightType == "中性"))
            result = "2";
        else if ((probeLeft && leftType != "中性") || (probeRight && rightType != "中性"))
            result = "1";

        probeConsistencyInput.text = result;
    }

    public override string[] GetRowData()
    {
        string probePos = "";
        if (probeLeftToggle.isOn) probePos += "左";
        if (probeRightToggle.isOn) probePos += "右";
        if (string.IsNullOrEmpty(probePos)) probePos = "无";

        return new string[]
        {
            PackPath(leftImagePath, leftDropdown),
            PackPath(rightImagePath, rightDropdown),
            probePos,
            genderInput.text,
            emotionTypeInput.text,
            facePosInput.text,
            imageLabelInput.text,
            probeConsistencyInput.text,
            controlCondInput.text
        };
    }

    protected override void OnDelete()
    {
        parentEditor?.RemoveRow(this);
    }
}
