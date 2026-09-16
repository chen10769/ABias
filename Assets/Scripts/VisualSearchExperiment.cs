using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using Fungus;

public class VisualSearchExperiment : BaseExperiment
{


    [Header("UI References")]

    public Image[] imageDisplays; // 0左上 1右上 2左下 3右下
    public GameObject imagePanel;
    public Text countdownText;

    [Header("Experiment Settings")]
    public Vector2 intervalTimeRange = new Vector2(0.4f, 0.6f);
    public Vector2 selectionTimeRange = new Vector2(2.5f, 3.5f);
    private float currentIntervalTime;
    private float currentSelectionTime;

    public int loopCount = 1;
        public bool haveTest=true;



    private class TrialData
    {
        public int trialIndex;
        public string lt, rt, lb, rb;
        public string ltRaw, rtRaw, lbRaw, rbRaw;
        public string type1, type2, type3;
        public int correctIndex;

        public double reactionTime;
        public string actualResponse;
        public bool isCorrect;
    }

    private List<TrialData> trialSequence = new List<TrialData>();
    private List<TrialData> experimentData = new List<TrialData>();
    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();


    private bool isSelectionActive = false;


    private int currentTrial = 0;
    private float selectionStartTime;
    public bool willShowEva = true;

    public GameObject tip0;
    public GameObject tip;
    public static VisualSearchExperiment instance;

