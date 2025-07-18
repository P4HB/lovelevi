using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 720f; // 1초에 720도까지 회전 가능

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;

        if (moveDir != Vector3.zero)
        {
            // 이동
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            // 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}
