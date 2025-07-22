using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player; 
    public float mouseSensitivity = 2f;
    public Vector3 offset = new Vector3(0, 2.5f, -3f); // 카메라 오프셋 (X, Y, Z)

    private float currentRotationX = 0f; 
    private float currentRotationY = 0f; 

    // === 카메라 X축 회전 제한 범위 ===
    public float minVerticalAngle = -10f; // 카메라가 아래를 볼 수 있는 최소 각도 (0으로 시작하여 조절)
    public float maxVerticalAngle = 80f;  // 카메라가 위를 볼 수 있는 최대 각도

    // === 카메라 충돌 회피 관련 변수 (새로 추가) ===
    public float cameraCollisionRadius = 0.3f; // 카메라 충돌 감지 구체의 반지름
    public LayerMask cameraCollisionLayers; // 카메라가 충돌을 감지할 레이어 (Ground, Building 등)
    public float minDistanceToPlayer = 1f; // 카메라가 플레이어에게 얼마나 가까이 붙을 수 있는지 최소 거리
    public float maxDistanceToPlayer = 5f; // 카메라가 플레이어로부터 얼마나 멀어질 수 있는지 최대 거리 (offset Z와 연동)
    public float cameraReturnSpeed = 10f; // 충돌 후 카메라가 원래 위치로 돌아오는 속도


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentRotationY = player.eulerAngles.y;
        // player.transform.position + rotation * offset 계산 시 offset.z가 음수이므로
        // maxDistanceToPlayer를 offset.z의 절댓값과 동일하게 맞추거나, offset.z를 maxDistanceToPlayer로 대체합니다.
        maxDistanceToPlayer = -offset.z; 
    }

    void LateUpdate() // Update 대신 LateUpdate를 사용하면 플레이어 이동 후 카메라가 움직여 더 자연스러움
    {
        if (player == null) return;

        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 플레이어의 Y축 회전 (좌우 시점)
        currentRotationY += mouseX;

        // 카메라의 X축 회전 (상하 시점)
        currentRotationX -= mouseY; 
        
        // X축 회전 제한 (minVerticalAngle은 0이나 아주 약간의 음수부터 시작)
        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);

        // 플레이어의 회전과 카메라의 상하 회전을 합쳐서 최종 회전값 계산
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
        transform.rotation = rotation;

        // --- 카메라 충돌 회피 로직 (새로 추가) ---
        Vector3 desiredCameraPosition = player.position + rotation * offset; // 원래 카메라가 가고 싶은 위치
        Vector3 correctedCameraPosition = desiredCameraPosition;

        // 플레이어와 원하는 카메라 위치 사이에 레이캐스트/스피어캐스트 발사
        RaycastHit hit;
        // SphereCast를 사용하여 카메라의 충돌 반경을 고려합니다.
        if (Physics.SphereCast(player.position, cameraCollisionRadius, (desiredCameraPosition - player.position).normalized, out hit, maxDistanceToPlayer, cameraCollisionLayers))
        {
            // 충돌이 감지되면 충돌 지점까지 카메라 위치를 당겨옴
            correctedCameraPosition = player.position + (desiredCameraPosition - player.position).normalized * (hit.distance - cameraCollisionRadius);

            // 카메라가 플레이어에게 너무 가까이 붙지 않도록 최소 거리 제한
            if (Vector3.Distance(correctedCameraPosition, player.position) < minDistanceToPlayer)
            {
                correctedCameraPosition = player.position + (correctedCameraPosition - player.position).normalized * minDistanceToPlayer;
            }

            // 카메라가 바닥을 파고드는 것을 방지 (옵션)
            // 만약 correctedCameraPosition의 Y가 플레이어 위치보다 낮다면, 최소 Y값 설정
            if (correctedCameraPosition.y < player.position.y - 0.5f) // 플레이어 발 밑보다 더 내려가지 않게
            {
                correctedCameraPosition.y = player.position.y - 0.5f;
            }

            // 충돌 시 카메라가 더 위로 올라가거나, 각도를 조절하여 바닥을 보지 않게 할 수도 있습니다.
            // 예를 들어, 플레이어가 땅에 가까이 있을 때만 minVerticalAngle을 높여 바닥 밑을 못 보게 하고,
            // 공중에서는 minVerticalAngle을 낮게 유지할 수 있습니다. (아래 2번 참조)
        }
        else
        {
            // 충돌이 없으면 원하는 위치로 부드럽게 돌아감
            correctedCameraPosition = Vector3.Lerp(transform.position, desiredCameraPosition, cameraReturnSpeed * Time.deltaTime);
        }

        transform.position = correctedCameraPosition;
    }

    // 디버깅용으로 Scene 뷰에 카메라 충돌 구체 및 레이 표시
    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, cameraCollisionRadius);
            Gizmos.DrawLine(player.position, transform.position);
        }
    }
}