using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// SettingsManager
/// —— 从 TXT 文件加载和保存实验设置（替代 PlayerPrefs）
/// 文件位置：游戏根目录（即可执行文件所在文件夹）
/// 文件名：ExperimentSettings.txt
/// </summary>
public static class SettingsManager
{
    private static string settingsFilePath = Path.Combine(Application.dataPath, "../ExperimentSettings.txt");

    // -------------------- 初始化 --------------------
    static SettingsManager()
    {
        EnsureSettingsFileExists();
    }

    // 确保设置文件存在（如不存在则自动创建默认文件）
    private static void EnsureSettingsFileExists()
    {
        if (!File.Exists(settingsFilePath))
        {
            Debug.Log("未找到 ExperimentSettings.txt，正在创建默认文件...");

            Dictionary<string, string> defaults = new Dictionary<string, string>()
            {
                // DotProbe 默认设置
                ["Dot_Floors"] = "5",
                ["Dot_DoorOpenDelayMin"] = "1",
                ["Dot_DoorOpenDelayMax"] = "1.5",
                ["Dot_FloorIntervalMin"] = "1",
                ["Dot_FloorIntervalMax"] = "2",
                ["Dot_GuardReaction"] = "0.5",
                ["Dot_DoorCloseDelayMin"] = "0.5",
                ["Dot_DoorCloseDelayMax"] = "1.5",
                ["Dot_ImageSet"] = "Test2",
                ["Dot_NeutralProb"] = "0.5",
                ["Dot_BreakTrials"] = "20",
                ["Dot_BreakDuration"] = "30",
                ["Dot_HasPractice"] = "1",
                ["Dot_PracticeName"] = "pratice",
                ["Dot_ShowHp"] = "1",
                ["Dot_ShowRedScreen"] = "1",
                ["Dot_ShowEva"] = "1",
                ["Dot_PerfectThreshold"] = "0.5",
                ["Dot_GreatThreshold"] = "1",

                ["Dot_HavePre"] = "1",
                ["Dot_HavePost"] = "1",


                // VisualSearch 默认设置
                ["VS_IntervalMin"] = "0.4",
                ["VS_IntervalMax"] = "0.6",
                ["VS_SelectionMin"] = "2.5",
                ["VS_SelectionMax"] = "3.5",
                ["VS_LoopCount"] = "1",
                ["VS_HasPractice"] = "1",
                ["VS_PracticeCsv"] = "VS1",
                ["VS_FormalCsv"] = "VS1",
                ["VS_BreakTrials"] = "20",
                ["VS_BreakDuration"] = "3",
                ["VS_ShowEva"] = "3",
                ["VS_PerfectThreshold"] = "0.5",
                ["VS_GreatThreshold"] = "1.0",
                ["VS_TrueCount"] = "2",

                ["VS_HaveTest"] = "1",




            };

            SaveAllSettings(defaults);
        }
    }

    // -------------------- 保存 DotProbe 设置 --------------------
    public static void SaveDotProbeSettings(
        int floors, Vector2 doorOpenDelayRange,
        float guardReaction, Vector2 doorCloseDelayRange, Vector2 floorIntervalRange,
        string imageSet, float neutralProb,
        int breakTrials, float breakDuration,
        bool hasPractice, string practiceCsvFileName,
        bool willShowHp, bool willShowRedScreen, bool willShowEva, float perfectThreshold, float greatThreshold, bool havePre,
    bool havePost
    )
    {
        Dictionary<string, string> data = LoadAllSettings();
        data["Dot_Floors"] = floors.ToString();
        data["Dot_DoorOpenDelayMin"] = doorOpenDelayRange.x.ToString();
        data["Dot_DoorOpenDelayMax"] = doorOpenDelayRange.y.ToString();
        data["Dot_FloorIntervalMin"] = floorIntervalRange.x.ToString();
        data["Dot_FloorIntervalMax"] = floorIntervalRange.y.ToString();
        data["Dot_GuardReaction"] = guardReaction.ToString();
        data["Dot_DoorCloseDelayMin"] = doorCloseDelayRange.x.ToString();
        data["Dot_DoorCloseDelayMax"] = doorCloseDelayRange.y.ToString();
        data["Dot_ImageSet"] = imageSet;
        data["Dot_NeutralProb"] = neutralProb.ToString();
        data["Dot_BreakTrials"] = breakTrials.ToString();
        data["Dot_BreakDuration"] = breakDuration.ToString();
        data["Dot_HasPractice"] = hasPractice ? "1" : "0";
        data["Dot_PracticeName"] = practiceCsvFileName;
        data["Dot_ShowHp"] = willShowHp ? "1" : "0";
        data["Dot_ShowRedScreen"] = willShowRedScreen ? "1" : "0";
        data["Dot_ShowEva"] = willShowEva ? "1" : "0";
        data["Dot_PerfectThreshold"] = perfectThreshold.ToString();
        data["Dot_GreatThreshold"] = greatThreshold.ToString();
        data["Dot_HavePre"] = havePre ? "1" : "0";
        data["Dot_HavePost"] = havePost ? "1" : "0";

        SaveAllSettings(data);
    }

