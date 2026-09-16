using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Text;
using Fungus;
using Unity.VisualScripting;


public class DotProbeTask : BaseExperiment
{

    [Header("加载设置")]

    [Header("实验设置")]
    public int floors = 5; //楼层数（实验次数） 
    public Vector2 doorOpenDelayRange = new Vector2(0.25f, 1f);//开门前的间隔 图片显示时间
    public Vector2 floorIntervalRange = new Vector2(0.25f, 1f);//到下一楼层的间隔时间
    public float guardReactionTime = 0.5f; //敌人发现玩家并开枪的反应时间
    public Vector2 doorCloseDelayRange = new Vector2(0.5f, 1.5f); //关门前间隔（新增范围）

    public bool willShowHp = true;
    public bool willShowRedScreen = true;
    public bool willShowEva = true;

    [Header("CSV / 输入设置")]
    public bool enableLoop = false; // 是否循环试次
    public int loopCount = 1;       // 循环次数（当 enableLoop 为 true 时）
    public string inputFolderName = "input";     // 相对 project root 的 input 文件夹
    public Text healthText;
    public GameObject healthPanel;


    [Header("游戏对象引用")]
    public Animator elevatorAnimator;
    public Animator guardAnimator;
    public Transform leftGuardPosition;
    public Transform rightGuardPosition;
    public Renderer leftBillboard;
    public Renderer rightBillboard;
    public string albedoProperty = "_MainTex";
    public string emissionProperty = "_EmissionMap";
    public GuardController guardController;
    public bool isDoorOpen = false;
    public GameObject bulletIcon;

    [Header("玩家设置")]
    public float maxHealth = 100f;
    public float damagePerSecond = 10f;

   
    private class FloorData
    {
        public int floorIndex;
        public double reactionTime;
        public string evaluation;

        public bool guardOnNeutralSide;
        public string neutralImage;
        public string threatImage;
        public string neutralSubtype;
        public string threatSubtype;

        public string leftImageType;   // 左边图片类型（中性/负面）
        public string rightImageType;  // 右边图片类型
        public string leftImageSubtype;  // 左边图片子类型（文件夹名，如悲伤）
        public string rightImageSubtype; // 右边图片子类型
        public string guardPosition;   // "左" 或 "右"
        public int consistency;        // 1=负面侧, 2=中性侧, 3=两边中性
        public string correctKey;      // "Q" 或 "E"
        public string actualKey;       // 玩家实际按键（第一次）
        public bool isCorrect;         // 是否正确
        public string leftImagePath;   // 左图相对路径（如 Test1\负面\1 (6).png）
        public string rightImagePath;  // 右图相对路径
        public List<string> originalColumns; // 原始 CSV 列（保持顺序）
    }

    // image 路径与纹理映射
    private Dictionary<string, string> imagePaths = new(); // name -> relativePath
    private Dictionary<string, Texture2D> imageByRelativePath = new(); // relativePath -> tex


    private List<FloorData> experimentData = new();
    private List<FloorData> trialSequence = new(); // 从 CSV 读取后构造的试次序列（含循环）
    public int currentFloor = 0;

    public bool isExperimentDone = false;
    //public bool isGuardShooting = false;
    private float playerHealth;
    private float guardAppearTime;
    private bool guardOnLeft;

    public static DotProbeTask instance;

    public GameObject tip;
    public GameObject beforeP;

    public bool havePre=true;

    public bool havePost=true;


    void Awake() => instance = this;

    void Start()
    {
        // 加载保存的设置
        SettingsManager.LoadDotProbeSettings(this);
        playerHealth = maxHealth;
        UpdateHealthDisplay();
        //floorText.text = "准备开始";
        evaluationText.text = "";
        // 在启动时检查 input 文件夹和模板
        //EnsureInputFolderAndTemplate();
    }

    public void ShowTip()
    {
        if (PersistentObject.instance != null && PersistentObject.instance.isSettingMode)
            return;

        RuntimeDialogueLoader.instance.LoadAndExecuteDialogue("提示语/DotProbeTask", "DotProbeTaskTip");
       
        
    }


