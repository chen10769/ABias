using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdjustmentUIController : MonoBehaviour
{

   [Header("UI输入框引用")]
    public InputField a1SizeInput;
    public InputField a2SizeInput;
    public InputField aYSpacingInput;
    public InputField aXPositionInput;
    public InputField bXSpacingInput;
    public InputField bZPositionInput;
    public InputField cSizeInput;
    public InputField pZPositionInput;
    [Header("数值限制")]
    public float minSize = 0.1f;
    public float maxSize = 10f;
    public float minSpacing = 0.1f;
    public float maxSpacing = 20f;
    public float minPosition = -50f;
    public float maxPosition = 50f;
    
    private SceneObjectAdjuster sceneAdjuster;
    
    void Start()
    {
        sceneAdjuster = FindObjectOfType<SceneObjectAdjuster>();
        InitializeUI();
        SetupInputEvents();
    }
    
    private void InitializeUI()
    {
        AdjustmentData data = sceneAdjuster.GetCurrentData();
        
        // 设置初始值
        a1SizeInput.text = data.a1Size.ToString("F2");
        a2SizeInput.text = data.a2Size.ToString("F2");
        aYSpacingInput.text = data.aYSpacing.ToString("F2");
        aXPositionInput.text = data.aXPosition.ToString("F2");
        bXSpacingInput.text = data.bXSpacing.ToString("F2");
        bZPositionInput.text = data.bZPosition.ToString("F2");
        cSizeInput.text = data.cSize.ToString("F2");
        pZPositionInput.text = data.pZPosition.ToString("F2");
    }
    
    private void SetupInputEvents()
    {
        // 绑定输入框变化事件
        a1SizeInput.onValueChanged.AddListener(value => OnInputFieldChanged(value, "a1Size"));
        a2SizeInput.onValueChanged.AddListener(value => OnInputFieldChanged(value, "a2Size"));
        aYSpacingInput.onValueChanged.AddListener(value => OnInputFieldChanged(value, "aYSpacing"));
        aXPositionInput.onValueChanged.AddListener(value => OnInputFieldChanged(value, "aXPosition"));
        bXSpacingInput.onValueChanged.AddListener(value => OnInputFieldChanged(value, "bXSpacing"));
        bZPositionInput.onValueChanged.AddListener(value => OnInputFieldChanged(value, "bZPosition"));
        cSizeInput.onValueChanged.AddListener(value => OnInputFieldChanged(value, "cSize"));
        pZPositionInput.onValueChanged.AddListener(value => OnInputFieldChanged(value, "pZPosition"));
    }
    
    private void OnInputFieldChanged(string value, string fieldName)
    {
        if (float.TryParse(value, out float floatValue))
        {
            AdjustmentData newData = sceneAdjuster.GetCurrentData();
            
            // 根据字段名更新对应数据
            switch (fieldName)
            {
                case "a1Size":
                    newData.a1Size = Mathf.Clamp(floatValue, minSize, maxSize);
                    a1SizeInput.text = newData.a1Size.ToString("F2");
                    break;
                case "a2Size":
                    newData.a2Size = Mathf.Clamp(floatValue, minSize, maxSize);
                    a2SizeInput.text = newData.a2Size.ToString("F2");
                    break;
                case "aYSpacing":
                    newData.aYSpacing = Mathf.Clamp(floatValue, minSpacing, maxSpacing);
                    aYSpacingInput.text = newData.aYSpacing.ToString("F2");
                    break;
                case "aXPosition":
                    newData.aXPosition = Mathf.Clamp(floatValue, minPosition, maxPosition);
                    aXPositionInput.text = newData.aXPosition.ToString("F2");
                    break;
                case "bXSpacing":
                    newData.bXSpacing = Mathf.Clamp(floatValue, minSpacing, maxSpacing);
                    bXSpacingInput.text = newData.bXSpacing.ToString("F2");
                    break;
                case "bZPosition":
                    newData.bZPosition = Mathf.Clamp(floatValue, minPosition, maxPosition);
                    bZPositionInput.text = newData.bZPosition.ToString("F2");
                    break;
                case "cSize":
                    newData.cSize = Mathf.Clamp(floatValue, minSize, maxSize);
                    cSizeInput.text = newData.cSize.ToString("F2");
                    break;
                case "pZPosition":
                    newData.pZPosition = Mathf.Clamp(floatValue, minPosition, maxPosition);
                    pZPositionInput.text = newData.pZPosition.ToString("F2");
                    break;
            }
            
            sceneAdjuster.UpdateAdjustmentData(newData);
        }
    }
    
    // 按钮点击方法
    public void OnArrowButtonClick(string fieldName, bool isIncrement)
    {
        float step = 0.1f;
        AdjustmentData newData = sceneAdjuster.GetCurrentData();
        
        switch (fieldName)
        {
            case "a1Size":
                newData.a1Size = Mathf.Clamp(newData.a1Size + (isIncrement ? step : -step), minSize, maxSize);
                a1SizeInput.text = newData.a1Size.ToString("F2");
                break;
            case "a2Size":
                newData.a2Size = Mathf.Clamp(newData.a2Size + (isIncrement ? step : -step), minSize, maxSize);
                a2SizeInput.text = newData.a2Size.ToString("F2");
                break;
            case "aYSpacing":
                newData.aYSpacing = Mathf.Clamp(newData.aYSpacing + (isIncrement ? step : -step), minSpacing, maxSpacing);
                aYSpacingInput.text = newData.aYSpacing.ToString("F2");
                break;
            case "aXPosition":
                newData.aXPosition = Mathf.Clamp(newData.aXPosition + (isIncrement ? step : -step), minPosition, maxPosition);
                aXPositionInput.text = newData.aXPosition.ToString("F2");
                break;
            case "bXSpacing":
                newData.bXSpacing = Mathf.Clamp(newData.bXSpacing + (isIncrement ? step : -step), minSpacing, maxSpacing);
                bXSpacingInput.text = newData.bXSpacing.ToString("F2");
                break;
            case "bZPosition":
                newData.bZPosition = Mathf.Clamp(newData.bZPosition + (isIncrement ? step : -step), minPosition, maxPosition);
                bZPositionInput.text = newData.bZPosition.ToString("F2");
                break;
            case "cSize":
                newData.cSize = Mathf.Clamp(newData.cSize + (isIncrement ? step : -step), minSize, maxSize);
                cSizeInput.text = newData.cSize.ToString("F2");
                break;
            case "pZPosition":
                newData.pZPosition = Mathf.Clamp(newData.pZPosition + (isIncrement ? step : -step), minPosition, maxPosition);
                pZPositionInput.text = newData.pZPosition.ToString("F2");
                break;
        }
        
        sceneAdjuster.UpdateAdjustmentData(newData);
    }
}