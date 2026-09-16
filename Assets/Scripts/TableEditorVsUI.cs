using UnityEngine;
using UnityEngine.UI;

public class TableEditorVsUI : TableEditorUI<TableRowVsUI>
{


    protected override void OnCreateTable()
    {
         if (!SettingsPanelController.instance.isVs) return;
        base.OnCreateTable();
        
    }
    protected override void OnReadCsv()
    {
         if (!SettingsPanelController.instance.isVs) return;
        base.OnReadCsv();
        
    }

    

    protected override void InitRow(TableRowVsUI row)
    {
        
        row.Init(currentTableName, this);
    }

    protected override string GetCsvHeader()
    {
        return "左上图,右上图,左下图,右下图,性别,图片,情绪面孔位置,正确面孔位置";
    }

    protected override string[] GetRowData(TableRowVsUI row)
    {
        return row.GetRowData();
    }

    protected override void RestoreRow(string[] cols, TableRowVsUI row)
    {
        Restore(cols[0], ref row.ltPath, row.ltImage, row.ltDropdown);
        Restore(cols[1], ref row.rtPath, row.rtImage, row.rtDropdown);
        Restore(cols[2], ref row.lbPath, row.lbImage, row.lbDropdown);
        Restore(cols[3], ref row.rbPath, row.rbImage, row.rbDropdown);

        row.type1Input.text = cols[4];
        row.type2Input.text = cols[5];
        row.type3Input.text = cols[6];
        row.correctFacePosInput.text = cols[7];
    }

    private void Restore(string data, ref string path, Image img, Dropdown dd)
    {
        SetDropdownFromPath(data, dd);
        path = RemoveDropdownPrefix(data);
        LoadImage(path, img);
    }
}
