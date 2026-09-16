using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class ObjectAdjuster : MonoBehaviour
{
    [System.Serializable]
    public class AdjustItem
    {
        public string key;
        public Text label;
        public Button leftBtn;
        public Button rightBtn;
        public InputField input;
        public float defaultValue = 1f;
        [HideInInspector] public float value;
        public float step = 0.1f;
        public float min = -10f;
        public float max = 10f;
    }

    // === 图片大小位置设置 ===
    public AdjustItem a1a2ScaleX;
    public AdjustItem a1a2ScaleZ;
    public AdjustItem a1a2YDistance;
    public AdjustItem a1a2XPos;

    // === 敌人设置 ===
    public AdjustItem b1b2XDistance;
    public AdjustItem b1b2ZPos;

    // === 准心设置===
    public AdjustItem cSize;
    // === 玩家位置设置===
    public AdjustItem pZPos;

    // 场景物体
    public Transform a1;
    public Transform a2;
    public Transform b1;
    public Transform b2;
    public Transform c;
    public Transform p;
    [Header("Grid Layout 设置")]
    public GridLayoutGroup grid;
    public AdjustItem gridCellSizeX;
    public AdjustItem gridCellSizeY;
    public AdjustItem gridSpacingX;
    public AdjustItem gridSpacingY;


    public Button resetDefaultsBtn;

    private List<AdjustItem> allItems = new List<AdjustItem>();
    private string settingsFilePath;

    void Start()
    {
        // 文件路径 = 游戏根目录/SceneSettings.txt
        settingsFilePath = Path.Combine(Application.dataPath, "../SceneSettings.txt");

        allItems.AddRange(new AdjustItem[] {
    a1a2ScaleX, a1a2ScaleZ, a1a2YDistance, a1a2XPos,
    b1b2XDistance, b1b2ZPos, cSize, pZPos,
    gridCellSizeX, gridCellSizeY,
    gridSpacingX, gridSpacingY
});


        LoadSettings();

        foreach (var item in allItems)
        {
            item.value = Mathf.Clamp(item.value, item.min, item.max);
            item.input.text = item.value.ToString("F4");

            item.leftBtn.onClick.AddListener(() =>
            {
                item.value = Mathf.Clamp(item.value - item.step, item.min, item.max);
                item.input.text = item.value.ToString("F4");
                ApplyChanges();
                SaveSettings();
            });

            item.rightBtn.onClick.AddListener(() =>
            {
                item.value = Mathf.Clamp(item.value + item.step, item.min, item.max);
                item.input.text = item.value.ToString("F4");
                ApplyChanges();
                SaveSettings();
            });

            item.input.onEndEdit.AddListener((string s) =>
            {
                if (float.TryParse(s, out float v))
                {
                    item.value = Mathf.Clamp(v, item.min, item.max);
                    item.input.text = item.value.ToString("F4");
                    ApplyChanges();
                    SaveSettings();
                }
            });
        }

        if (resetDefaultsBtn != null)
        {
            resetDefaultsBtn.onClick.AddListener(() =>
            {
                ResetToDefault();
            });
        }

        ApplyChanges();
    }

    // === 自动加载或创建文件 ===
    private void LoadSettings()
    {
        Dictionary<string, float> loaded = new Dictionary<string, float>();

        if (File.Exists(settingsFilePath))
        {
            foreach (var line in File.ReadAllLines(settingsFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                string[] parts = line.Split('=');
                if (parts.Length == 2 && float.TryParse(parts[1], out float val))
                {
                    loaded[parts[0]] = val;
                }
            }
        }
        else
        {
            // 没有文件就创建默认文件
            SaveSettings(true);
            Debug.Log($"未找到 SceneSettings.txt，已创建默认设置文件。");
        }

        foreach (var item in allItems)
        {
            if (loaded.ContainsKey(item.key))
                item.value = loaded[item.key];
            else
                item.value = item.defaultValue;
        }
    }

    // === 保存设置 ===
    private void SaveSettings(bool forceDefault = false)
    {
        using (StreamWriter sw = new StreamWriter(settingsFilePath, false))
        {
            foreach (var item in allItems)
            {
                float val = forceDefault ? item.defaultValue : item.value;
                sw.WriteLine($"{item.key}={val}");
            }
        }
    }

    private void ResetToDefault()
    {
        foreach (var item in allItems)
        {
            item.value = Mathf.Clamp(item.defaultValue, item.min, item.max);
            item.input.text = item.value.ToString("F4");
        }
        SaveSettings(true);
        ApplyChanges();
    }

    private void ApplyChanges()
    {
        // === a1,a2 缩放 ===
        Vector3 scaleA1 = a1.localScale;
        Vector3 scaleA2 = a2.localScale;
        scaleA1.x = scaleA2.x = a1a2ScaleX.value;
        scaleA1.z = scaleA2.z = a1a2ScaleZ.value;
        a1.localScale = scaleA1;
        a2.localScale = scaleA2;

        // === a1,a2 Y间距 + X位置 ===
        Vector3 posA1 = a1.localPosition;
        Vector3 posA2 = a2.localPosition;
        posA1.y = a1a2YDistance.value / 2f;
        posA2.y = a1a2YDistance.value / 2f;
        posA1.x = posA2.x = a1a2XPos.value;
        a1.localPosition = posA1;
        a2.localPosition = posA2;

        // === b1,b2 X间距 + Z位置 ===
        Vector3 posB1 = b1.localPosition;
        Vector3 posB2 = b2.localPosition;
        float dist = Mathf.Abs(b1b2XDistance.value);
        posB1.x = dist / 2f;
        posB2.x = -dist / 2f;
        posB1.z = posB2.z = b1b2ZPos.value;
        b1.localPosition = posB1;
        b2.localPosition = posB2;

        // === c 大小 ===
        c.localScale = Vector3.one * cSize.value;

        // === p Z位置 ===
        Vector3 posP = p.localPosition;
        posP.z = pZPos.value;
        p.localPosition = posP;
        // === GridLayoutGroup 设置 ===
        if (grid != null)
        {
            Vector2 cell = grid.cellSize;
            cell.x = gridCellSizeX.value * 50;
            cell.y = gridCellSizeY.value * 50;
            grid.cellSize = cell;

            Vector2 spacing = grid.spacing;
            spacing.x = gridSpacingX.value * 50;
            spacing.y = gridSpacingY.value * 50;
            grid.spacing = spacing;
        }

        //FirstPersonController.instance.gameObject.transform.position = FirstPersonController.instance.startPos.position;
        FirstPersonController.instance.BackToCenter();
        FirstPersonController.instance.HandleAutoAim();
    }
}
