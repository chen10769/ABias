using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;

public class VisualSearchExperiment : MonoBehaviour
{
    [Header("加载设置")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public Text progressText;
    public bool isLoadingComplete = false;

    [Header("UI References")]
    public Button startButton;
    public Image[] imageDisplays; // 0左上 1右上 2左下 3右下
    public GameObject imagePanel;
    public Text countdownText;

    [Header("Experiment Settings")]
    public Vector2 intervalTimeRange = new Vector2(0.4f, 0.6f);
    public Vector2 selectionTimeRange = new Vector2(2.5f, 3.5f);
    private float currentIntervalTime;
    private float currentSelectionTime;
    [Header("CSV Settings")]
    public string practiceCsv = "VisualSearch_Practice.csv";
    public string formalCsv = "VisualSearch_Formal.csv";
    public int loopCount = 1;

    [Header("练习阶段设置")]
    public bool hasPractice = true;
    public float practicePassRate = 0.5f;



    [Header("休息设置")]
    public int trialsPerBreak = 1;
    public float breakDuration = 3f;



    [Header("评价设置")]
    public float perfectThreshold = 0.5f;
    public float greatThreshold = 1.0f;
    // public float goodThreshold = 1.5f;
    // public float badThreshold = 2.5f;
    public Text evaluationText;
    public Animator evaluationAni;

    private class TrialData
    {
        public int trialIndex;
        // —— 用于加载图片（已去掉前缀）——
        public string lt, rt, lb, rb;

        // —— 用于导出 CSV（保留原始前缀）——
        public string ltRaw, rtRaw, lbRaw, rbRaw;
        public string type1, type2, type3;
        public int correctIndex;

        public double reactionTime;
        public int actualResponse;
        public bool isCorrect;
    }

    private List<TrialData> trialSequence = new List<TrialData>();
    private List<TrialData> experimentData = new List<TrialData>();
    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();

    private bool isExperimentRunning = false;
    private bool isPracticePhase = false;
    private bool isSelectionActive = false;
    private bool practiceFinished = false;
    private bool practicePassed = false;

    private int currentTrial = 0;
    private float selectionStartTime;
    public bool willShowEva = true;

    public GameObject tip0;
    public GameObject tip;
    string fileName;

    void Start()
    {
        SettingsManager.LoadVisualSearchSettings(this);

        //HideAllImages();
        countdownText.gameObject.SetActive(false);
    }
    public void ShowTip()
    {
        if (GameState.instance.gameStateName == GameStateName.beforeV1 || GameState.instance.gameStateName == GameStateName.beforeV2)
        { tip0.SetActive(true); }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isExperimentRunning && tip0.activeInHierarchy)
        {
            tip.SetActive(true);
            tip0.SetActive(false);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isExperimentRunning && tip.activeInHierarchy)
            {
                StartExperiment(hasPractice);
            }

        }

        if (practiceFinished && Input.GetKeyDown(KeyCode.Space))
        {
            UIManager.instance.practiceResultPanel.SetActive(false);
            practiceFinished = false;

            if (practicePassed)
                StartExperiment(false);
            else
                StartExperiment(true);
        }

