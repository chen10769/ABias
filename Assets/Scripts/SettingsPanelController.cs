// SettingsPanelController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsPanelController : MonoBehaviour
{
    public bool isVs = false;
    [Header("UI References")]
    public GameObject dotProbeSettings;
    public GameObject visualSearchSettings;

    // DotProbe UI 元素
    public InputField dotFloorsInput;
    public InputField dotDoorOpenMinInput;
    public InputField dotDoorOpenMaxInput;
    public InputField dotFloorIntervalMinInput;
    public InputField dotFloorIntervalMaxInput;
    public InputField dotGuardReactionInput;
    public InputField dotDoorCloseMinInput;
    public InputField dotDoorCloseMaxInput;
    public InputField dotImageSetInput;
    public Slider dotNeutralProbSlider;
    public Text dotNeutralProbText;
    public InputField dotBreakTrialsInput;   // 每多少次进入休息
    public InputField dotBreakDurationInput; // 休息时间（秒）
                                             // 新增 练习阶段 UI
    public Toggle dotHasPracticeToggle;
    public InputField dotPracticeCsvInput;
    public Toggle dotShowHpToggle;
    public Toggle dotShowRedScreenToggle;
    public Toggle dotShowEvaToggle;
    // DotProbe 阈值
    public InputField dotPerfectThresholdInput;
    public InputField dotGreatThresholdInput;

    // VisualSearch UI 元素
    public InputField vsIntervalMinInput;
    public InputField vsIntervalMaxInput;
    public InputField vsSelectionMinInput;
    public InputField vsSelectionMaxInput;

    public InputField vsLoopCountInput;

    public Toggle vsHasPracticeToggle;

    public InputField vsPracticeCsvInput;
    public InputField vsFormalCsvInput;

    public InputField vsBreakTrialsInput;
    public InputField vsBreakDurationInput;


    // VisualSearch 阈值
    public InputField vsPerfectThresholdInput;
    public InputField vsGreatThresholdInput;
    public Toggle vsEvaToggle;
    public static SettingsPanelController instance;
    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        // 同时显示两种实验的设置
        // dotProbeSettings.SetActive(true);
        // visualSearchSettings.SetActive(true);

        // 初始化概率滑块文本更新
        dotNeutralProbSlider.onValueChanged.AddListener(UpdateNeutralProbText);

        // 加载保存的设置到UI
        LoadSettingsToUI();
    }

    // 更新中性概率文本
    private void UpdateNeutralProbText(float value)
    {
        dotNeutralProbText.text = value.ToString("P0"); // 显示为百分比格式
    }

    // 从PlayerPrefs加载设置到UI
    private void LoadSettingsToUI()
    {
        var data = typeof(SettingsManager)
            .GetMethod("LoadAllSettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, null) as System.Collections.Generic.Dictionary<string, string>;

        string GetStr(string key, string def) => data.ContainsKey(key) ? data[key] : def;
        int GetInt(string key, int def) => int.TryParse(GetStr(key, def.ToString()), out var v) ? v : def;
        float GetFloat(string key, float def) => float.TryParse(GetStr(key, def.ToString()), out var v) ? v : def;

        // DotProbe 设置
        dotFloorsInput.text = GetInt("Dot_Floors", 5).ToString();
        dotDoorOpenMinInput.text = GetFloat("Dot_DoorOpenDelayMin", 1f).ToString();
        dotDoorOpenMaxInput.text = GetFloat("Dot_DoorOpenDelayMax", 1f).ToString();
        dotFloorIntervalMinInput.text = GetFloat("Dot_FloorIntervalMin", 2f).ToString();
        dotFloorIntervalMaxInput.text = GetFloat("Dot_FloorIntervalMax", 2f).ToString();
        dotGuardReactionInput.text = GetFloat("Dot_GuardReaction", 0.5f).ToString();
        dotDoorCloseMinInput.text = GetFloat("Dot_DoorCloseDelayMin", 0.5f).ToString();
        dotDoorCloseMaxInput.text = GetFloat("Dot_DoorCloseDelayMax", 1.5f).ToString();
        dotImageSetInput.text = GetStr("Dot_ImageSet", "Test1");
        dotBreakTrialsInput.text = GetInt("Dot_BreakTrials", 20).ToString();
        dotBreakDurationInput.text = GetFloat("Dot_BreakDuration", 30f).ToString();
        dotHasPracticeToggle.isOn = GetInt("Dot_HasPractice", 1) == 1;
        dotPracticeCsvInput.text = GetStr("Dot_PracticeName", "practice");
        dotShowHpToggle.isOn = GetInt("Dot_ShowHp", 1) == 1;
        dotShowRedScreenToggle.isOn = GetInt("Dot_ShowRedScreen", 1) == 1;
        dotShowEvaToggle.isOn = GetInt("Dot_ShowEva", 1) == 1;

        float neutralProb = GetFloat("Dot_NeutralProb", 0.5f);
        // DotProbe 阈值
        dotPerfectThresholdInput.text = GetFloat("Dot_PerfectThreshold", 0.3f).ToString();
        dotGreatThresholdInput.text = GetFloat("Dot_GreatThreshold", 0.5f).ToString();
        dotNeutralProbSlider.value = neutralProb;
        UpdateNeutralProbText(neutralProb);

        // VisualSearch 设置
        vsIntervalMinInput.text = GetFloat("VS_IntervalMin", 0.4f).ToString();
        vsIntervalMaxInput.text = GetFloat("VS_IntervalMax", 0.6f).ToString();

        vsSelectionMinInput.text = GetFloat("VS_SelectionMin", 2.5f).ToString();
        vsSelectionMaxInput.text = GetFloat("VS_SelectionMax", 3.5f).ToString();

        vsLoopCountInput.text = GetInt("VS_LoopCount", 1).ToString();

        vsHasPracticeToggle.isOn = GetInt("VS_HasPractice", 1) == 1;

        vsPracticeCsvInput.text = GetStr("VS_PracticeCsv", "VS1.csv");
        vsFormalCsvInput.text = GetStr("VS_FormalCsv", "VS1.csv");

        vsBreakTrialsInput.text = GetInt("VS_BreakTrials", 20).ToString();
        vsBreakDurationInput.text = GetFloat("VS_BreakDuration", 10f).ToString();
        vsEvaToggle.isOn = GetInt("VS_ShowEva", 1) == 1;
        // VisualSearch 阈值
        vsPerfectThresholdInput.text = GetFloat("VS_PerfectThreshold", 0.5f).ToString();
        vsGreatThresholdInput.text = GetFloat("VS_GreatThreshold", 1.0f).ToString();

    }

    // 保存设置按钮事件
    public void OnSaveSettings()
    {
        // 保存 DotProbe 设置
        int floors = ParseIntSafe(dotFloorsInput.text, 5);
        float doorOpenMin = ParseFloatSafe(dotDoorOpenMinInput.text, 1f);
        float doorOpenMax = ParseFloatSafe(dotDoorOpenMaxInput.text, 1f);
        float floorIntervalMin = ParseFloatSafe(dotFloorIntervalMinInput.text, 2f);
        float floorIntervalMax = ParseFloatSafe(dotFloorIntervalMaxInput.text, 2f);
        float guardReaction = ParseFloatSafe(dotGuardReactionInput.text, 0.5f);
        float doorCloseMin = ParseFloatSafe(dotDoorCloseMinInput.text, 0.5f);
        float doorCloseMax = ParseFloatSafe(dotDoorCloseMaxInput.text, 1.5f);
        string dotImageSet = dotImageSetInput.text;
        float neutralProb = dotNeutralProbSlider.value;
        int breakTrials = ParseIntSafe(dotBreakTrialsInput.text, 20);
        float breakDuration = ParseFloatSafe(dotBreakDurationInput.text, 30f);
        bool hasPractice = dotHasPracticeToggle.isOn;
        string practiceCsv = dotPracticeCsvInput.text;
        bool willShowHp = dotShowHpToggle.isOn;
        bool willShowRedScreen = dotShowRedScreenToggle.isOn;
        bool willShowEva = dotShowEvaToggle.isOn;
        float dotPerfectThreshold = ParseFloatSafe(dotPerfectThresholdInput.text, 0.3f);
        float dotGreatThreshold = ParseFloatSafe(dotGreatThresholdInput.text, 0.5f);

        SettingsManager.SaveDotProbeSettings(
      floors,
      new Vector2(doorOpenMin, doorOpenMax),
      guardReaction,
      new Vector2(doorCloseMin, doorCloseMax),
      new Vector2(floorIntervalMin, floorIntervalMax),
      dotImageSet,
      neutralProb,
      breakTrials,
      breakDuration,
      hasPractice,
      practiceCsv,
      willShowHp,
      willShowRedScreen,
      willShowEva,
          dotPerfectThreshold,
    dotGreatThreshold // 新增
  );

        // 保存 VisualSearch 设置
        float intervalMin = ParseFloatSafe(vsIntervalMinInput.text, 0.4f);
        float intervalMax = ParseFloatSafe(vsIntervalMaxInput.text, 0.6f);
        float selectionMin = ParseFloatSafe(vsSelectionMinInput.text, 2.5f);
        float selectionMax = ParseFloatSafe(vsSelectionMaxInput.text, 3.5f);
        float vsPerfectThreshold = ParseFloatSafe(vsPerfectThresholdInput.text, 0.5f);
        float vsGreatThreshold = ParseFloatSafe(vsGreatThresholdInput.text, 1.0f);


        VisualSearchExperiment dummyVS = new VisualSearchExperiment
        {
            intervalTimeRange = new Vector2(intervalMin, intervalMax),
            selectionTimeRange = new Vector2(selectionMin, selectionMax),
            loopCount = ParseIntSafe(vsLoopCountInput.text, 1),

            hasPractice = vsHasPracticeToggle.isOn,
            practiceCsv = vsPracticeCsvInput.text,
            formalCsv = vsFormalCsvInput.text,

            trialsPerBreak = ParseIntSafe(vsBreakTrialsInput.text, 20),
            breakDuration = ParseFloatSafe(vsBreakDurationInput.text, 3f),
            willShowEva = vsEvaToggle.isOn,


        };
        dummyVS.perfectThreshold = vsPerfectThreshold;
        dummyVS.greatThreshold = vsGreatThreshold;


        SettingsManager.SaveVisualSearchSettings(dummyVS);


        Debug.Log("所有实验设置已保存！");
        gameObject.SetActive(false);
    }

    // 安全解析整数（避免无效输入）
    private int ParseIntSafe(string input, int defaultValue)
    {
        int result;
        return int.TryParse(input, out result) ? result : defaultValue;
    }

    // 安全解析浮点数（避免无效输入）
    private float ParseFloatSafe(string input, float defaultValue)
    {
        float result;
        return float.TryParse(input, out result) ? result : defaultValue;
    }

    // 重置设置按钮事件
    public void OnResetSettings()
    {
        PlayerPrefs.DeleteAll();
        LoadSettingsToUI();
        Debug.Log("设置已重置为默认值！");
    }

    // 开始游戏按钮事件
    public void OnStartGame()
    {
        // 加载游戏场景
        SceneManager.LoadScene("GameScene");
    }
    public void SetBool(bool bl)
    {
        isVs = bl;
    }
}