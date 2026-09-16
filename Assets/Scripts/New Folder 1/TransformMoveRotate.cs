using UnityEngine;

public class TransformMoveRotate : MonoBehaviour
{
     [Header("Target (Local Space)")]
    public Vector3 targetLocalPosition;
    public Vector3 targetLocalRotation; // 欧拉角（本地）

    [Header("Speed")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 180f;

    private Vector3 originLocalPosition;
    private Quaternion originLocalRotation;

    private bool movingToTarget = false;
    private bool movingToOrigin = false;

    void Start()
    {
        originLocalPosition = transform.localPosition;
        originLocalRotation = transform.localRotation;
        GetComponent<Collider>().isTrigger=true;
    }

    void Update()
    {
        if (movingToTarget)
        {
            MoveAndRotate(
                targetLocalPosition,
                Quaternion.Euler(targetLocalRotation),
                ref movingToTarget
            );
        }
        else if (movingToOrigin)
        {
            MoveAndRotate(
                originLocalPosition,
                originLocalRotation,
                ref movingToOrigin
            );
        }
    }

    void MoveAndRotate(Vector3 pos, Quaternion rot, ref bool flag)
    {
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            pos,
            moveSpeed * Time.deltaTime
        );

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            rot,
            rotateSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.localPosition, pos) < 0.001f &&
            Quaternion.Angle(transform.localRotation, rot) < 0.1f)
        {
            transform.localPosition = pos;
            transform.localRotation = rot;
            flag = false;
        }
    }

    /// <summary>
    /// 移动到目标（本地坐标）
    /// </summary>
    public void MoveToTarget()
    {
        movingToOrigin = false;
        movingToTarget = true;
    }

    /// <summary>
    /// 复原到初始本地坐标
    /// </summary>
    public void ResetToOrigin()
    {
        movingToTarget = false;
        movingToOrigin = true;
    }
}
