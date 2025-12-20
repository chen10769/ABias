using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Globalization;
using System.Text;


public class DotProbeTask : MonoBehaviour
{

    [Header("加载设置")]
    public GameObject loadingPanel; // 加载面板
    public Slider progressBar;      // 进度条
    public Text progressText;       // 进度文本
    public bool isLoadingComplete = false; // 加载完成标志
    [Header("实验设置")]
    public int floors = 5; //楼层数（实验次数） - 会在读取 CSV 后根据试次数覆盖
    public Vector2 doorOpenDelayRange = new Vector2(0.25f, 1f);//开门前的间隔 图片显示时间
    public Vector2 floorIntervalRange = new Vector2(0.25f, 1f);//到下一楼层的间隔时间
    public float guardReactionTime = 0.5f; //敌人发现玩家并开枪的反应时间
    public Vector2 doorCloseDelayRange = new Vector2(0.5f, 1.5f); //关门前间隔（新增范围）
    string fileName;

    public string inputCsvFileName = "input"; // 放在项目根 input 文件夹中的 CSV
    [Range(0f, 1f)] public float neutralSideProbability = 0.5f;//出现在中性的概率
    public bool willShowHp = true;
    public bool willShowRedScreen = true;
    public bool willShowEva = true;

    [Header("CSV / 输入设置")]
    public bool enableLoop = false; // 是否循环试次
    public int loopCount = 1;       // 循环次数（当 enableLoop 为 true 时）

    public string inputFolderName = "input";     // 相对 project root 的 input 文件夹
    [Header("练习阶段设置")]
    public bool hasPractice = true;               // 是否有练习阶段
    public string practiceCsvFileName = "practice"; // 练习阶段用的 CSV 文件名
    //public GameObject practiceResultPanel;        // 显示练习结果的面板
    //public Text practiceResultText;               // 面板上的提示文字

    private bool isPracticePhase = false;         // 当前是否在练习阶段
    private bool practiceFinished = false;        // 是否完成了练习
    private bool practicePassed = false;          // 练习是否通过

    public int practiceLoopCount = 1;

    [Header("休息设置")]
    public int trialsPerBreak = 20; // 每多少次进入休息
    public float breakDuration = 30f; // 休息时间（秒）


    private bool isOnBreak = false;
    private float breakTimer = 0f;


    [Header("UI显示")]
    public Text floorText;
    public Text resultsText;
    public Text healthText;
    public GameObject healthPanel;
    public Text evaluationText;
    public Animator evaluationAni;

    [Header("评价设置")]
    public float perfectThreshold = 0.3f;
    public float greatThreshold = 0.5f;
    // public float goodThreshold = 0.7f;
    // public float badThreshold = 1.0f;

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

    // 1. FloorData 新增字段
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

    private Texture2D[] neutralTextures;
    private Texture2D[] threatTextures;
    public string[] supportedFormats = { ".png", ".jpg", ".jpeg", ".bmp", ".tga" };

    private Dictionary<string, string> neutralImageSubtypes = new();
    private Dictionary<string, string> threatImageSubtypes = new();

    private List<FloorData> experimentData = new();
    private List<FloorData> trialSequence = new(); // 从 CSV 读取后构造的试次序列（含循环）
    private int currentFloor = 0;
    public bool isExperimentRunning = false;
    public bool isExperimentDone = false;
    //public bool isGuardShooting = false;
    private float playerHealth;
    private float guardAppearTime;
    private bool guardOnLeft;
    public bool guardIsDead = false;
    public static DotProbeTask instance;

    bool neutralOnLeft;
    public GameObject tip;
    public GameObject beforeP;
    public GameObject InteractArea;
    public GameObject error;

    void Awake() => instance = this;

    void Start()
    {
        // 加载保存的设置
        SettingsManager.LoadDotProbeSettings(this);

        playerHealth = maxHealth;
        UpdateHealthDisplay();
        floorText.text = "准备开始";
        evaluationText.text = "";

        // 在启动时检查 input 文件夹和模板
        EnsureInputFolderAndTemplate();

    }

    public void ShowTip()
    {
        if (PersistentObject.instance != null && PersistentObject.instance.isSettingMode)
            return;
        if (GameState.instance.gameStateName == GameStateName.beforeD1 || GameState.instance.gameStateName == GameStateName.beforeD2)
        {
            tip.SetActive(true);
        }
    }