    // -------------------- 加载 DotProbe 设置 --------------------
    public static void LoadDotProbeSettings(DotProbeTask dotProbe)
    {
        Dictionary<string, string> data = LoadAllSettings();

        dotProbe.loopCount = GetInt(data, "Dot_Floors", dotProbe.floors);
        dotProbe.doorOpenDelayRange = new Vector2(
            GetFloat(data, "Dot_DoorOpenDelayMin", dotProbe.doorOpenDelayRange.x),
            GetFloat(data, "Dot_DoorOpenDelayMax", dotProbe.doorOpenDelayRange.y)
        );
        dotProbe.floorIntervalRange = new Vector2(
            GetFloat(data, "Dot_FloorIntervalMin", dotProbe.floorIntervalRange.x),
            GetFloat(data, "Dot_FloorIntervalMax", dotProbe.floorIntervalRange.y)
        );
        dotProbe.guardReactionTime = GetFloat(data, "Dot_GuardReaction", dotProbe.guardReactionTime);
        dotProbe.doorCloseDelayRange = new Vector2(
            GetFloat(data, "Dot_DoorCloseDelayMin", dotProbe.doorCloseDelayRange.x),
            GetFloat(data, "Dot_DoorCloseDelayMax", dotProbe.doorCloseDelayRange.y)
        );
        dotProbe.inputCsvFileName = GetString(data, "Dot_ImageSet", dotProbe.inputCsvFileName);
        dotProbe.trialsPerBreak = GetInt(data, "Dot_BreakTrials", dotProbe.trialsPerBreak);
        dotProbe.breakDuration = GetFloat(data, "Dot_BreakDuration", dotProbe.breakDuration);
        dotProbe.hasPractice = GetInt(data, "Dot_HasPractice", dotProbe.hasPractice ? 1 : 0) == 1;
        dotProbe.practiceCsvFileName = GetString(data, "Dot_PracticeName", dotProbe.practiceCsvFileName);
        dotProbe.willShowHp = GetInt(data, "Dot_ShowHp", dotProbe.willShowHp ? 1 : 0) == 1;
        dotProbe.willShowRedScreen = GetInt(data, "Dot_ShowRedScreen", dotProbe.willShowRedScreen ? 1 : 0) == 1;
        dotProbe.willShowEva = GetInt(data, "Dot_ShowEva", dotProbe.willShowEva ? 1 : 0) == 1;

        dotProbe.perfectThreshold = GetFloat(data, "Dot_PerfectThreshold", dotProbe.perfectThreshold);
        dotProbe.greatThreshold = GetFloat(data, "Dot_GreatThreshold", dotProbe.greatThreshold);
        dotProbe.havePre =
    GetInt(data, "Dot_HavePre", dotProbe.havePre ? 1 : 0) == 1;

        dotProbe.havePost =
            GetInt(data, "Dot_HavePost", dotProbe.havePost ? 1 : 0) == 1;

    }

