using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class BaseExperiment : MonoBehaviour
{
    [Header("加载设置")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public Text progressText;
    protected bool isLoadingComplete = false;

    [Header("实验状态")]
    protected bool isExperimentRunning = false;
    protected bool isPracticePhase = false;
    protected bool practiceFinished = false;
    protected bool practicePassed = false;
    [Header("练习阶段设置")]
    public bool hasPractice = true;               // 是否有练习阶段
    public bool hadDoPractice = false;               
    public int practiceLoopCount = 1;

    [Header("休息设置")]
    public int trialsPerBreak = 0;
    public float breakDuration = 0f;
    [Header("CSV 文件名设置")]
    public string inputCsvFileName;     
    public string practiceCsvFileName;  
    protected string fileName; 

    [Header("评价反馈设置")]
    public float perfectThreshold;
    public float greatThreshold;
    public Text evaluationText;
    public Animator evaluationAni;
    public int combo = 0;
    string evaluationContent;
    int baseScore = 10;
    float comboStep = 0.05f; 
    protected float reactionMultiplier = 1f;


    public void StartExperiment(bool practice)
    {
        if (isExperimentRunning) return;
        combo = 0;
        isPracticePhase = practice;
        isExperimentRunning = true;
        PrepareExperiment();
        StartCoroutine(LoadAndRun());
        ItemGet.instance.evidenceListPanel.SetActive(false);
    }
    protected abstract void PrepareExperiment();  
    IEnumerator LoadAndRun()
    {
        ShowLoading(true);
        yield return StartCoroutine(LoadImagesAsync());
        ShowLoading(false);
        OnExperimentStart();
    }

    protected void ShowLoading(bool show)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
    }
    protected abstract void OnExperimentStart();      

    protected string GetEvaluation(double reactionTime, bool isCorrect)
    {
        //evaluationAni?.SetTrigger("play");
        if (!isCorrect)
        {
            evaluationText.color = Color.red;
            evaluationContent = "Error";
            reactionMultiplier = 0f;
            combo = 0;
            return evaluationContent;
        }
        else
        {
            combo++;
            if (reactionTime <= perfectThreshold)
            {
                evaluationText.color = new Color(1f, 0.92f, 0.16f);
                evaluationContent = "Perfect";
                reactionMultiplier = 1.3f;
            }
            else if (reactionTime <= greatThreshold)
            {
                evaluationText.color = new Color(1f, 0.5f, 0f);
                evaluationContent = "Great";
                reactionMultiplier = 1.1f;
            }
            else
            {
                evaluationText.color = Color.green;
                evaluationContent = "Good";
                reactionMultiplier = 1.0f;
            }
            // 连击倍率
            float comboMultiplier = 1f + (combo - 1) * comboStep;

            // 最终得分
            int finalScore = Mathf.RoundToInt(
                baseScore * reactionMultiplier * comboMultiplier
            );
            Debug.Log(finalScore);
            ScoreManager.instance.score += finalScore;
            UIManager.instance.scoreText.text = "您的分数为："+ScoreManager.instance.score.ToString();
            if (combo > 1)
            {
                evaluationContent += $"\n{combo}连击";
            }
            return evaluationContent;
        }
    }

    protected void ShowEvaluation(string evaluation)
    {
        evaluationText.text = evaluation;
        evaluationAni.SetTrigger("play");
    }

    protected virtual IEnumerator StartBreak()
    {
        UIManager.instance.breakPanel.SetActive(true);
        float timer = breakDuration;

        while (timer > 0)
        {
            UIManager.instance.breakText.text =
                $"休息剩余 {Mathf.CeilToInt(timer)} 秒";
            timer -= Time.deltaTime;
            yield return null;
        }

        UIManager.instance.breakText.text = "按<color=red> 空格 </color>继续";
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        UIManager.instance.breakPanel.SetActive(false);
    }

    protected IEnumerator LoadImagesAsyncCommon(IEnumerable<string> paths, System.Action<string, Texture2D> onLoadTex = null, System.Action<string, Sprite> onLoadSprite = null)
    {
        isLoadingComplete = false;
        progressBar.value = 0;
        progressText.text = "0%";

        var pathList = paths.ToList();
        int total = pathList.Count;
        int loaded = 0;

        foreach (var relPath in pathList)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, "TestImg", relPath);
            Debug.Log(fullPath);
            byte[] data = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);

            onLoadTex?.Invoke(relPath, tex); 
            if (onLoadSprite != null)
            {
                Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                onLoadSprite.Invoke(relPath, sp); 
            }

            loaded++;
            float progress = (float)loaded / Mathf.Max(1, total);
            progressBar.value = progress;
            progressText.text = $"加载中：{(int)(progress * 100)}%";

            yield return null;
        }

        isLoadingComplete = true;
    }

    protected abstract IEnumerator LoadImagesAsync(); 

    protected virtual bool CheckPracticePassed()
    {
        return true;
    }

    protected virtual void EndExperiment()
    {
        FirstPersonController.instance.UnlockCursor();
        GetResults();
        isExperimentRunning = false;
        if (evaluationText != null)
        {
            evaluationText.text = "";
        }
        if (evaluationAni != null)
        {
            evaluationAni.ResetTrigger("play");
        }

        // 练习阶段逻辑
        if (isPracticePhase && hasPractice)
        {
            practiceFinished = true;
            practicePassed = CheckPracticePassed();
            hadDoPractice = true;
            AfterPratice();
            if (UIManager.instance != null)
            {
                UIManager.instance.practiceResultPanel.SetActive(true);
                UIManager.instance.practiceResultText.text = practicePassed
                    ? "按<color=red> 空格 </color>键进入正式实验"
                    : $"练习未通过\n请按<color=red> 空格 </color>键重新开始练习";
            }
        }
        else
        {
            AfterExperiment();

        }
    }
    protected void SaveCsvFile(
        List<string> csvLines,
        string experimentTag,         
        Encoding encoding               
    )
    {
        string folderPath = Path.Combine(Application.dataPath, "../ExperimentResults");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string studentId = PersistentObject.instance.studentId;
        string phone = PersistentObject.instance.phoneNumber;
        string group = PersistentObject.instance.group;

        string filePath = Path.Combine(
            folderPath,
            $"{studentId}_{phone}_{group}_{fileName}_{experimentTag}_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv"
        );

        try
        {
            File.WriteAllLines(filePath, csvLines, encoding);
            Debug.Log($"结果已保存至: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("保存CSV文件时出错: " + e.Message);
        }
    }
    protected void GetResultsCommon<T>(
    List<T> dataList,
    string headerLine,
    System.Func<T, string> buildDataColumns, // 原始数据列
    System.Func<T, string> buildResultColumns, 
    System.Func<T, bool> isCorrectFunc,
    string resultTag,
    Encoding encoding
)
    {
        List<string> csvLines = new List<string>();

        // 表头
        csvLines.Add(headerLine + ",反应时间,实际反应,正确与否,总体正确率,学号,电话号码,组别");

        float accuracy = dataList.Count > 0
    ? 100f * dataList.Count(d => isCorrectFunc(d)) / dataList.Count
    : 0f;


        for (int i = 0; i < dataList.Count; i++)
        {
            var d = dataList[i];

            string accStr = i == 0 ? accuracy.ToString("F1") + "%" : "";
            string idStr = i == 0 ? PersistentObject.instance.studentId : "";
            string phoneStr = i == 0 ? PersistentObject.instance.phoneNumber : "";
            string groupStr = i == 0 ? PersistentObject.instance.group : "";

            csvLines.Add(
                $"{buildDataColumns(d)}," +
                $"{buildResultColumns(d)}," +
                $"{accStr},=\"{idStr}\",=\"{phoneStr}\",{groupStr}"
            );
        }

        SaveCsvFile(csvLines, resultTag, encoding);
    }
    protected bool LoadCsvCommon<T>(
            string csvPath,
            Encoding encoding,
            bool skipHeader,
            bool enableLoop,
            int loopCount,
            bool shuffleOnce,
            bool shuffleEachLoop,
            System.Func<List<string>, T> parseRow, 
            out List<T> trialSequence
        )
    {
        trialSequence = new List<T>();

        List<string> lines;
        try
        {
            lines = File.ReadAllLines(csvPath, encoding)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();
        }
        catch (IOException e)
        {
            TipManager.instance?.ToShowTip("表格被占用，请关闭 Excel / WPS 后重试！");
            Debug.LogError(e);
            return false;
        }

        if (lines.Count == 0) return false;

        int start = skipHeader ? 1 : 0;

        var rows = new List<T>();
        for (int i = start; i < lines.Count; i++)
        {
            var cols = lines[i].Split(',').Select(s => s.Trim()).ToList();
            var data = parseRow(cols);
            if (data != null)
                rows.Add(data);
        }

        if (rows.Count == 0) return false;

        // ===== 随机策略统一 =====
        if (shuffleOnce)
            rows = rows.OrderBy(_ => Random.value).ToList();

        if (enableLoop && loopCount > 1 && !isPracticePhase)
        {
            for (int i = 0; i < loopCount; i++)
            {
                var batch = shuffleEachLoop
                    ? rows.OrderBy(_ => Random.value).ToList()
                    : new List<T>(rows);

                trialSequence.AddRange(batch);
            }
        }
        else
        {
            trialSequence.AddRange(rows);
        }

        return true;
    }
    protected string NormalizeRelativePath(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        string s = raw.Replace('\\', '/').Trim();

        if (s.StartsWith("TestImg/"))
            s = s.Substring("TestImg/".Length);

        return s;
    }
    protected string RemoveBeforeFirstOccurrence(string fullPath, string target)
    {
        if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(target))
            return fullPath;

        int idx = fullPath.IndexOf(target);
        if (idx >= 0)
        {
            return fullPath.Substring(idx); 
        }
        return fullPath;
    }
    public static string RemoveFirstParenthesesContent(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        int start = input.IndexOf('(');
        if (start == -1) return input; 

        int count = 0;
        for (int i = start; i < input.Length; i++)
        {
            if (input[i] == '(')
                count++;
            else if (input[i] == ')')
            {
                count--;
                if (count == 0)
                {
                    // 找到匹配的右括号，移除从 start 到 i 的部分
                    return input.Substring(0, start) + input.Substring(i + 1);
                }
            }
        }

        // 没有找到匹配的右括号，返回原字符串
        return input;
    }

    protected abstract void GetResults();
    protected abstract void AfterExperiment();
    protected abstract void AfterPratice();

}
