using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 720f;
    public Transform cameraTransform;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (cameraTransform == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 1. 카메라 기준으로 입력 방향 계산
        Vector3 forward = cameraTransform.forward; forward.y = 0;
        Vector3 right = cameraTransform.right; right.y = 0;

        Vector3 moveDirection = (forward * v + right * h);

        // 2. 이동 방향이 있을 때만 처리
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            animator.SetBool("isRunning", true);
            moveDirection.Normalize();

            // ✅ 캐릭터를 이동 방향으로 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            // ✅ 이동
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        else // 캐릭터가 멈춰있을 때
        {
            animator.SetBool("isRunning", false);
        }
    }
}
