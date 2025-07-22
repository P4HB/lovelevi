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
    public string[] grappleableTags = { "Tree", "Building", "Monster", "MonsterHand", "MonsterFoot" }; // 줄을 걸 수 있는 태그 배열 (새로 추가 또는 수정)

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
    // 이 변수들은 이제 자세 제어 로직이 제거되므로 필요 없을 수 있습니다.
    // 하지만 Start()에서 null 체크를 위해 남겨두거나, 다른 용도로 사용될 수 있다면 유지합니다.
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

    // --- 새로운 헬퍼 함수 추가 (이 부분이 클래스 중괄호 안에, Update 함수 밖에 있어야 함) ---ws
    bool IsGrappleableTarget(Collider collider)
    {
        if (collider == null) return false;

        foreach (string tag in grappleableTags)
        {
            if (collider.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
}

    void Update()
    {
        // --- 조준점 색상 변경 로직 ---
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, ~ignoreLayers); 
// 충돌한 오브젝트가 줄을 걸 수 있는 대상인지 확인하는 헬퍼 함수 호출
    bool canGrappleTarget = IsGrappleableTarget(hit.collider); // 이 라인은 Update 함수 내부에 있어야 합니다.

    if (crosshairImage != null)
    {
        if (hitSomething && canGrappleTarget) // hit.collider.CompareTag("Tree") 대신 canGrappleTarget 사용
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
        if (hitSomething && canGrappleTarget) // hit.collider.CompareTag("Tree") 대신 canGrappleTarget 사용
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
        if (hitSomething && canGrappleTarget) // hit.collider.CompareTag("Tree") 대신 canGrappleTarget 사용
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
        bool isGrapplingActive = (isLeftGrappling || isRightGrappling);
        bool isHoldingSpaceWhileGrappling = isGrapplingActive && Input.GetKey(KeyCode.Space);


        if (!isHoldingSpaceWhileGrappling) 
        {
            float horizontalInput = Input.GetAxis("Horizontal"); 
            float verticalInput = Input.GetAxis("Vertical");   

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
            
            Vector3 moveInput = cameraRightFlat * horizontalInput + cameraForwardFlat * verticalInput;
            if (moveInput.magnitude > 0.1f) moveInput.Normalize(); 
            
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
            
        } 


        // --- 비행 및 가스 이펙트 로직 (스페이스바) ---
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
            else 
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
            
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                Vector3 burstDirection = (directionToTarget * burstAccelerationForce) + (Vector3.up * burstUpwardForce);
                playerRigidbody.AddForce(burstDirection, ForceMode.Impulse);

                if (gasJetEffect != null)
                {
                    gasJetEffect.Play();
                }
            }

            if (playerRigidbody.velocity.magnitude > maxGrappleSpeed)
            {
                playerRigidbody.velocity = playerRigidbody.velocity.normalized * maxGrappleSpeed;
            }
        }
        
        // --- 가스 이펙트 활성화/비활성화 (지속적인 분출) ---
        if (gasJetEffect != null)
        {
            if (isGrapplingActive) 
            {
                if (!gasJetEffect.isPlaying)
                {
                    gasJetEffect.Play();
                }
            }
            else 
            {
                if (gasJetEffect.isPlaying)
                {
                    gasJetEffect.Stop();
                }
            }
        }

        // --- 바람 효과 활성화/비활성화 (지속적인 분출) ---
        if (windEffect != null)
        {
            if (isHoldingSpaceWhileGrappling) 
            {
                if (!windEffect.isPlaying)
                {
                    windEffect.Play();
                }
            }
            else 
            {
                if (windEffect.isPlaying)
                {
                    windEffect.Stop();
                }
            }
        }

        // --- 카메라 FOV 조절 로직 ---
        if (playerCamera != null)
        {
            float targetFOV = isHoldingSpaceWhileGrappling ? grapplignFOV : normalFOV; 
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
        }

        // --- 플레이어 모델 자세 제어 (젖히는 느낌) ---
        // 이 블록은 isHoldingSpaceWhileGrappling 상태에 따라 모델 자세를 변경합니다.
        if (playerModel != null)
        {
            if (isHoldingSpaceWhileGrappling)
            {
                // 항상 뒤로 젖히는 각도
                float targetPitch = -maxTiltAngle; 
                float targetRoll = 0f; // 옆으로 젖히는 각도는 현재 로직에서 제외됨

                // 이전에 'targetRoll'을 계산하던 로직 (Rigidbody.velocity 기반)은 제거되었으므로,
                // 이제 옆으로 젖히는 효과를 원하지 않는다면 이 변수들은 모두 0이 됩니다.
                // 만약 다시 옆으로 젖히는 효과가 필요하다면 해당 로직을 다시 추가해야 합니다.
                
                Quaternion targetTiltRotation = Quaternion.Euler(targetPitch, 0, targetRoll); 
                playerModel.localRotation = Quaternion.Slerp(playerModel.localRotation, targetTiltRotation, tiltSpeed * Time.deltaTime);
            }
            else
            {
                Quaternion targetNormalRotation = Quaternion.identity; 
                playerModel.localRotation = Quaternion.Slerp(playerModel.localRotation, targetNormalRotation, tiltSpeed * Time.deltaTime);
            }
        }
    }
}