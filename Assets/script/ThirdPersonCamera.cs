using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player; // 인스펙터에서 Cube 오브젝트를 여기에 드래그하여 연결
    public float mouseSensitivity = 2f; // 카메라 회전 속도
    public Vector3 offset = new Vector3(0, 1.5f, -3f); // 플레이어 기준 카메라 오프셋 (Y: 높이, Z: 플레이어로부터의 거리)

    private float rotationX = 0f; // 카메라의 X축 (상하) 회전
    private float rotationY = 0f; // 카메라의 Y축 (좌우) 회전

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 초기 카메라 시점을 플레이어의 초기 방향과 일치시키기 위해 (선택 사항)
        if (player != null)
        {
            rotationY = player.eulerAngles.y; // 플레이어의 초기 Y축 회전을 카메라 Y축 회전에 반영
        }
    }

    void LateUpdate() // LateUpdate는 모든 Update 함수가 호출된 후에 호출되어 카메라 움직임이 더 자연스러움
    {
        if (player == null) return;

        // 1. 마우스 입력 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. 카메라의 Y축 (좌우) 회전
        rotationY += mouseX;

        // 3. 카메라의 X축 (상하) 회전
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // 상하 시야각 제한

        // 4. 카메라의 최종 회전 설정
        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        transform.rotation = targetRotation;

        // 5. 카메라의 위치 설정 (플레이어 위치 + 회전된 오프셋)
        transform.position = player.position + targetRotation * offset;
    }
}