    void Update()
    {
        if (PersistentObject.instance != null && PersistentObject.instance.isSettingMode)
            return;
        if (Input.GetKeyDown(KeyCode.Space) && !isExperimentRunning && (tip.activeInHierarchy || beforeP.activeInHierarchy))
        {
            if (!hasPractice||(hasPractice&&hadDoPractice))
            {
                // 开始正式实验
                StartExperiment(false);
            }
            else
            {
                if (tip.activeInHierarchy)
                {
                    beforeP.SetActive(true);
                }
                else
                {
                    if (beforeP.activeInHierarchy)
                    {
                        beforeP.SetActive(false);
                    }
                    StartExperiment(true);
                }
            }
            tip.SetActive(false);
        }

        if (isDoorOpen && experimentData.Count > 0)
        {
            var data = experimentData[currentFloor - 1];
            if (string.IsNullOrEmpty(data.actualKey))
            {
                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J))
                {
                    // 记录玩家实际按键
                    data.actualKey = Input.GetKeyDown(KeyCode.F) ? "F" : "J";
                    data.isCorrect = (data.actualKey == data.correctKey);

                    // 按键时就计算反应时间 
                    float reactionTime = Time.time - guardAppearTime;
                    data.reactionTime = reactionTime;
                    string evaluation = GetEvaluation(reactionTime, data.isCorrect);
                    data.evaluation = evaluation;
                    if (willShowEva && !isPracticePhase || isPracticePhase)
                    {
                        ShowEvaluation(evaluation);
                        
                    }
                    if (willShowHp)
                    {
                        healthPanel.gameObject.SetActive(true);
                    }


                }
            }
        }


        // 如果练习完成，监听空格
        if (practiceFinished && Input.GetKeyDown(KeyCode.Space))
        {
            UIManager.instance.practiceResultPanel.SetActive(false);
            practiceFinished = false;
            FirstPersonController.instance.LockCursor();
            if (practicePassed)
            {
                // 开始正式实验
                StartExperiment(false);
            }
            else
            {
                // 重新开始练习
                StartExperiment(true);
            }
        }
    }
    public void Damaged()
    {
        playerHealth -= damagePerSecond * Time.deltaTime;
        if (willShowRedScreen)
        {
            UIManager.instance.damageScreen.SetTrigger("damage");
        }
        UpdateHealthDisplay();
        if (playerHealth <= 0)
            GameOver();
    }

    private float GetRandomInRange(Vector2 range)
    {
        float num = Mathf.Approximately(range.x, range.y) ? range.x : Random.Range(range.x, range.y);
        return num;
    }
    public void ShootEnemy()
    {
        guardController.isDead = true;
        guardController.isShooting = false;

        guardController?.SetDead(true);
        guardController?.SetShooting(false);
        if (willShowHp)
        {
            healthPanel.gameObject.SetActive(true);
        }
        if (currentFloor == floors)
        {
            Invoke(nameof(EndExperiment), 0.3f);
        }
    }


    protected override void PrepareExperiment()
    {
        // 根据当前阶段设置文件名
        fileName = isPracticePhase ? practiceCsvFileName : inputCsvFileName;

        // 确保 input 文件夹和模板存在
        //EnsureInputFolderAndTemplate();

        bool ok = LoadCsv();
        if (!ok)
        {
            Debug.LogWarning("无法读取输入 CSV，请检查输入文件。");
            return;
        }
        // 显示加载面板
        ShowLoading(true);

        FirstPersonController.instance.LockCursor();
        FirstPersonController.instance.autoAim = true;
        FirstPersonController.instance.BackToCenter();
        FirstPersonController.instance.gameObject.transform.position = FirstPersonController.instance.startPos.position;
        guardController.interactArea.SetActive(false);

        // 开始异步加载图片
        leftBillboard.material.color = Color.black;
        rightBillboard.material.color = Color.black;
        leftBillboard.material.SetColor("_EmissionColor", Color.black);
        rightBillboard.material.SetColor("_EmissionColor", Color.black);
        StartCoroutine(LoadImagesAsync());
    }


    public void KaiGuanMen()
    {
        if (isDoorOpen)
        {
            elevatorAnimator.SetTrigger("Close");
            isDoorOpen = false;
        }
        else
        {
            elevatorAnimator.SetTrigger("Open");
            isDoorOpen = true;
        }

    }

    public IEnumerator NextFloor()
    {
        while (!isLoadingComplete)
            yield return null;
        // 判断是否要进入休息（跳过第0次）
        if (currentFloor > 0 && trialsPerBreak > 0 && currentFloor % trialsPerBreak == 0 && currentFloor < floors)
        {
            yield return StartCoroutine(StartBreak());
        }
        // currentFloor 指示已开始的层数，从 1 开始
        currentFloor++;
        // 获取当前试次数据（由 trialSequence 提供）
        if (currentFloor - 1 >= trialSequence.Count)
        {
            // 如果越界，直接结束
            EndExperiment();
            yield break;
        }
        FirstPersonController.instance.LockCursor();
        // 复制 trialSequence 的模板数据到 experimentData（以便后续记录 reaction 等）
        FloorData data = new FloorData();
        // 复制原始列
        data.originalColumns = trialSequence[currentFloor - 1].originalColumns != null ? new List<string>(trialSequence[currentFloor - 1].originalColumns) : new List<string>();
        data.leftImagePath = trialSequence[currentFloor - 1].leftImagePath;
        data.rightImagePath = trialSequence[currentFloor - 1].rightImagePath;
        experimentData.Add(data);
        yield return new WaitForSeconds(GetRandomInRange(floorIntervalRange));
        SetupBillboards(); // 将使用 experimentData[currentFloor-1] 中的 left/right path
        guardController.ResetSelf();
        yield return new WaitForSeconds(GetRandomInRange(doorOpenDelayRange));
        string csvProbePos = (trialSequence[currentFloor - 1].originalColumns != null && trialSequence[currentFloor - 1].originalColumns.Count >= 3) ? trialSequence[currentFloor - 1].originalColumns[2].Trim() : "";
        if (!string.IsNullOrEmpty(csvProbePos))
        {
            if (csvProbePos == "左" || csvProbePos.ToLower() == "left")
                guardOnLeft = true;
            else if (csvProbePos == "右" || csvProbePos.ToLower() == "right")
                guardOnLeft = false;
            else
            {
                guardOnLeft = Random.value > 0.5f;

            }
        }
        else
        {
            guardOnLeft = Random.value > 0.5f;

        }

        data.guardPosition = guardOnLeft ? "左" : "右";
        data.correctKey = guardOnLeft ? "F" : "J";
        data.actualKey = null;  // 还没按
        data.isCorrect = false;
        // 一致性判定
        if (data.leftImageType == "负面" && guardOnLeft) data.consistency = 1;
        else if (data.rightImageType == "负面" && !guardOnLeft) data.consistency = 1;
        else if (data.leftImageType == "中性" && data.rightImageType == "中性") data.consistency = 3;
        else data.consistency = 2;

        guardAnimator.transform.position = guardOnLeft ? leftGuardPosition.position : rightGuardPosition.position;
        guardAnimator.transform.rotation = guardOnLeft ? leftGuardPosition.rotation : rightGuardPosition.rotation;

        elevatorAnimator.SetTrigger("Open");
        SoundManager.instance.PlaySound("Audio/Open");
        isDoorOpen = true;
        guardAppearTime = Time.time;

        yield return new WaitForSeconds(guardReactionTime);

        guardController.StartShooting();


        if (currentFloor < floors)
        {
            StartCoroutine(CloseDoorProcess());
        }



    }

    void SetupBillboards()
    {

        Texture2D leftTex = null, rightTex = null;
        string leftType = "未知", rightType = "未知";
        string leftSubtype = "未知", rightSubtype = "未知";

        FloorData data = experimentData[currentFloor - 1];

        // 如果 CSV 给了 left/right image path，则试图从 imageByRelativePath 中取到对应纹理
        if (!string.IsNullOrEmpty(data.leftImagePath))
        {
            string normLeft = NormalizeRelativePath(data.leftImagePath);
            if (imageByRelativePath.ContainsKey(normLeft))
            {
                leftTex = imageByRelativePath[normLeft];


            }
        }
        if (!string.IsNullOrEmpty(data.rightImagePath))
        {
            string normRight = NormalizeRelativePath(data.rightImagePath);
            if (imageByRelativePath.ContainsKey(normRight))
            {
                rightTex = imageByRelativePath[normRight];

            }
        }
        // 若某一边仍为空（非常罕见），不赋贴图
        if (leftTex != null)
        {
            leftBillboard.material.SetTexture(albedoProperty, leftTex);
            leftBillboard.material.SetTexture(emissionProperty, leftTex);
        }
        if (rightTex != null)
        {
            rightBillboard.material.SetTexture(albedoProperty, rightTex);
            rightBillboard.material.SetTexture(emissionProperty, rightTex);
        }


        leftBillboard.material.color = Color.white;
        rightBillboard.material.color = Color.white;
        leftBillboard.material.SetColor("_EmissionColor", Color.white);
        rightBillboard.material.SetColor("_EmissionColor", Color.white);
        leftBillboard.material.EnableKeyword("_EMISSION");
        rightBillboard.material.EnableKeyword("_EMISSION");

        // 将信息写回数据结构供后续记录
        data.leftImageType = leftType;
        data.rightImageType = rightType;
        data.neutralSubtype = leftType == "中性" ? leftSubtype : rightSubtype;
        data.threatSubtype = leftType == "负面" ? leftSubtype : rightSubtype;
        data.leftImageSubtype = leftSubtype;
        data.rightImageSubtype = rightSubtype;


        if (!string.IsNullOrEmpty(data.leftImagePath))
            data.leftImagePath = NormalizeRelativePath(data.leftImagePath);
        else if (leftTex != null && imagePaths.ContainsKey(leftTex.name))
            data.leftImagePath = imagePaths[leftTex.name];

        if (!string.IsNullOrEmpty(data.rightImagePath))
            data.rightImagePath = NormalizeRelativePath(data.rightImagePath);
        else if (rightTex != null && imagePaths.ContainsKey(rightTex.name))
            data.rightImagePath = imagePaths[rightTex.name];
    }

    IEnumerator CloseDoorProcess()
    {

        var data = experimentData[currentFloor - 1];

        leftBillboard.material.color = Color.black;
        rightBillboard.material.color = Color.black;
        leftBillboard.material.SetColor("_EmissionColor", Color.black);
        rightBillboard.material.SetColor("_EmissionColor", Color.black);

        float delay = GetRandomInRange(doorCloseDelayRange) - guardReactionTime;
        if (delay > 0)

            yield return new WaitForSeconds(delay);

        elevatorAnimator.SetTrigger("Close");
        SoundManager.instance.PlaySound("Audio/Close");
        isDoorOpen = false;
        FirstPersonController.instance.BackToCenter();
        yield return new WaitForSeconds(0.25f);
        guardController.ResetSelf();
        yield return new WaitForSeconds(0.25f);
        if (string.IsNullOrEmpty(data.actualKey))
        {
            data.actualKey = "超时";
            data.reactionTime = -1;
            data.isCorrect = false;
            data.evaluation = "超时";
            evaluationText.color = Color.red;
            combo = 0;
            ShowEvaluation("超时");
            reactionMultiplier = 0f;
        }
        StartCoroutine(NextFloor());
    }



    protected override IEnumerator LoadImagesAsync()
    {
        // 取所有相对路径
        var paths = trialSequence.SelectMany(t => new[] { t.leftImagePath, t.rightImagePath })
                                 .Where(p => !string.IsNullOrEmpty(p))
                                 .Distinct();

        yield return StartCoroutine(LoadImagesAsyncCommon(paths,
            onLoadTex: (relPath, tex) => imageByRelativePath[relPath] = tex
        ));
    }


    void UpdateHealthDisplay()
    {
        healthText.text = $"玩家剩余血量: {Mathf.CeilToInt(playerHealth)}";
    }

    void GameOver()
    {
        isExperimentRunning = false;
        guardController.isShooting = false;

    }
    protected override bool CheckPracticePassed()
    {
        float correctRate = experimentData.Count > 0
            ? experimentData.Count(d => d.isCorrect) / (float)experimentData.Count
            : 0f;
        return correctRate >= 0.9f;
    }

    protected override void GetResults()
    {
        GetResultsCommon(
            experimentData,
            "左图,右图,探测点位置,性别,情绪类型,情绪面孔位置,图片,探测点一致性,控制条件",
            d => d.originalColumns != null ? string.Join(",", d.originalColumns) : ",,,,,,,,",
            d => $"{d.reactionTime:F15},{d.actualKey},{(d.isCorrect ? 1 : 0)}",
            d => d.isCorrect,   // ⭐ 明确告诉父类“怎么算正确”
            "DotProbeResult",
            Encoding.UTF8
        );
    }

    protected override void AfterExperiment()
    {
        isExperimentDone = true;
        if (GameState.instance.gameStateName == GameStateName.D2)
        {
            DialogManager.instance?.ShowDialog("结束");
        }
        else
        {
            DialogManager.instance?.ShowDialog("到达");
            FirstPersonController.instance.autoAim = false;

            if (GameState.instance.gameStateName == GameStateName.start)
            {
                GameState.instance.ChangeGameState(GameStateName.D1);
            }

            guardController.interactArea.SetActive(true);
        }

    }
    public IEnumerator SkipExperiment()
    {
        FirstPersonController.instance.SetCanControll(false);
        //isExperimentDone = true;
        guardController.isDead = true;
        guardController.isShooting = false;

        guardController?.SetDead(true);
        guardController?.SetShooting(false);
        guardAnimator.SetTrigger("lying");
        //yield return new WaitForSeconds(1.5f);
        KaiGuanMen();
        DialogManager.instance?.ShowDialog("到达");
        if (GameState.instance.gameStateName == GameStateName.start)
        {
            GameState.instance.ChangeGameState(GameStateName.D1);
        }

        guardController.interactArea.SetActive(true);
        FirstPersonController.instance.SetCanControll(true);
        yield return new WaitForSeconds(30f);
             KaiGuanMen();
        
    }

    protected override void AfterPratice()
    {
        guardController.isShooting = false;
    }

    // 确保 input 文件夹存在并写入模板（若不存在）
    void EnsureInputFolderAndTemplate()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string inputFolder = Path.Combine(projectRoot, inputFolderName);
        if (!Directory.Exists(inputFolder))
        {
            Directory.CreateDirectory(inputFolder);
        }

        string csvPath = Path.Combine(inputFolder, fileName + ".csv");
        if (!File.Exists(csvPath))
        {
            // 创建模板 CSV（使用你指定的列）
            var templateHeader = "左图,右图,探测点位置,性别,情绪类型,情绪面孔位置,图片,探测点一致性,控制条件";
            File.WriteAllText(csvPath, templateHeader, Encoding.Default); // ANSI 编码

            Debug.Log($"已创建输入模板: {csvPath}");
        }
    }

    // 读取 input.csv 并构造 trialSequence（非放回随机、支持循环）
    protected bool LoadCsv()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string csvPath = Path.Combine(projectRoot, inputFolderName, fileName + ".csv");

        return LoadCsvCommon(
            csvPath,
            Encoding.Default,
            skipHeader: true,
            enableLoop: enableLoop,
            loopCount: loopCount,
            shuffleOnce: true,
            shuffleEachLoop: true,
            parseRow: cols =>
            {
                if (cols.Count < 2) return null;

                var fd = new FloorData();
                fd.originalColumns = cols;

                string leftRaw = RemoveFirstParenthesesContent(cols[0]);
                string rightRaw = RemoveFirstParenthesesContent(cols[1]);

                fd.leftImagePath = NormalizeRelativePath(leftRaw);
                fd.rightImagePath = NormalizeRelativePath(rightRaw);

                return fd;
            },
            out trialSequence
        );
    }

    protected override void OnExperimentStart()
    {
        if (isDoorOpen)
        {
            elevatorAnimator.SetTrigger("Close");
            SoundManager.instance.PlaySound("Audio/Close");
        }
        currentFloor = 0;
        experimentData.Clear();
        playerHealth = maxHealth;
        UpdateHealthDisplay();
        isExperimentRunning = true;
        isExperimentDone = false;
        // 将 floors 设置为试次数（trialSequence 长度）
        floors = trialSequence.Count;
        StartCoroutine(NextFloor());
    }



}