    void Update()
    {
        if (PersistentObject.instance != null && PersistentObject.instance.isSettingMode)
            return;
        if (Input.GetKeyDown(KeyCode.Space) && !isExperimentRunning && (tip.activeInHierarchy || beforeP.activeInHierarchy))
        {
            if (!hasPractice)
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
                    string evaluation = GetEvaluation(reactionTime);
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

        if (guardController.isShooting && !guardIsDead)
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


    private float GetRandomInRange(Vector2 range)
    {
        float num = Mathf.Approximately(range.x, range.y) ? range.x : Random.Range(range.x, range.y);
        return num;
    }
    public void ShootEnemy()
    {
        guardIsDead = true;
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

    private string GetEvaluation(float reactionTime)
    {
        var data = experimentData[currentFloor - 1];
        if (data.isCorrect == false)
        {
            evaluationText.color = Color.red;
            return "Error";
        }
        else
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


    }

    private void ShowEvaluation(string evaluation)
    {
        evaluationText.text = evaluation;
        evaluationAni.SetTrigger("play");
    }

    public void StartExperiment(bool practice = false)
    {

        isPracticePhase = practice;
        fileName = practice ? practiceCsvFileName : inputCsvFileName;
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string inputFolder = Path.Combine(projectRoot, inputFolderName);
        string csvPath = Path.Combine(inputFolder, fileName + ".csv");

        // 读取 input.csv 构建 trialSequence
        bool ok = ReadInputCsvAndBuildSequence();
        if (!ok)
        {
            Debug.LogWarning("无法读取输入 CSV，请检查 input/input.csv 文件或模板。");
            return;
        }

        // 显示加载面板
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        FirstPersonController.instance.autoAim = true;
        FirstPersonController.instance.gameObject.transform.position = FirstPersonController.instance.startPos.position;
        // 开始异步加载图片（会建立 imageByRelativePath 映射）
        StartCoroutine(LoadImagesAsync(fileName));
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

    // NextFloor：在敌人出现时保存正确按键、位置和一致性（部分保持原逻辑，但使用 trialSequence）
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
        floorText.text = $"第 {currentFloor} 层";

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
        // 探测点位置来自 CSV 的第三列（如果 CSV 写的是“左/右/随机”）
        string probePos = (trialSequence[currentFloor - 1].originalColumns != null && trialSequence[currentFloor - 1].originalColumns.Count >= 3) ? trialSequence[currentFloor - 1].originalColumns[2] : "";
        experimentData.Add(data);

        yield return new WaitForSeconds(GetRandomInRange(floorIntervalRange));

        SetupBillboards(); // 将使用 experimentData[currentFloor-1] 中的 left/right path



        guardIsDead = false;
        guardController?.SetDead(false);

        yield return new WaitForSeconds(GetRandomInRange(doorOpenDelayRange));
        // 决定 guardOnLeft：优先使用 CSV 中的探测点位置（若为左/右/随机），否则使用随机或中性概率逻辑
        bool guardOnNeutralSide = Random.value < neutralSideProbability;

        // 如果 CSV 提供了探测点位置（左/右），则使用其决定 guardOnLeft；如果是"随机"或空则回退至原来逻辑
        string csvProbePos = (trialSequence[currentFloor - 1].originalColumns != null && trialSequence[currentFloor - 1].originalColumns.Count >= 3) ? trialSequence[currentFloor - 1].originalColumns[2].Trim() : "";
        if (!string.IsNullOrEmpty(csvProbePos))
        {
            if (csvProbePos == "左" || csvProbePos.ToLower() == "left")
                guardOnLeft = true;
            else if (csvProbePos == "右" || csvProbePos.ToLower() == "right")
                guardOnLeft = false;
            else
            {
                // csv 写了其他值（如 随机），回退到原逻辑
                if (experimentData[currentFloor - 1].leftImageType == "中性" && experimentData[currentFloor - 1].rightImageType == "中性")
                {
                    guardOnLeft = Random.value > 0.5f;
                }
                else
                {
                    guardOnLeft = guardOnNeutralSide ? neutralOnLeft : !neutralOnLeft;
                }
            }
        }
        else
        {
            // CSV 未提供探测点位置，使用原逻辑
            if (experimentData[currentFloor - 1].leftImageType == "中性" && experimentData[currentFloor - 1].rightImageType == "中性")
            {
                guardOnLeft = Random.value > 0.5f;
            }
            else
            {
                guardOnLeft = guardOnNeutralSide ? neutralOnLeft : !neutralOnLeft;
            }
        }

        data.guardOnNeutralSide = guardOnNeutralSide;
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
        if (!guardController.isShooting && !guardIsDead)
        {
            guardController.isShooting = true;
            guardAnimator.SetTrigger("shoot");
            guardController?.SetShooting(true);
        }
        //yield return new WaitForSeconds(1f);
        if (currentFloor < floors)
        {
            StartCoroutine(CloseDoorProcess());
        }



    }

    // SetupBillboards：优先使用 CSV 指定的图片（相对路径），找不到再退回到随机选择逻辑
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

        // 颜色和发光设置...
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

        // 保存完整路径（使用 CSV 提供或 imagePaths 映射）
        // imagePaths: name -> relativePath
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
        // ====== 超时判定 ======
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
        guardController.isShooting = false;
        guardAnimator.SetTrigger("reset");
        yield return new WaitForSeconds(0.25f);
        if (string.IsNullOrEmpty(data.actualKey))
        {
            data.actualKey = "超时";
            data.reactionTime = -1;   // 用 -1 或者留空来表示超时
            data.isCorrect = false;
            data.evaluation = "超时";
            evaluationText.color = Color.gray;
            ShowEvaluation("超时");
        }
        StartCoroutine(NextFloor());
    }


    // 异步加载图片协程：与旧逻辑类似，但额外构建 imageByRelativePath 映射（相对 TestImg\ 的路径）
    private IEnumerator LoadImagesAsync(string setName)
    {
        isLoadingComplete = false;
        progressBar.value = 0;
        progressText.text = "0%";

        string basePath = Path.Combine(Application.streamingAssetsPath, "TestImg", setName);

        if (!Directory.Exists(basePath))
        {
            error.SetActive(true);
            yield break; // 没找到文件夹，直接退出
        }

        // 获取所有图片文件路径（只取常见格式）
        List<string> allFiles = new List<string>();
        string[] extensions = new string[] { "*.png", "*.jpg", "*.jpeg" };
        foreach (string ext in extensions)
        {
            allFiles.AddRange(Directory.GetFiles(basePath, ext, SearchOption.AllDirectories));
        }

        int totalFiles = allFiles.Count;
        int loadedFiles = 0;

        // 遍历所有文件并加载
        foreach (string file in allFiles)
        {
            byte[] fileData = File.ReadAllBytes(file);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            // 取相对路径（相对于 TestImg）
            string relativePath = file.Substring(Path.Combine(Application.streamingAssetsPath, "TestImg").Length + 1);
            relativePath = relativePath.Replace("\\", "/"); // 统一分隔符

            // 存到映射：key 使用相对路径（包含子文件夹）
            imageByRelativePath[relativePath] = tex;

            // 可选：建立文件名到路径的映射（如果需要）
            string fileName = Path.GetFileNameWithoutExtension(file);
            imagePaths[fileName] = relativePath;
            // 更新进度
            loadedFiles++;
            float progress = (float)loadedFiles / Mathf.Max(1, totalFiles);
            progressBar.value = progress;
            progressText.text = $"加载中：{(int)(progress * 100)}%";

            // 每帧加载一张图片
            yield return null;
        }

        // 加载完成
        isLoadingComplete = true;
        loadingPanel.SetActive(false);



        // 开始实验流程
        StartExperimentAfterLoading();
    }




    // 加载完成后的实验流程
    private void StartExperimentAfterLoading()
    {
        if (isDoorOpen)
        {
            elevatorAnimator.SetTrigger("Close");
            SoundManager.instance.PlaySound("Audio/Close");
        }

        resultsText.gameObject.SetActive(false);
        currentFloor = 0;
        experimentData.Clear();
        playerHealth = maxHealth;
        UpdateHealthDisplay();
        isExperimentRunning = true;
        isExperimentDone = false;

        // 将 floors 设置为试次数（trialSequence 长度）
        floors = trialSequence.Count;

        // 开始第一层
        StartCoroutine(NextFloor());
    }



    Texture2D LoadTextureFromFile(string filePath)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new(2, 2);
            tex.LoadImage(fileData);
            tex.name = Path.GetFileNameWithoutExtension(filePath);
            return tex;
        }
        catch { return null; }
    }