    // -------------------- VisualSearch --------------------
    public static void SaveVisualSearchSettings(VisualSearchExperiment vs)
    {
        Dictionary<string, string> data = LoadAllSettings();

        data["VS_IntervalMin"] = vs.intervalTimeRange.x.ToString();
        data["VS_IntervalMax"] = vs.intervalTimeRange.y.ToString();
        data["VS_SelectionMin"] = vs.selectionTimeRange.x.ToString();
        data["VS_SelectionMax"] = vs.selectionTimeRange.y.ToString();

        data["VS_LoopCount"] = vs.loopCount.ToString();

        data["VS_HasPractice"] = vs.hasPractice ? "1" : "0";
        data["VS_PracticeCsv"] = vs.practiceCsvFileName;
        data["VS_FormalCsv"] = vs.inputCsvFileName;

        data["VS_BreakTrials"] = vs.trialsPerBreak.ToString();
        data["VS_BreakDuration"] = vs.breakDuration.ToString();
        data["VS_ShowEva"] = vs.willShowEva ? "1" : "0";
        data["VS_PerfectThreshold"] = vs.perfectThreshold.ToString();
        data["VS_GreatThreshold"] = vs.greatThreshold.ToString();
        data["VS_TrueCount"] = vs.trueCount.ToString();
        data["VS_HaveTest"] = vs.haveTest ? "1" : "0";


        SaveAllSettings(data);
    }


    public static void LoadVisualSearchSettings(VisualSearchExperiment vs)
    {
        Dictionary<string, string> data = LoadAllSettings();

        vs.intervalTimeRange = new Vector2(
    GetFloat(data, "VS_IntervalMin", vs.intervalTimeRange.x),
    GetFloat(data, "VS_IntervalMax", vs.intervalTimeRange.y)
);

        vs.selectionTimeRange = new Vector2(
            GetFloat(data, "VS_SelectionMin", vs.selectionTimeRange.x),
            GetFloat(data, "VS_SelectionMax", vs.selectionTimeRange.y)
        );

        vs.loopCount = GetInt(data, "VS_LoopCount", vs.loopCount);

        vs.hasPractice = GetInt(data, "VS_HasPractice", vs.hasPractice ? 1 : 0) == 1;

        vs.practiceCsvFileName = GetString(data, "VS_PracticeCsv", vs.practiceCsvFileName);
        vs.inputCsvFileName = GetString(data, "VS_FormalCsv", vs.inputCsvFileName);

        vs.trialsPerBreak = GetInt(data, "VS_BreakTrials", vs.trialsPerBreak);
        vs.breakDuration = GetFloat(data, "VS_BreakDuration", vs.breakDuration);
        vs.willShowEva = GetInt(data, "VS_ShowEva", vs.willShowEva ? 1 : 0) == 1;
        vs.perfectThreshold = GetFloat(data, "VS_PerfectThreshold", vs.perfectThreshold);
        vs.greatThreshold = GetFloat(data, "VS_GreatThreshold", vs.greatThreshold);
        vs.trueCount = GetInt(data, "VS_TrueCount", vs.trueCount);
        vs.haveTest =
    GetInt(data, "VS_HaveTest", vs.haveTest ? 1 : 0) == 1;


    }


    // -------------------- 文件读写核心 --------------------
    private static Dictionary<string, string> LoadAllSettings()
    {
        EnsureSettingsFileExists();
        Dictionary<string, string> data = new Dictionary<string, string>();
        foreach (string line in File.ReadAllLines(settingsFilePath))
        {
            if (line.Contains("="))
            {
                var parts = line.Split('=');
                if (parts.Length >= 2)
                    data[parts[0].Trim()] = parts[1].Trim();
            }
        }
        return data;
    }

    private static void SaveAllSettings(Dictionary<string, string> data)
    {
        using (StreamWriter writer = new StreamWriter(settingsFilePath, false))
        {
            foreach (var kv in data)
                writer.WriteLine($"{kv.Key}={kv.Value}");
        }
        Debug.Log($"设置已保存到: {settingsFilePath}");
    }

    // -------------------- 工具方法 --------------------
    private static int GetInt(Dictionary<string, string> data, string key, int defaultValue)
        => data.ContainsKey(key) && int.TryParse(data[key], out var v) ? v : defaultValue;

    private static float GetFloat(Dictionary<string, string> data, string key, float defaultValue)
        => data.ContainsKey(key) && float.TryParse(data[key], out var v) ? v : defaultValue;

    private static string GetString(Dictionary<string, string> data, string key, string defaultValue)
        => data.ContainsKey(key) ? data[key] : defaultValue;
}
