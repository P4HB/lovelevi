using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform cameraTransform; // 인스펙터에서 Main Camera의 Transform을 여기에 연결

    void Start()
    {
        // 커서 잠금 및 숨기기 (카메라 스크립트에서 이미 처리했다면 생략 가능)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        // cameraTransform이 연결되었는지 확인 (선택 사항)
        if (cameraTransform == null)
        {
            Debug.LogError("PlayerController: 'Camera Transform'이 연결되지 않았습니다! Main Camera를 연결해주세요.");
            // Main Camera를 직접 찾을 수도 있지만, 명시적으로 연결하는 것이 좋습니다.
            // cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (cameraTransform == null) return; // 카메라가 없으면 이동하지 않음

        // 1. WASD 입력 값 가져오기
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D 또는 좌우 화살표
        float verticalInput = Input.GetAxis("Vertical");   // W/S 또는 상하 화살표

        // 2. 카메라의 시점 방향을 기준으로 이동 벡터 계산
        // cameraTransform.forward: 카메라가 바라보는 앞 방향
        // cameraTransform.right: 카메라가 바라보는 오른쪽 방향

        // Y축은 지면에 고정되도록 (공중으로 뜨거나 가라앉지 않도록)
        Vector3 forwardMovement = cameraTransform.forward * verticalInput;
        forwardMovement.y = 0; // Y축 이동 방지

        Vector3 rightMovement = cameraTransform.right * horizontalInput;
        rightMovement.y = 0; // Y축 이동 방지

        // 최종 이동 방향 벡터 합산
        Vector3 moveDirection = forwardMovement + rightMovement;
        
        // 대각선 이동 시 속도 보정 (벡터 정규화)
        // moveDirection이 (0,0,0)이 아닐 때만 정규화
        if (moveDirection.magnitude > 0.01f) // 작은 값으로 비교하여 부동 소수점 오차 방지
        {
            moveDirection.Normalize(); 
        }

        // 3. 플레이어 위치 업데이트
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // **플레이어 자체의 회전은 여기서 하지 않습니다.**
        // 플레이어는 카메라 시점과 관계없이 이동만 합니다.
    }
}
