using UnityEngine;
using UnityEngine.UI;

public class GrapplingTest : MonoBehaviour
{
    // === 갈고리 관련 변수 ===
    public Transform leftGrappleStartPoint;
    public LineRenderer leftLine;
    private bool isLeftGrappling = false;
    private Vector3 leftGrapplePoint;

    // 오른쪽 갈고리
    public Transform rightGrappleStartPoint;
    public LineRenderer rightLine;
    private bool isRightGrappling = false;
    private Vector3 rightGrapplePoint;

    // === 조준점 관련 변수 ===
    public Image crosshairImage;
    public Color defaultCrosshairColor = Color.white;
    public Color grappleableCrosshairColor = Color.green;

    // === 비행 및 이동 관련 변수 ===
    public Rigidbody playerRigidbody; 
    public float grappleForce = 40f;  
    public float burstAccelerationForce = 40f; 
    public float burstUpwardForce = 30f; 
    public float maxGrappleSpeed = 50f;

    // === 가스 분사 이펙트 관련 변수 ===
    public ParticleSystem gasJetEffect;

    // === 커브 비행 관련 변수 ===
    public float curveForce = 10f;
    public float adKeyCurveMultiplier = 20f; 
    public float maxSideForce = 10f; 

    // === 레이캐스트 필터링 변수 ===
    public LayerMask ignoreLayers; 

    // === WASD 이동 및 회전 관련 변수 ===
    public float moveSpeed = 5f; 
    public float rotationSpeed = 10f; 

    private Quaternion targetPlayerRotation; 

    // === 카메라 FOV 조절 관련 변수 ===
    public Camera playerCamera; 
    public float normalFOV = 60f; 
    public float grapplignFOV = 75f; 
    public float fovTransitionSpeed = 5f; 

    // === 바람 효과 파티클 시스템 ===
    public ParticleSystem windEffect; 

