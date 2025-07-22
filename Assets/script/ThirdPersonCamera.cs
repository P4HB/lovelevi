using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player; 
    public float mouseSensitivity = 2f;
    public Vector3 offset = new Vector3(0, 2.5f, -3f); // 카메라 오프셋 (X, Y, Z)

    private float currentRotationX = 0f; 
    private float currentRotationY = 0f; 

    // === 카메라 X축 회전 제한 범위 ===
    public float minVerticalAngle = -10f; // 카메라가 아래를 볼 수 있는 최소 각도
    public float maxVerticalAngle = 80f;  // 카메라가 위를 볼 수 있는 최대 각도

    // === 카메라 충돌 회피 관련 변수 === (재활용 및 확장)
    public float cameraCollisionRadius = 0.3f; // 카메라 충돌 감지 구체의 반지름
    public LayerMask cameraCollisionLayers; // 카메라가 충돌을 감지할 레이어 (BuildingObstacle, Ground 등)
    public float minDistanceToPlayer = 1f; // 카메라가 플레이어에게 얼마나 가까이 붙을 수 있는지 최소 거리
    public float cameraReturnSpeed = 10f; // 충돌 후 카메라가 원래 위치로 돌아오는 속도


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentRotationY = player.eulerAngles.y;
    }

    void LateUpdate() 
    {
        if (player == null) return;

        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 플레이어의 Y축 회전 (좌우 시점)
        currentRotationY += mouseX;

        // 카메라의 X축 회전 (상하 시점)
        currentRotationX -= mouseY; 
        
        // X축 회전 제한 
        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);

        // 플레이어의 회전과 카메라의 상하 회전을 합쳐서 최종 회전값 계산
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
        transform.rotation = rotation;

        // --- 카메라 충돌 회피 로직 ---
        Vector3 desiredCameraPosition = player.position + rotation * offset; // 원래 카메라가 가고 싶은 위치
        Vector3 playerToDesired = desiredCameraPosition - player.position; // 플레이어에서 원하는 카메라 위치까지의 벡터
        float currentMaxDistance = playerToDesired.magnitude; // 현재 최대 거리 (offset.z의 절댓값)

        RaycastHit hit;
        // SphereCast를 사용하여 카메라의 충돌 반경을 고려합니다.
        // 플레이어 위치부터 원하는 카메라 위치까지 레이 발사
        if (Physics.SphereCast(player.position, cameraCollisionRadius, playerToDesired.normalized, out hit, currentMaxDistance, cameraCollisionLayers))
        {
        bool isGroundCollision = Vector3.Dot(hit.normal, Vector3.up) > 0.7f; // 수직에 가까운 평면 = 땅

            if (isGroundCollision)
            {
                // 땅과 충돌한 경우 → 위쪽 보기 허용: 부드럽게 카메라를 플레이어 쪽으로 붙임
                Vector3 adjustedPosition = player.position + playerToDesired.normalized * minDistanceToPlayer;
                transform.position = Vector3.Lerp(transform.position, adjustedPosition, cameraReturnSpeed * Time.deltaTime);
            }
            else
            {
                // 벽과 충돌한 경우 → 기존처럼 카메라 당기기
                Vector3 hitPoint = player.position + playerToDesired.normalized * (hit.distance - cameraCollisionRadius);

                // 최소 거리 보장
                if (Vector3.Distance(hitPoint, player.position) < minDistanceToPlayer)
                {
                    hitPoint = player.position + playerToDesired.normalized * minDistanceToPlayer;
                }

                transform.position = hitPoint;
            }
            }
        else
        {
            // 충돌이 없으면 원하는 위치로 부드럽게 돌아감
            transform.position = Vector3.Lerp(transform.position, desiredCameraPosition, cameraReturnSpeed * Time.deltaTime);
        }
    }

    // 디버깅용으로 Scene 뷰에 카메라 충돌 구체 및 레이 표시
    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            // 카메라 현재 위치에 구체 표시
            Gizmos.DrawWireSphere(transform.position, cameraCollisionRadius);
            
            // 플레이어에서 카메라까지 레이 표시 (충돌 감지 방향)
            Gizmos.DrawLine(player.position, transform.position);
        }
    }
}