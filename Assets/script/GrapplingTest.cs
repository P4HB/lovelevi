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
    public float grappleForce = 20f;  
    public float burstAccelerationForce = 30f; 
    public float burstUpwardForce = 15f; 
    public float maxGrappleSpeed = 20f;

    // === 가스 분사 이펙트 관련 변수 ===
    public ParticleSystem gasJetEffect;

    // === 커브 비행 관련 변수 ===
    public float curveForce = 10f;

    // === 레이캐스트 필터링 변수 ===
    public LayerMask ignoreLayers; 

    // === WASD 이동 및 회전 관련 변수 ===
    public float moveSpeed = 5f; 
    public float rotationSpeed = 10f; 

    private Quaternion targetPlayerRotation; 

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
            Debug.LogWarning("GrapplingTest: Gas Jet Effect Particle System이 연결되지 않았습니다. 인스펙터에서 연결해주세요.");
        }
        else
        {
            gasJetEffect.Stop();
        }

        targetPlayerRotation = transform.rotation;
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
                leftLine.positionCount = 2;
            }
            else
            {
                isLeftGrappling = false;
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
                rightLine.positionCount = 2;
            }
            else
            {
                isRightGrappling = false;
                rightLine.positionCount = 0;
            }
        }
        
        // --- 라인 렌더링 ---
        if (isLeftGrappling)
        {
            leftLine.SetPosition(0, leftGrappleStartPoint.position);
            leftLine.SetPosition(1, leftGrapplePoint);
        }
        else
        {
            if (leftLine.positionCount > 0) leftLine.positionCount = 0;
        }

        if (isRightGrappling)
        {
            rightLine.SetPosition(0, rightGrappleStartPoint.position);
            rightLine.SetPosition(1, rightGrapplePoint);
        }
        else
        {
            if (rightLine.positionCount > 0) rightLine.positionCount = 0;
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
        if (!((isLeftGrappling || isRightGrappling) && Input.GetKey(KeyCode.Space)))
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
            // 대각선 입력 처리 (우선순위 높음)
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
                // W에서 S로 전환 시 뒤도는 느낌을 위해 현재 전방에서 후방으로 회전
                targetPlayerRotation = Quaternion.LookRotation(-cameraForwardFlat);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                targetPlayerRotation = Quaternion.LookRotation(-cameraRightFlat); // 카메라 기준 왼쪽
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                targetPlayerRotation = Quaternion.LookRotation(cameraRightFlat); // 카메라 기준 오른쪽
            }
            
            // 플레이어 Rigidbody 회전: 목표 회전값을 향해 부드럽게 보간
            playerRigidbody.MoveRotation(Quaternion.Slerp(playerRigidbody.rotation, targetPlayerRotation, rotationSpeed * Time.deltaTime));

            // 플레이어 Rigidbody 이동
            if (moveInput.magnitude > 0.1f) 
            {
                playerRigidbody.MovePosition(playerRigidbody.position + moveInput * moveSpeed * Time.deltaTime);
            }
            
        } // WASD 이동/회전 로직 끝


        // --- 비행 및 가스 이펙트 로직 (스페이스바) ---
        if ((isLeftGrappling || isRightGrappling) && Input.GetKey(KeyCode.Space))
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

            // --- 커브 비행 로직 ---
            Vector3 playerForwardFlat = transform.forward;
            playerForwardFlat.y = 0;
            if (playerForwardFlat.magnitude > 0.01f) playerForwardFlat.Normalize();

            Vector3 relativeTarget = targetGrapplePoint - transform.position;
            Vector3 crossProd = Vector3.Cross(playerForwardFlat, relativeTarget.normalized);

            Vector3 curveDirection = Vector3.zero;
            if (crossProd.y > 0.1f)
            {
                curveDirection = -transform.right; 
            }
            else if (crossProd.y < -0.1f)
            {
                curveDirection = transform.right; 
            }

            if (curveDirection != Vector3.zero)
            {
                playerRigidbody.AddForce(curveDirection.normalized * curveForce, ForceMode.Acceleration);
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
        else 
        {
            if (gasJetEffect != null && gasJetEffect.isPlaying)
            {
                gasJetEffect.Stop(); 
            }
        }
    }
}