        if (isSelectionActive)
        {
            float timeLeft = currentSelectionTime - (Time.time - selectionStartTime);
            countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
            if (timeLeft <= 0)
            {
                RecordSelection(-1);
            }
        }
    }

    public void StartExperiment(bool practice)
    {
        if (isExperimentRunning) return;
        HideAllImages();
        isPracticePhase = practice;
        fileName = practice ? practiceCsv : formalCsv;
        StartCoroutine(LoadExperiment(fileName));
        tip.SetActive(false);
        FirstPersonController.instance.UnlockCursor();
    }

    IEnumerator LoadExperiment(string csvName)
    {
        isExperimentRunning = true;
        trialSequence.Clear();
        experimentData.Clear();

        LoadCsv(csvName);
        if (isExperimentRunning == false)
        {
            yield break;
        }
        loadingPanel.SetActive(true);
        yield return StartCoroutine(LoadImagesAsync());
        loadingPanel.SetActive(false);

        // ⭐ 打乱试次顺序（在正式开始前）
        ShuffleTrials(trialSequence);

        imagePanel.SetActive(true);
        currentTrial = 0;

        yield return StartCoroutine(RunExperiment());
    }

    void LoadCsv(string csvName)
    {
        string path = Path.Combine(Application.dataPath, "../input", csvName + ".csv");

        try
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8).Skip(1);

            foreach (var line in lines)
            {
                var c = line.Split(',');

                TrialData t = new TrialData
                {
                    ltRaw = c[0],
                    rtRaw = c[1],
                    lbRaw = c[2],
                    rbRaw = c[3],

                    lt = CleanPath(c[0]),
                    rt = CleanPath(c[1]),
                    lb = CleanPath(c[2]),
                    rb = CleanPath(c[3]),

                    type1 = c[4],
                    type2 = c[5],
                    type3 = c[6],
                    correctIndex = int.Parse(c[7])
                };

                trialSequence.Add(t);
            }

            // ⭐ 只有正式实验才循环表格
            if (!isPracticePhase)
            {
                var origin = new List<TrialData>(trialSequence);
                for (int i = 1; i < loopCount; i++)
                    trialSequence.AddRange(origin);
            }
        }
        catch (IOException e)
        {
            bool isSharingViolation = false;

            // Windows 下：32 = ERROR_SHARING_VIOLATION, 33 = ERROR_LOCK_VIOLATION
            int hResult = e.HResult & 0xFFFF;
            if (hResult == 32 || hResult == 33)
            {
                isSharingViolation = true;
            }

            if (isSharingViolation)
            {
                Debug.LogError("表格被占用，请关闭 Excel / WPS 后重试！");
                TipManager.instance.ToShowTip("表格被占用，请关闭 Excel / WPS 后重试！");
            }
            else
            {
                Debug.LogError("读取表格失败（非占用错误）");
                Debug.LogError(e);
                TipManager.instance.ToShowTip("读取表格失败，请检查文件是否损坏或路径是否正确");
            }

            // 防止实验继续跑
            isExperimentRunning = false;
            return;
        }

    }


    string CleanPath(string raw)
    {
        int idx = raw.IndexOf(')');
        return idx >= 0 ? raw.Substring(idx + 1) : raw;
    }

    IEnumerator LoadImagesAsync()
    {
        imageCache.Clear();

        var paths = trialSequence
            .SelectMany(t => new[] { t.lt, t.rt, t.lb, t.rb })
            .Distinct();

        int loaded = 0;
        int total = paths.Count();

        foreach (var p in paths)
        {
            string full = Path.Combine(Application.streamingAssetsPath, "TestImg", p);
            imageCache[p] = LoadSpriteFromFile(full);

            loaded++;
            float progress = (float)loaded / total;
            progressBar.value = progress;
            progressText.text = $"{(int)(progress * 100)}%";
            yield return null;
        }

        isLoadingComplete = true;
    }

    Sprite LoadSpriteFromFile(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }
    void ShuffleTrials(List<TrialData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }


    IEnumerator RunExperiment()
    {
        for (int i = 0; i < trialSequence.Count; i++)
        {
            if (i > 0 && trialsPerBreak > 0 && i % trialsPerBreak == 0)
                yield return StartCoroutine(StartBreak());

            currentTrial = i + 1;
            currentIntervalTime = Random.Range(intervalTimeRange.x, intervalTimeRange.y);
            yield return new WaitForSeconds(currentIntervalTime);


            SetupTrialImages(trialSequence[i]);
            StartSelectionPhase();

            while (isSelectionActive)
                yield return null;
        }

        EndExperiment();
    }

    IEnumerator StartBreak()
    {
        UIManager.instance.breakPanel.SetActive(true);
        float timer = breakDuration;

        while (timer > 0)
        {
            UIManager.instance.breakText.text = $"休息剩余 {Mathf.CeilToInt(timer)} 秒";
            timer -= Time.deltaTime;
            yield return null;
        }

        UIManager.instance.breakText.text = "按<color=red> 空格 </color>继续";
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        UIManager.instance.breakPanel.SetActive(false);
    }

    void SetupTrialImages(TrialData t)
    {
        imageDisplays[0].sprite = imageCache[t.lt];
        imageDisplays[1].sprite = imageCache[t.rt];
        imageDisplays[2].sprite = imageCache[t.lb];
        imageDisplays[3].sprite = imageCache[t.rb];

        foreach (var img in imageDisplays)
            img.gameObject.SetActive(true);
    }

    void StartSelectionPhase()
    {
        isSelectionActive = true;
        currentSelectionTime = Random.Range(selectionTimeRange.x, selectionTimeRange.y);
        selectionStartTime = Time.time;

        countdownText.gameObject.SetActive(true);
        UIManager.instance.crossHairAni.gameObject.SetActive(false);
    }

    public void OnImageSelected(int index)
    {
        if (!isSelectionActive) return;
        if (PersistentObject.instance.isSettingMode) return;
        RecordSelection(index);
    }

    void RecordSelection(int index)
    {
        isSelectionActive = false;
        countdownText.gameObject.SetActive(false);
        UIManager.instance.crossHairAni.gameObject.SetActive(true);

        TrialData t = trialSequence[currentTrial - 1];
        t.trialIndex = currentTrial;
        t.actualResponse = index + 1;
        t.reactionTime = index < 0 ? -1 : Time.time - selectionStartTime;
        t.isCorrect = t.actualResponse == t.correctIndex;

        experimentData.Add(t);
        HideAllImages();
        if (willShowEva == false && !isPracticePhase)
        {
            return;
        }
        evaluationText.text = GetEvaluation(t.reactionTime);




    }

    string GetEvaluation(double reactionTime)
    {

        evaluationAni.SetTrigger("play");

        if (trialSequence[currentTrial - 1].isCorrect)
        {
            if (reactionTime <= perfectThreshold)
            {
                evaluationText.color = new Color(1f, 0.92f, 0.16f);
                return "Perfect";
            }
            else if (reactionTime <= greatThreshold)
            {
                evaluationText.color = new Color(1f, 0.5f, 0f);
                return "Great";
            }
            else
            {
                evaluationText.color = Color.green;
                return "Good";
            }
            // else if (reactionTime <= badThreshold)
            // {
            //     evaluationText.color = Color.blue;
            //     return "Bad";
            // }
            // else
            // {
            //     evaluationText.color = Color.red;
            //     return "Poor";
            // }
        }
        else
        {
            evaluationText.color = Color.red;
            return "Error";
        }




    }

    void HideAllImages()
    {
        foreach (var img in imageDisplays)
            img.gameObject.SetActive(false);
    }

    void EndExperiment()
    {
        isExperimentRunning = false;
        imagePanel.SetActive(false);
        DisplayResults();
        if (isPracticePhase && hasPractice)
        {
            float acc = 100f * experimentData.Count(d => d.isCorrect) / (float)experimentData.Count;

            practicePassed = acc >= practicePassRate;
            practiceFinished = true;

            UIManager.instance.practiceResultPanel.SetActive(true);
            UIManager.instance.practiceResultText.text = practicePassed
                ? "按<color=red> 空格 </color>键进入正式实验"
                : $"练习未通过（正确率 {(acc):F1}%）\n请按<color=red> 空格 </color>键重新开始练习";
        }
        else
        {
            Invoke("MissonComplete", 1f);
        }
    }
    public void MissonComplete()
    {
        UIManager.instance.companel.SetActive(true);
        FirstPersonController.instance.UnlockCursor();
        //TipManager.instance.ToShowTip("请前往办公室");
    }

    void DisplayResults()
    {
        float acc = 100f * experimentData.Count(d => d.isCorrect) / (float)experimentData.Count;

        List<string> csv = new List<string>();
        csv.Add("左上图,右上图,左下图,右下图,性别,图片,情绪面孔位置,正确面孔位置,反应时间,实际反应,正确与否,总体正确率,学号,电话号码,组别");

        for (int i = 0; i < experimentData.Count; i++)
        {
            var d = experimentData[i];

            // ⭐ 只在第一行写被试信息
            string accStr = i == 0 ? acc.ToString("F1") + "%" : "";
            string idStr = i == 0 ? PersistentObject.instance.studentId : "";
            string phoneStr = i == 0 ? PersistentObject.instance.phoneNumber : "";
            string groupStr = i == 0 ? PersistentObject.instance.group : "";

            csv.Add(
                $"{d.ltRaw},{d.rtRaw},{d.lbRaw},{d.rbRaw}," +
                $"{d.type1},{d.type2},{d.type3},{d.correctIndex}," +
                $"{d.reactionTime:F15},{d.actualResponse},{(d.isCorrect ? 1 : 0)}," +
                $"{accStr},=\"{idStr}\",=\"{phoneStr}\",=\"{groupStr}\""
            );
        }

        SaveCsvFile(csv);
    }

    void SaveCsvFile(List<string> csvLines)
    {
        string folder = Path.Combine(Application.dataPath, "../ExperimentResults");
        Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, $"{PersistentObject.instance.studentId}_{PersistentObject.instance.phoneNumber}_{PersistentObject.instance.group}_{fileName}_VisualSearch.csv");

        File.WriteAllLines(file, csvLines, Encoding.UTF8);
    }
}