    void UpdateHealthDisplay()
    {
        healthText.text = $"玩家剩余血量: {Mathf.CeilToInt(playerHealth)}";
    }

    void GameOver()
    {
        isExperimentRunning = false;
        guardController.isShooting = false;
        resultsText.text = "游戏失败!";
    }

    void EndExperiment()
    {

        isExperimentRunning = false;
        isExperimentDone = true;
        leftBillboard.material.color = Color.black;
        rightBillboard.material.color = Color.black;
        leftBillboard.material.SetColor("_EmissionColor", Color.black);
        rightBillboard.material.SetColor("_EmissionColor", Color.black);
        DisplayResults();
        if (isPracticePhase && hasPractice)
        {
            float correctRate = experimentData.Count > 0
                ? experimentData.Count(d => d.isCorrect) / (float)experimentData.Count
                : 0f;

            practiceFinished = true;
            practicePassed = correctRate >= 0.9f;
            Invoke("showPPa", 1f);

            if (practicePassed)
            {
                UIManager.instance.practiceResultText.text = "按<color=red> 空格 </color>键进入正式实验";
            }
            else
            {
                UIManager.instance.practiceResultText.text = "正确率小于90% (" + (correctRate * 100f).ToString("F1") + "%)\n请按<color=red> 空格 </color>键重新开始练习";
            }
            FirstPersonController.instance.BackToCenter();

            KaiGuanMen();
        }
        else
        {
            //TipManager.instance.ToShowTip("请前往办公室");
            Invoke("MissonComplete", 1f);
            FirstPersonController.instance.autoAim = false;
            if (GameState.instance.gameStateName == GameStateName.beforeD1)
            {
                GameState.instance.ChangeGameState(GameStateName.afterD1);
            }
            else if (GameState.instance.gameStateName == GameStateName.beforeD2)
            {
                GameState.instance.ChangeGameState(GameStateName.afterD2);
            }

        }

        //InteractArea.SetActive(true);
    }
    public void showPPa()
    {
        guardController.isShooting = false;
        guardAnimator.SetTrigger("reset");
        UIManager.instance.practiceResultPanel.SetActive(true);
    }
    public void MissonComplete()
    {
        //UIManager.instance.companel.SetActive(true);
        //FirstPersonController.instance.UnlockCursor();
        TipManager.instance.ToShowTip("请前往办公室");
    }


