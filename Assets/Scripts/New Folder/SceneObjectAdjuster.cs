using UnityEngine;

public class SceneObjectAdjuster : MonoBehaviour
{
    [Header("场景物体引用")]
    public GameObject a1;
    public GameObject a2;
    public GameObject b1;
    public GameObject b2;
    public GameObject c;
    public GameObject p;
    
    private AdjustmentData currentData;
    
    void Start()
    {
        ApplySavedData();
    }
    
    public void ApplySavedData()
    {
        currentData = AdjustmentDataManager.Instance.CurrentData;
        ApplyAllAdjustments();
    }
    
    public void ApplyAllAdjustments()
    {
        ApplyAObjectsAdjustment();
        ApplyBObjectsAdjustment();
        ApplyCAdjustment();
        ApplyPAdjustment();
    }
    
    private void ApplyAObjectsAdjustment()
    {
        if (a1 != null)
        {
            a1.transform.localScale = Vector3.one * currentData.a1Size;
            Vector3 a1Pos = a1.transform.position;
            a1Pos.x = currentData.aXPosition;
            a1Pos.y = currentData.aYSpacing / 2f;
            a1.transform.position = a1Pos;
        }
        
        if (a2 != null)
        {
            a2.transform.localScale = Vector3.one * currentData.a2Size;
            Vector3 a2Pos = a2.transform.position;
            a2Pos.x = currentData.aXPosition;
            a2Pos.y = -currentData.aYSpacing / 2f;
            a2.transform.position = a2Pos;
        }
    }
    
    private void ApplyBObjectsAdjustment()
    {
        if (b1 != null)
        {
            Vector3 b1Pos = b1.transform.position;
            b1Pos.x = -currentData.bXSpacing / 2f;
            b1Pos.z = currentData.bZPosition;
            b1.transform.position = b1Pos;
        }
        
        if (b2 != null)
        {
            Vector3 b2Pos = b2.transform.position;
            b2Pos.x = currentData.bXSpacing / 2f;
            b2Pos.z = currentData.bZPosition;
            b2.transform.position = b2Pos;
        }
    }
    
    private void ApplyCAdjustment()
    {
        if (c != null)
        {
            c.transform.localScale = Vector3.one * currentData.cSize;
        }
    }
    
    private void ApplyPAdjustment()
    {
        if (p != null)
        {
            Vector3 pPos = p.transform.position;
            pPos.z = currentData.pZPosition;
            p.transform.position = pPos;
        }
    }
    
    // 公共方法用于外部更新数据
    public void UpdateAdjustmentData(AdjustmentData newData)
    {
        currentData = newData;
        ApplyAllAdjustments();
        AdjustmentDataManager.Instance.UpdateDataAndSave(newData);
    }
    
    public AdjustmentData GetCurrentData()
    {
        return currentData;
    }
}