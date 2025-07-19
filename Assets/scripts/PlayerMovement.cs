using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    void Update()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        if (inputDirection != Vector3.zero)
        {
            // 즉시 회전: 튀지 않도록 정식으로 회전 대입
            transform.rotation = Quaternion.LookRotation(inputDirection);

            // 이동
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }
}
