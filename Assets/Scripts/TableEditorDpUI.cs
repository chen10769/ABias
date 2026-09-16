using UnityEngine;
using UnityEngine.UI;

public class TableEditorDpUI : TableEditorUI<TableRowDpUI>
{
    

    protected override void OnCreateTable()
    {
         if (SettingsPanelController.instance.isVs) return;
        base.OnCreateTable();
        
    }
    protected override void OnReadCsv()
    {
         if (SettingsPanelController.instance.isVs) return;
        base.OnReadCsv();
        
    }

   

    protected override void InitRow(TableRowDpUI row)
    {
        row.Init(currentTableName, this);
    }

    protected override string GetCsvHeader()
    {
        return "左图,右图,探测点位置,性别,情绪类型,情绪面孔位置,图片,探测点一致性,控制条件";
    }

    protected override string[] GetRowData(TableRowDpUI row)
    {
        return row.GetRowData();
    }

    protected override void RestoreRow(string[] cols, TableRowDpUI row)
    {
        SetDropdownFromPath(cols[0], row.leftDropdown);
        row.leftImagePath = RemoveDropdownPrefix(cols[0]);
        LoadImage(row.leftImagePath, row.leftImage);

        SetDropdownFromPath(cols[1], row.rightDropdown);
        row.rightImagePath = RemoveDropdownPrefix(cols[1]);
        LoadImage(row.rightImagePath, row.rightImage);

        row.probeLeftToggle.isOn = cols[2].Contains("左");
        row.probeRightToggle.isOn = cols[2].Contains("右");

        row.genderInput.text = cols[3];
        row.emotionTypeInput.text = cols[4];
        row.facePosInput.text = cols[5];
        row.imageLabelInput.text = cols[6];
        row.probeConsistencyInput.text = cols[7];
        row.controlCondInput.text = cols[8];
    }
}