    // DisplayResults：输出新格式，增加正确率统计；现在使用 originalColumns（若存在）将输入列保留下来，并在末尾追加反应时间与正确与否
    void DisplayResults()
    {
        string resultStr = "点探测任务实验结果:\n\n";
        resultStr += "输入列...,反应时间,实际反应,正确与否,总体正确率,学号,电话号码,组别\n";
        resultStr += "----------------------------------------------------------------------------\n";


        List<string> csvLines = new List<string>();
        int correctCount = experimentData.Count(d => d.isCorrect);
        float accuracy = experimentData.Count > 0 ? (float)correctCount / experimentData.Count : 0f;

        // 判断是否有原始列
        if (experimentData.Count > 0 && experimentData[0].originalColumns != null && experimentData[0].originalColumns.Count > 0)
        {
            csvLines.Add("左图,右图,探测点位置,性别,情绪类型,情绪面孔位置,图片,探测点一致性,控制条件,反应时间,实际反应,正确与否,总体正确率,学号,电话号码,组别");
        }
        else
        {
            csvLines.Add("左图,右图,探测点位置,反应时间,实际反应,正确与否,总体正确率,学号,电话号码,组别");
        }
        bool haveExtraInfo = false;
        // 数据行
        foreach (var d in experimentData)
        {
            string original = d.originalColumns != null ? string.Join(",", d.originalColumns) : "";

            // 构建额外信息字符串
            string extraInfo = "";
            if (PersistentObject.instance != null && !haveExtraInfo)
            {
                extraInfo = $",{accuracy:P2},=\"{PersistentObject.instance.studentId}\",=\"{PersistentObject.instance.phoneNumber}\",{PersistentObject.instance.group}";
                haveExtraInfo = true;
            }
            else
            {
                extraInfo = ",,,,";
            }

            string line = $"{original},{d.reactionTime:F15},{d.actualKey},{(d.isCorrect ? "1" : "0")}{extraInfo}";
            csvLines.Add(line);
        }

        // 显示到 UI
        resultsText.text = string.Join("\n", csvLines);

        // 保存到 CSV
        SaveCsvFile(csvLines);
    }

    // public void DisableResult()
    // {
    //     resultsText.gameObject.SetActive(false);
    // }

