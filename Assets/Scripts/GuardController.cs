using UnityEngine;

public class GuardController : MonoBehaviour
{
    private Animator animator;
    private Transform playerTransform; // 玩家位置参考
    public bool isShooting = false;
    private bool isDead = false;
    
    public AudioSource guardAudio;
    public AudioClip shootClip;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerTransform = Camera.main.transform; // 假设玩家使用主摄像机
    }

    void Update()
    {
        // 如果守卫正在射击且未死亡，持续面向玩家
        if (isShooting && !isDead)
        {
            FacePlayer();
        }
    }

    // 面向玩家
    void FacePlayer()
    {
        // 计算玩家方向（忽略Y轴高度差）
        Vector3 lookDirection = playerTransform.position - transform.position;
        lookDirection.y = 0;
        
        if (lookDirection != Vector3.zero)
        {
            // 平滑旋转面向玩家
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
    }

    // 设置射击状态
    public void SetShooting(bool shooting)
    {
        isShooting = shooting;
    }
    public void ShootAudio()
    {
        guardAudio.PlayOneShot(shootClip);
    }

    // 设置死亡状态
    public void SetDead(bool dead)
    {
        isDead = dead;
    }

    // 动画事件 - 在死亡动画结束时调用
    public void OnDeathAnimationEnd()
    {
        // 通知主控制器
        //DotProbeTask.instance.OnGuardDeathAnimationEnd();
    }
}