    public int totalCount = 5;  
    public int trueCount = 2;    
    public int curCount = 0;    
    private List<bool> resultPool;
    private bool initialized = false;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        SettingsManager.LoadVisualSearchSettings(this);
        countdownText.gameObject.SetActive(false);
    }
    public void ToStart()
    {
        tip.SetActive(false);
        if (!hasPractice || (hasPractice && hadDoPractice))
        {
            StartExperiment(false);
        }
        else
        {
            StartExperiment(true);
        }

    }
    public void ShowTip()
    {
        if (PersistentObject.instance != null && PersistentObject.instance.isSettingMode)
            return;
        if (haveTest)
        {
             SmokeAppear();
        }
        //tip.SetActive(true);
           
    }
    public void SmokeAppear()
    {
        if (GameState.instance.gameStateName != GameStateName.D2)
        {
            UIManager.instance.EvidenceListPanel.SetActive(false);
            RuntimeDialogueLoader.instance.LoadAndExecuteDialogue("提示语/VisualSearchExperiment", "VisualSearchExperimentTip");
            //curCount++;
        }
    }
    protected override void PrepareExperiment()
    {
        HideAllImages();
        fileName = isPracticePhase ? practiceCsvFileName : inputCsvFileName;
        bool ok = LoadCsv();
        if (!ok)
        {
            Debug.LogWarning("无法读取输入 CSV，请检查输入文件或模板。");
            return;
        }
        StartCoroutine(LoadExperiment());
        tip.SetActive(false);
      
        trialSequence.Clear();
        experimentData.Clear();
        LoadCsv();
    }

    protected override void OnExperimentStart()
    {
        imagePanel.SetActive(true);
        StartCoroutine(RunExperiment());
    }

    void Update()
    {
          if (isExperimentRunning)
        {
             if (Input.GetKeyDown(KeyCode.E))
            {
                OnImageSelected(0);
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                OnImageSelected(1);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                OnImageSelected(2);
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                OnImageSelected(3);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (tip.activeInHierarchy)
            {
                ToStart();
            }
        }
        if (practiceFinished && Input.GetKeyDown(KeyCode.Space) && UIManager.instance.practiceResultPanel.activeInHierarchy)
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



    IEnumerator LoadExperiment()
    {
        isExperimentRunning = true;
        trialSequence.Clear();
        experimentData.Clear();
        if (isExperimentRunning == false)
        {
            yield break;
        }

        imagePanel.SetActive(true);
        currentTrial = 0;
    }

    protected bool LoadCsv()
    {
        string csvPath = Path.Combine(Application.dataPath, "../input", fileName + ".csv");

        return LoadCsvCommon(
            csvPath,
            Encoding.UTF8,
            skipHeader: true,
            enableLoop: true,
            loopCount: loopCount,
            shuffleOnce: true,
            shuffleEachLoop: true,
            parseRow: cols =>
            {
                if (cols.Count < 8) return null;

                return new TrialData
                {
                    ltRaw = cols[0],
                    rtRaw = cols[1],
                    lbRaw = cols[2],
                    rbRaw = cols[3],

                    lt = RemoveFirstParenthesesContent(cols[0]),
                    rt = RemoveFirstParenthesesContent(cols[1]),
                    lb = RemoveFirstParenthesesContent(cols[2]),
                    rb = RemoveFirstParenthesesContent(cols[3]),

                    type1 = cols[4],
                    type2 = cols[5],
                    type3 = cols[6],
                    correctIndex = int.Parse(cols[7])
                };
            },
            out trialSequence
        );
    }




    protected override IEnumerator LoadImagesAsync()
    {
        var paths = trialSequence
            .SelectMany(t => new[] { t.lt, t.rt, t.lb, t.rb })
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct();

        yield return StartCoroutine(LoadImagesAsyncCommon(paths,
            onLoadSprite: (relPath, sp) => imageCache[relPath] = sp
        ));
    }
     protected override bool CheckPracticePassed()
    {
        float correctRate = experimentData.Count > 0
            ? experimentData.Count(d => d.isCorrect) / (float)experimentData.Count
            : 0f;
        return correctRate >= 0.9f;
    }




    IEnumerator RunExperiment()
    {
        Debug.Log("RunExperiment");
        LockCursor();
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
    protected override IEnumerator StartBreak()
    {
        imagePanel.SetActive(false);
        DialogManager.instance?.ShowDialog("冷静");
        float timer = breakDuration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        UIManager.instance.breakText.text = "按<color=red> 空格 </color>继续";
        if (GameState.instance.gameStateName != GameStateName.D2)
        {
            UIManager.instance.EvidenceListPanel.SetActive(false);
            RuntimeDialogueLoader.instance.LoadAndExecuteDialogue("提示语/VisualSearchExperiment", "VisualSearchExperimentTip2");

        }
        // 等待 breakPanel 被激活
        while (!UIManager.instance.breakPanel.activeInHierarchy)
            yield return null;
        // 等待玩家按空格继续
        while (UIManager.instance.breakPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                break;

            yield return null;
        }

        UIManager.instance.breakPanel.SetActive(false);
        imagePanel.SetActive(true);
    }
    public void LockCursor()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;



    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }



    void SetupTrialImages(TrialData t)
    {
        
        //UnlockCursor();
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
        LockCursor();
    }

    void RecordSelection(int index)
    {
        isSelectionActive = false;
        countdownText.gameObject.SetActive(false);
        UIManager.instance.crossHairAni.gameObject.SetActive(true);

        TrialData src = trialSequence[currentTrial - 1];

        TrialData result = new TrialData
        {
            trialIndex = currentTrial,

            ltRaw = src.ltRaw,
            rtRaw = src.rtRaw,
            lbRaw = src.lbRaw,
            rbRaw = src.rbRaw,

            lt = src.lt,
            rt = src.rt,
            lb = src.lb,
            rb = src.rb,

            type1 = src.type1,
            type2 = src.type2,
            type3 = src.type3,
            correctIndex = src.correctIndex,

            actualResponse = (index + 1).ToString(),
            reactionTime = index < 0 ? -1 : Time.time - selectionStartTime,
            isCorrect = index + 1 == src.correctIndex
        };

        experimentData.Add(result);
        HideAllImages();
        if (willShowEva == false && !isPracticePhase)
        {
            return;
        }
        if (string.IsNullOrEmpty(result.actualResponse))
        {
            result.actualResponse = "超时";
            result.reactionTime = -1;
            result.isCorrect = false;
            evaluationText.color = Color.red;
            combo = 0;
            ShowEvaluation("超时");
            reactionMultiplier = 0f;
        }
        else
        {
             string evaluation = GetEvaluation(result.reactionTime, result.isCorrect);
            evaluationText.text = evaluation;
            ShowEvaluation(evaluation);
        }
        
    }



    void HideAllImages()
    {
        foreach (var img in imageDisplays)
            img.gameObject.SetActive(false);
    }

    protected override void AfterExperiment()
    {
        imagePanel.SetActive(false);
        DialogManager.instance?.ShowDialog("冷静");
        //Invoke(nameof(SmokeAppear), breakDuration);
    }



    protected override void GetResults()
    {
        GetResultsCommon(
            experimentData,
            "左上图,右上图,左下图,右下图,性别,图片,情绪面孔位置,正确面孔位置",
            d => $"{d.ltRaw},{d.rtRaw},{d.lbRaw},{d.rbRaw},{d.type1},{d.type2},{d.type3},{d.correctIndex}",
            d => $"{d.reactionTime:F15},{d.actualResponse},{(d.isCorrect ? 1 : 0)}",
            d => d.isCorrect,
            "VisualSearch",
            Encoding.UTF8
        );
    }

    protected override void AfterPratice()
    {

    }

    private void Init()
    {
        resultPool = new List<bool>();

        for (int i = 0; i < trueCount; i++)
            resultPool.Add(true);

        for (int i = 0; i < totalCount - trueCount; i++)
            resultPool.Add(false);

        initialized = true;
    }


    public bool Trigger()
    {
        if (!initialized)
            Init();

        if (resultPool.Count == 0)
        {
            return false;
        }

        int index = Random.Range(0, resultPool.Count);
        bool result = resultPool[index];
        resultPool.RemoveAt(index);

        return result;
    }





}