    void SaveCsvFile(List<string> csvLines)
    {
        string folderPath = Path.Combine(Application.dataPath, "../ExperimentResults");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filePath = Path.Combine(folderPath, $"{PersistentObject.instance.studentId}_{PersistentObject.instance.phoneNumber}_{PersistentObject.instance.group}_{fileName}_DotProbeResult.csv");

        try
        {
            File.WriteAllLines(filePath, csvLines, Encoding.Default);
            Debug.Log($"结果已保存至: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("保存CSV文件时出错: " + e.Message);
        }
    }

    // -------------------
    // CSV 读取与构建试次序列方法
    // -------------------

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
    // 去掉某个特定字符串前面的内容
    string RemoveBeforeFirstOccurrence(string fullPath, string target)
    {
        if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(target))
            return fullPath;

        int idx = fullPath.IndexOf(target);
        if (idx >= 0)
        {
            return fullPath.Substring(idx); // 保留 target 及其后面内容
        }
        return fullPath;
    }
    // 读取 input.csv 并构造 trialSequence（非放回随机、支持循环）
    bool ReadInputCsvAndBuildSequence()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string inputFolder = Path.Combine(projectRoot, inputFolderName);
        string csvPath = Path.Combine(inputFolder, fileName + ".csv");

        if (!File.Exists(csvPath))
        {
            Debug.LogError($"找不到输入文件: {csvPath}");
            error.SetActive(true);
            return false;
        }

        List<string> allLines;

        try
        {
            allLines = File.ReadAllLines(csvPath, Encoding.Default)
                           .Where(l => !string.IsNullOrWhiteSpace(l))
                           .ToList();
        }
        catch (IOException e)
        {
            TipManager.instance.ToShowTip("表格被占用，请关闭 Excel/WPS 后重试！");
            Debug.LogError(e.Message);
            error.SetActive(true);
            return false;
        }

        if (allLines.Count <= 0)
        {
            Debug.LogError("输入 CSV 无内容");
            error.SetActive(true);
            return false;
        }

        int startIndex = 0;
        var first = allLines[0];
        if (first.Contains("左图") && first.Contains("右图"))
            startIndex = 1;

        List<FloorData> rows = new List<FloorData>();

        for (int i = startIndex; i < allLines.Count; i++)
        {
            var cols = allLines[i].Split(',').Select(s => s.Trim()).ToList();
            if (cols.Count < 2) continue;

            FloorData fd = new FloorData();
            fd.originalColumns = cols;

            string leftRaw = cols[0];
            string rightRaw = cols[1];

            leftRaw = RemoveBeforeFirstOccurrence(leftRaw, fileName);
            rightRaw = RemoveBeforeFirstOccurrence(rightRaw, fileName);

            fd.leftImagePath = NormalizeRelativePath(leftRaw);
            fd.rightImagePath = NormalizeRelativePath(rightRaw);

            rows.Add(fd);
        }

        if (rows.Count == 0)
        {
            Debug.LogError("输入 CSV 行数不足（没有有效数据行）");
            error.SetActive(true);
            return false;
        }

        var shuffled = rows.OrderBy(x => Random.value).ToList();

        trialSequence.Clear();
        if (enableLoop && loopCount > 0 && !isPracticePhase)
        {
            for (int c = 0; c < loopCount; c++)
                trialSequence.AddRange(shuffled.OrderBy(x => Random.value));
        }
        else
        {
            trialSequence.AddRange(shuffled);
        }

        floors = trialSequence.Count;
        Debug.Log($"已读取 CSV，构建试次 {floors} 次");

        return true;
    }


    // 规范化 csv 中写的相对路径，返回与 LoadImagesAsync 中 imageByRelativePath 一致的 key
    // 将 "Test1\\负面\\1 (6).png" 或 "TestImg\\Test1\\负面\\1 (6).png" 等转换为 "Test1\负面\1 (6).png"（使用系统分隔符）
    string NormalizeRelativePath(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        string s = raw.Replace('\\', '/').Replace("//", "/").Trim();

        // 移除 TestImg 前缀
        string prefix = "TestImg/";
        if (s.StartsWith(prefix))
            s = s.Substring(prefix.Length);

        return s;
    }
    private IEnumerator StartBreak()
    {
        isOnBreak = true;
        UIManager.instance.breakPanel.SetActive(true);

        breakTimer = breakDuration;
        while (breakTimer > 0)
        {
            UIManager.instance.breakText.text = $"现在您有 {Mathf.CeilToInt(breakTimer)} 秒的休息时间";
            breakTimer -= Time.deltaTime;
            yield return null;
        }

        UIManager.instance.breakText.text = "按<color=red> 空格 </color>键继续";

        // 等待玩家按空格
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        UIManager.instance.breakPanel.SetActive(false);
        isOnBreak = false;
    }





}