    // === 플레이어 모델 자세 제어 변수 ===
    public Transform playerModel; 
    public float maxTiltAngle = 30f; 
    public float tiltSpeed = 5f; 
    public float maxSideTiltAngle = 15f; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>(); 
            if (playerRigidbody == null)
            {
                Debug.LogError("GrapplingTest: Rigidbody가 연결되지 않았습니다! 플레이어 오브젝트에 Rigidbody를 추가하고 연결해주세요.");
            }
        }
        
        if (gasJetEffect == null)
        {
            Debug.LogWarning("GrapplingTest: Gas Jet Effect Particle System이 연결되지 않았습니다. 인스펙터에 연결해주세요.");
        }
        else
        {
            gasJetEffect.Stop();
        }

        targetPlayerRotation = transform.rotation;

        if (playerCamera == null)
        {
            playerCamera = Camera.main; 
            if (playerCamera == null)
            {
                Debug.LogError("GrapplingTest: Player Camera가 연결되지 않았습니다! 씬에 MainCamera 태그가 붙은 카메라가 없거나 인스펙터에 수동으로 연결해주세요.");
            }
        }
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
        }

        if (windEffect == null)
        {
            Debug.LogWarning("GrapplingTest: Wind Effect Particle System이 연결되지 않았습니다. 인스펙터에 연결해주세요.");
        }
        else
        {
            windEffect.Stop(); 
        }

        if (playerModel == null)
        {
            Debug.LogError("GrapplingTest: PlayerModel Transform이 연결되지 않았습니다! 사람 모양 모델의 부모 오브젝트를 연결해주세요.");
        }
    }

    void Update()
    {
        // --- 조준점 색상 변경 로직 ---
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, ~ignoreLayers); 

        if (crosshairImage != null)
        {
            if (hitSomething && hit.collider.CompareTag("Tree"))
            {
                crosshairImage.color = grappleableCrosshairColor;
            }
            else
            {
                crosshairImage.color = defaultCrosshairColor;
            }
        }

        // --- 왼쪽 갈고리 발사 로직 (마우스 왼클릭) ---
        if (Input.GetMouseButtonDown(0))
        {
            if (hitSomething && hit.collider.CompareTag("Tree"))
            {
                leftGrapplePoint = hit.point;
                isLeftGrappling = true;
                if (leftLine != null)
                    leftLine.positionCount = 2;
            }
            else
            {
                isLeftGrappling = false;
                if (leftLine != null)
                    leftLine.positionCount = 0;
            }
        }

        // --- 오른쪽 갈고리 발사 로직 (마우스 우클릭) ---
        if (Input.GetMouseButtonDown(1))
        {
            if (hitSomething && hit.collider.CompareTag("Tree"))
            {
                rightGrapplePoint = hit.point;
                isRightGrappling = true;
                if (rightLine != null)
                    rightLine.positionCount = 2;
            }
            else
            {
                isRightGrappling = false;
                if (rightLine != null)
                    rightLine.positionCount = 0;
            }
        }
        
        // --- 라인 렌더링 ---
        if (isLeftGrappling)
        {
            if (leftLine != null && leftLine.positionCount >= 2) 
            {
                leftLine.SetPosition(0, leftGrappleStartPoint.position);
                leftLine.SetPosition(1, leftGrapplePoint);
            }
            else if (leftLine != null) 
            {
                 leftLine.positionCount = 2; 
            }
        }
        else 
        {
            if (leftLine != null && leftLine.positionCount > 0)
            {
                leftLine.positionCount = 0;
            }
        }

        if (isRightGrappling)
        {
            if (rightLine != null && rightLine.positionCount >= 2)
            {
                rightLine.SetPosition(0, rightGrappleStartPoint.position);
                rightLine.SetPosition(1, rightGrapplePoint);
            }
            else if (rightLine != null) 
            {
                rightLine.positionCount = 2; 
            }
        }
        else 
        {
            if (rightLine != null && rightLine.positionCount > 0)
            {
                rightLine.positionCount = 0;
            }
        }

        // --- 갈고리 해제 로직 ---
        if (Input.GetMouseButtonUp(0))
        {
            isLeftGrappling = false;
            leftLine.positionCount = 0;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRightGrappling = false;
            rightLine.positionCount = 0;
        }

        // --- WASD 이동 및 회전 로직 ---
        // 갈고리 비행 중이 아닐 때만 WASD 이동 및 회전 적용
        // (주의: isGrapplingActive && Input.GetKey(KeyCode.Space) 대신 isGrapplingActive만 사용하여
        // 비행 중이 아닐 때의 로직을 더 명확히 분리합니다.)
        bool isGrapplingActive = (isLeftGrappling || isRightGrappling); // 이 변수를 여기서도 사용
        bool isHoldingSpaceWhileGrappling = isGrapplingActive && Input.GetKey(KeyCode.Space);


        if (!isHoldingSpaceWhileGrappling) // 갈고리 비행 중 (스페이스바 누름)이 아닐 때
        {
            float horizontalInput = Input.GetAxis("Horizontal"); // A, D
            float verticalInput = Input.GetAxis("Vertical");   // W, S

            Vector3 cameraForwardFlat = Vector3.forward; 
            Vector3 cameraRightFlat = Vector3.right;   

            if (Camera.main != null) 
            {
                cameraForwardFlat = Camera.main.transform.forward;
                cameraForwardFlat.y = 0; 
                if (cameraForwardFlat.magnitude > 0.01f) cameraForwardFlat.Normalize();

                cameraRightFlat = Camera.main.transform.right;
                cameraRightFlat.y = 0; 
                if (cameraRightFlat.magnitude > 0.01f) cameraRightFlat.Normalize();
            }
            
            // 이동 방향 계산
            Vector3 moveInput = cameraRightFlat * horizontalInput + cameraForwardFlat * verticalInput;
            if (moveInput.magnitude > 0.1f) moveInput.Normalize(); 
            
            // 회전 로직: 각 키가 눌리는 순간에만 목표 회전값 업데이트
            if (moveInput.magnitude > 0.1f) 
            {
                if (Input.GetKeyDown(KeyCode.W) && Input.GetKeyDown(KeyCode.A))
                {
                    targetPlayerRotation = Quaternion.LookRotation((cameraForwardFlat - cameraRightFlat).normalized);
                }
                else if (Input.GetKeyDown(KeyCode.W) && Input.GetKeyDown(KeyCode.D))
                {
                    targetPlayerRotation = Quaternion.LookRotation((cameraForwardFlat + cameraRightFlat).normalized);
                }
                else if (Input.GetKeyDown(KeyCode.S) && Input.GetKeyDown(KeyCode.A))
                {
                    targetPlayerRotation = Quaternion.LookRotation((-cameraForwardFlat - cameraRightFlat).normalized);
                }
                else if (Input.GetKeyDown(KeyCode.S) && Input.GetKeyDown(KeyCode.D))
                {
                    targetPlayerRotation = Quaternion.LookRotation((-cameraForwardFlat + cameraRightFlat).normalized);
                }
                // 단일 키 입력
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    targetPlayerRotation = Quaternion.LookRotation(cameraForwardFlat);
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    targetPlayerRotation = Quaternion.LookRotation(-cameraForwardFlat);
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    targetPlayerRotation = Quaternion.LookRotation(-cameraRightFlat); 
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    targetPlayerRotation = Quaternion.LookRotation(cameraRightFlat); 
                }
            }
            
            playerRigidbody.MoveRotation(Quaternion.Slerp(playerRigidbody.rotation, targetPlayerRotation, rotationSpeed * Time.deltaTime));

            if (moveInput.magnitude > 0.1f) 
            {
                playerRigidbody.MovePosition(playerRigidbody.position + moveInput * moveSpeed * Time.deltaTime);
            }
            
        } // WASD 이동/회전 로직 끝


        // --- 비행/가스/자세/FOV 로직 (갈고리 활성화 또는 스페이스바 누름) ---
        // 이 블록은 isGrapplingActive (갈고리가 걸려있는지) 또는 isHoldingSpaceWhileGrappling (갈고리 중 스페이스바) 상태를 처리합니다.
        
        // 가스 이펙트 활성화/비활성화 (지속적인 분출)
        // 갈고리가 하나라도 걸려있으면 가스 이펙트 재생
        if (gasJetEffect != null)
        {
            if (isGrapplingActive) // 갈고리가 하나라도 걸려있으면 지속 분출
            {
                if (!gasJetEffect.isPlaying)
                {
                    gasJetEffect.Play();
                }
            }
            else // 갈고리가 모두 해제되면 정지
            {
                if (gasJetEffect.isPlaying)
                {
                    gasJetEffect.Stop();
                }
            }
        }

        // 바람 효과 활성화/비활성화 (지속적인 분출)
        // 갈고리 중 스페이스바 누른 상태일 때만 바람 효과 활성화
        if (windEffect != null)
        {
            if (isHoldingSpaceWhileGrappling) // 스페이스바를 누르고 있을 때만 바람 효과
            {
                if (!windEffect.isPlaying)
                {
                    windEffect.Play();
                }
            }
            else // 스페이스바를 떼거나 갈고리가 해제되면 정지
            {
                if (windEffect.isPlaying)
                {
                    windEffect.Stop();
                }
            }
        }

        // 카메라 FOV 조절 로직 (스페이스바 누른 상태에 따라)
        if (playerCamera != null)
        {
            float targetFOV = isHoldingSpaceWhileGrappling ? grapplignFOV : normalFOV; // 스페이스바 누른 상태에 따라 FOV 조절
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
        }

        if (playerModel != null)
        {
            if (isHoldingSpaceWhileGrappling)
            {
                Vector3 targetGrapplePoint;
                if (isLeftGrappling && isRightGrappling)
                    targetGrapplePoint = (leftGrapplePoint + rightGrapplePoint) / 2f;
                else if (isLeftGrappling)
                    targetGrapplePoint = leftGrapplePoint;
                else
                    targetGrapplePoint = rightGrapplePoint;

                // 줄 당기는 방향 = 줄의 반대방향
                Vector3 grappleDirection = (transform.position - targetGrapplePoint).normalized;

                // 로컬 기준으로 변환
                Vector3 localGrappleDir = transform.InverseTransformDirection(grappleDirection);

                // Pitch: Z방향이 앞으로 향할수록 뒤로 젖힘 (양수면 앞으로 숙이므로 -붙임)
                float targetPitch = -localGrappleDir.z * maxTiltAngle;

                // Roll: X방향이 오른쪽으로 클수록 오른쪽으로 기울임
                float targetRoll = -localGrappleDir.x * maxSideTiltAngle;

                Quaternion targetTiltRotation = Quaternion.Euler(targetPitch, 0, targetRoll);
                playerModel.localRotation = Quaternion.Slerp(playerModel.localRotation, targetTiltRotation, tiltSpeed * Time.deltaTime);
            }
            else
            {
                Quaternion targetNormalRotation = Quaternion.identity;
                playerModel.localRotation = Quaternion.Slerp(playerModel.localRotation, targetNormalRotation, tiltSpeed * Time.deltaTime);
            }
        }


        // 비행 물리 로직 (갈고리가 하나라도 걸려있고, 스페이스바를 누르고 있을 때만)
        if (isHoldingSpaceWhileGrappling)
        {
            if (playerRigidbody == null) return; 

            Vector3 targetGrapplePoint; 
            if (isLeftGrappling && isRightGrappling)
            {
                targetGrapplePoint = (leftGrapplePoint + rightGrapplePoint) / 2f;
            }
            else if (isLeftGrappling)
            {
                targetGrapplePoint = leftGrapplePoint;
            }
            else // if (isRightGrappling)
            {
                targetGrapplePoint = rightGrapplePoint;
            }
            
            Vector3 directionToTarget = (targetGrapplePoint - transform.position).normalized;
            playerRigidbody.AddForce(directionToTarget * grappleForce, ForceMode.Acceleration);

            // --- 커브 비행 로직 (기존) ---
            Vector3 playerForwardFlat = transform.forward; 
            playerForwardFlat.y = 0;
            if (playerForwardFlat.magnitude > 0.01f) playerForwardFlat.Normalize();

            Vector3 relativeTarget = targetGrapplePoint - transform.position;
            
            Vector3 crossProdPlayerRelative = Vector3.Cross(playerForwardFlat, relativeTarget.normalized);
            Vector3 curveDirection = Vector3.zero;
            if (crossProdPlayerRelative.y > 0.1f)
            {
                curveDirection = -transform.right; 
            }
            else if (crossProdPlayerRelative.y < -0.1f)
            {
                curveDirection = transform.right; 
            }
            if (curveDirection != Vector3.zero)
            {
                playerRigidbody.AddForce(Vector3.ClampMagnitude(curveDirection.normalized * curveForce, maxSideForce), ForceMode.Acceleration);
            }

            // --- 카메라 시점 기반 선회 로직 ---
            if (playerCamera != null)
            {
                Vector3 cameraForwardFlat = playerCamera.transform.forward; 
                cameraForwardFlat.y = 0; 
                if (cameraForwardFlat.magnitude > 0.01f) cameraForwardFlat.Normalize();

                Vector3 cameraRightFlat = playerCamera.transform.right; 
                cameraRightFlat.y = 0;
                if (cameraRightFlat.magnitude > 0.01f) cameraRightFlat.Normalize();

                float angleToTarget = Vector3.SignedAngle(cameraForwardFlat, relativeTarget, Vector3.up);

                Vector3 pivotForceDirection = Vector3.zero; 
                
                if (angleToTarget < -5f) 
                {
                    pivotForceDirection = -cameraRightFlat; 
                }
                else if (angleToTarget > 5f) 
                {
                    pivotForceDirection = cameraRightFlat; 
                }

                if (pivotForceDirection != Vector3.zero)
                {
                    playerRigidbody.AddForce(Vector3.ClampMagnitude(pivotForceDirection.normalized * curveForce * 2f, maxSideForce), ForceMode.Acceleration);
                }
            }

            // --- A, D 키에 따른 추가 커브 로직 ---
            float horizontalInputGrapple = Input.GetAxis("Horizontal"); 
            if (Mathf.Abs(horizontalInputGrapple) > 0.1f) 
            {
                playerRigidbody.AddForce(Vector3.ClampMagnitude(-transform.right * horizontalInputGrapple * adKeyCurveMultiplier, maxSideForce), ForceMode.Acceleration);
            }
            
            if (Input.GetKeyDown(KeyCode.Space)) // 스페이스바 누르는 순간 추가 가속
            {
                Vector3 burstDirection = (directionToTarget * burstAccelerationForce) + (Vector3.up * burstUpwardForce);
                playerRigidbody.AddForce(burstDirection, ForceMode.Impulse);

                // gasJetEffect.Play()는 이미 위에서 isGrapplingActive에 따라 재생되므로 여기서는 제거
            }

            if (playerRigidbody.velocity.magnitude > maxGrappleSpeed)
            {
                playerRigidbody.velocity = playerRigidbody.velocity.normalized * maxGrappleSpeed;
            }
        }
        // 이 else 블록은 비행 물리 로직에만 해당합니다. 이펙트 및 자세 제어는 이 바깥에서 처리됩니다.
        // 기존의 gasJetEffect.Stop()은 위에서 isGrapplingActive에 따라 분리됨.
    }
}