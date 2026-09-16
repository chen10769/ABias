using System.IO;
using Fungus;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{

    public static FirstPersonController instance;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Look Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 80f;

    [Header("UI Settings")]
    public Text speedDisplay;

    [Header("Auto Aim Settings")]
    public bool autoAim = true;              // 是否开启自动瞄准
    public Transform leftTarget;             // 左边目标
    public Transform centerTarget;           // 中间目标
    public Transform rightTarget;            // 右边目标
    public float aimSpeed = 5f;               // 瞄准旋转速度

    private Transform currentAimTarget;      // 当前瞄准目标

    public Transform startPos;
    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 velocity;
    private float rotationX = 0;
    private bool canRotate = true;
    private bool isGrounded;
    private bool isCrouching;
    private float standingHeight;
    private Vector3 standingCenter;
    private float crouchingHeight = 1f;
    private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    private float originalWalkSpeed;
    private Vector3 lastPosition;
    public float currentSpeed;
    public Animator gunAnimator;
    public GameObject adjustment;
    bool isDebugMode;
    public bool canControll = false;


    [SerializeField] private string targetTag = "Evidence"; // 要检测的标签
    private GameObject lastHitObject = null; // 记录上一次击中的物体

    void Awake()
    {
        instance = this;
        if (PersistentObject.instance != null && PersistentObject.instance.isSettingMode)
        {
            EnterSettingMode();
        }
    }
    public void EnterSettingMode()
    {
        autoAim = true;
        gameObject.transform.position = startPos.position;
        adjustment.SetActive(true);
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        originalWalkSpeed = walkSpeed;
        standingHeight = characterController.height;
        standingCenter = characterController.center;
        lastPosition = transform.position;

        // 默认瞄准中间
        currentAimTarget = centerTarget;
        if (PersistentObject.instance != null && !PersistentObject.instance.isSettingMode)
            LockCursor();
        string stateFilePath = Path.Combine(Application.dataPath, "../debugState.txt");

        // 读取状态
        if (File.Exists(stateFilePath))
        {
            string state = File.ReadAllText(stateFilePath).Trim();
            isDebugMode = state == "true";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isDebugMode)
        {
            UIManager.instance.pausePanel.SetActive(!UIManager.instance.pausePanel.activeInHierarchy);
            // GameObject dialog = GameObject.Find("SayDialog");
            // if (dialog != null)
            // {
            //      dialog.GetComponent<DialogInput>().enabled = !UIManager.instance.pausePanel.activeInHierarchy;
            // }
            
           
        }
        CheckCursorInput();
        if (UIManager.instance.isUI || canControll == false)
            return;
        if (PersistentObject.instance != null && PersistentObject.instance.isSettingMode)
            return;

        HandleGroundCheck();
        //MouseRaycast();
        if (autoAim)
        {
            HandleAutoAimInput();
            HandleAutoAim();
        }
        else
        {
            if (canRotate)
            {
                HandleMovement();
                //HandleJump();
                HandleRotation();
            }
        }

        
        CalculateSpeed();
        UpdateSpeedDisplay();


       
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ItemGet.instance.ShowEvidenceList();
            UnlockCursor();

        }
    }
    
    void HandleAutoAimInput()
    {
        if (DotProbeTask.instance.isDoorOpen == false)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentAimTarget = leftTarget;

        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            currentAimTarget = rightTarget;
        }
        else
        {
            //currentAimTarget = centerTarget;
        }


    }
    public void BackToCenter()
    {
        currentAimTarget = centerTarget;
    }
    public void SetCanControll(bool flag)
    {
        canControll = flag;
    }

    public void HandleAutoAim()
    {
        if (currentAimTarget != null)
        {

            // 计算水平旋转
            Vector3 dir = (currentAimTarget.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * aimSpeed);

            // 计算垂直旋转
            Vector3 camDir = (currentAimTarget.position - playerCamera.transform.position).normalized;
            Quaternion camRot = Quaternion.LookRotation(camDir);
            playerCamera.transform.rotation = Quaternion.Lerp(playerCamera.transform.rotation, camRot, Time.deltaTime * aimSpeed);
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J))
            {
                Shoot();
            }

        }
    }

    public void Shoot()
    {
        UIManager.instance.crossHairAni.SetTrigger("fire");
        gunAnimator.SetTrigger("Shoot");
        SoundManager.instance.PlaySound("Audio/heavy_blast_006");

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
        {
            if (hit.collider.CompareTag("Enemy") && !DotProbeTask.instance.guardController.isDead)
            {
                hit.collider.gameObject.GetComponent<Animator>().SetTrigger("dead");
                UIManager.instance.crossHairAni.SetTrigger("red");
                DotProbeTask.instance.ShootEnemy();
            }
        }
    }

    void CalculateSpeed()
    {
        float distanceMoved = Vector3.Distance(lastPosition, transform.position);
        currentSpeed = distanceMoved / Time.deltaTime;
        gunAnimator.SetFloat("Speed", currentSpeed);
        lastPosition = transform.position;
    }

    void UpdateSpeedDisplay()
    {
        if (speedDisplay != null)
            speedDisplay.text = $"SPEED: {currentSpeed:0.0} m/s";
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.SphereCast(
            transform.position + characterController.center,
            characterController.radius,
            Vector3.down,
            out _,
            (characterController.height / 2) + groundCheckDistance,
            groundLayer
        );
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDirection = transform.TransformDirection(new Vector3(moveX, 0, moveZ));

        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching && moveZ > 0 && isGrounded)
            moveDirection *= sprintSpeed;
        else if (isCrouching)
            moveDirection *= crouchSpeed;
        else
            moveDirection *= walkSpeed;

        velocity.y += gravity * Time.deltaTime;
        characterController.Move((moveDirection + velocity) * Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;
        transform.Rotate(0, mouseX, 0);
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    void CheckCursorInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // canRotate = !canRotate;
            // if (canRotate) LockCursor();
            // else UnlockCursor();
            UnlockCursor();
        }
        else if (Input.anyKeyDown)
        {

            LockCursor();
        }
         if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
            
        }
    }

    public void LockCursor()
    {
        if (!UIManager.instance.isUI)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canRotate = true;
        }

    }
   
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        canRotate = false;
    }
    bool IsPointerOverUI()
    {
        // EventSystem.current.IsPointerOverGameObject() 检查当前指针是否在 UI 上
        return EventSystem.current.IsPointerOverGameObject();
    }
}
