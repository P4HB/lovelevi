using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 필수 컴포넌트를 명시합니다.
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // === 애니메이션 관련 변수 ===
    private Animator animator;

    // === 갈고리 관련 변수 ===
    public Transform leftGrappleStartPoint;
    public LineRenderer leftLine;
    private bool isLeftGrappling = false; // 갈고리가 '연결'되었는지 여부
    private Vector3 leftGrapplePoint;

    public Transform rightGrappleStartPoint;
    public LineRenderer rightLine;
    private bool isRightGrappling = false; // 갈고리가 '연결'되었는지 여부
    private Vector3 rightGrapplePoint;

    // === 조준점 관련 변수 ===
    public Image crosshairImage;
    public Color defaultCrosshairColor = Color.white;
    public Color grappleableCrosshairColor = Color.green;

    // === 비행 및 이동 관련 변수 ===
    public Rigidbody playerRigidbody;
    public float grappleForce = 40f;
    public float burstAccelerationForce = 40f;
    public float burstUpwardForce = 10f;
    public float maxGrappleSpeed = 50f;

    // === 가스 분사 이펙트 관련 변수 ===
    public ParticleSystem gasJetEffect;

    // === 커브 비행 관련 변수 ===
    public float curveForce = 10f;
    public float adKeyCurveMultiplier = 20f;
    public float maxSideForce = 10f;

    // === 레이캐스트 필터링 변수 ===
    public LayerMask ignoreLayers; 
    // 줄을 걸 수 있는 태그 배열에 "Building"과 "Monster" 관련 태그들을 모두 포함
    public string[] grappleableTags = { "Tree", "Building", "Monster", "MonsterHand", "MonsterFoot" }; // <<< 이 배열에 "Building" 추가

    // === WASD 이동 및 회전 관련 변수 ===
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
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

    [Header("Ground Check")]
    public float groundCheckDistance = 3f; // 플레이어 발밑에서 얼마나 아래까지 바닥을 감지할지 거리
    public LayerMask Grappleable;            // 바닥으로 인식할 오브젝트들의 레이어 (이것은 줄 걸기와는 별개)
    private bool isGrounded = true;          // 플레이어가 땅에 닿아있는지 여부

    // 공격 관련 변수
    private GameObject currentTargetTitan;
    public float attackRange = 20.0f;
    //ui 관련 변수
    public GameObject titanPromptTextUI;

    [Header("Player Audio")] // PlayerController에 직접 오디오 변수 추가
    public AudioSource playerAudioSource; // 공격 사운드를 재생할 AudioSource
    public AudioClip[] attackSounds; // 3개의 공격 사운드 클립 배열
    public float attackSoundDelay = 0.1f; // <<< 공격 사운드 딜레이

    void Start()
    {
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerRigidbody == null) playerRigidbody = GetComponent<Rigidbody>();
        if (gasJetEffect != null) gasJetEffect.Stop();
        targetPlayerRotation = transform.rotation;
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera != null) playerCamera.fieldOfView = normalFOV;
        if (windEffect != null) windEffect.Stop();

        // AudioSource 컴포넌트가 Inspector에 연결되지 않았다면 자동으로 찾기 (안전 장치)
        if (playerAudioSource == null)
        {
            playerAudioSource = GetComponent<AudioSource>();
            if (playerAudioSource == null)
            {
                // 플레이어 오브젝트에 AudioSource가 없다면 추가 (공격 사운드 전용)
                playerAudioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("PlayerController: Added AudioSource component for attack sounds.");
                playerAudioSource.playOnAwake = false;
                playerAudioSource.loop = false;
                playerAudioSource.spatialBlend = 0; // 2D Sound
            }
        }
        else
        {
             // 이미 연결된 AudioSource의 설정 확인
             playerAudioSource.playOnAwake = false;
             playerAudioSource.loop = false;
             playerAudioSource.spatialBlend = 0; // 2D Sound
        }
    }

    void Update()
    {
        checkIfGrounded();
        // --- 조준점 및 레이캐스트 ---
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)); 
        RaycastHit hit;
        // Physics.Raycast에 ignoreLayers를 사용합니다.
        // 그리고 canGrapple 로직을 IsGrappleableTarget 함수로 변경합니다.
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, ~ignoreLayers);
        bool canGrappleTarget = IsGrappleableTarget(hit.collider); // <<< 이 함수를 사용하여 태그 확인

        if (crosshairImage != null)
        {
            // hitSomething && canGrappleTarget을 사용하여 조준점 색상 결정
            crosshairImage.color = hitSomething && canGrappleTarget ? grappleableCrosshairColor : defaultCrosshairColor;
        }
        UpdateNearestTitan();
        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Attack");
            MonsterCtrl titan = currentTargetTitan.GetComponent<MonsterCtrl>();
            if (titan != null && !titan.isDie && titan.DistanceToPlayer <= titan.xMarkDisplayDistance)
            {
                titan.Death(); // or titan.Die();
                if (titanPromptTextUI != null)
                    titanPromptTextUI.SetActive(false);
            }
            GameObject boss = GameObject.FindWithTag("Boss"); // Boss 태그 필수!
            if (boss != null)
            {
                float dist = Vector3.Distance(transform.position, boss.transform.position);
                if (dist <= 20f) // 공격 범위 제한
                {
                    BossHealth BossHealth = boss.GetComponent<BossHealth>();
                    if (BossHealth != null)
                    {
                        BossHealth.TakeDamage(20); // 데미지 20 주기
                    }
                }
            }

            // 무작위 공격 사운드 재생 (코루틴으로 딜레이 적용)
            if (playerAudioSource != null && attackSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, attackSounds.Length);
                AudioClip clipToPlay = attackSounds[randomIndex];
                
                // === 코루틴 시작: 딜레이 후 사운드 재생 ===
                StartCoroutine(PlayAttackSoundWithDelay(clipToPlay, attackSoundDelay));
                Debug.Log($"[PlayerController] Attack animation triggered. Sound '{clipToPlay.name}' will play after {attackSoundDelay:F2}s.");
            }
            else
            {
                if(playerAudioSource == null) Debug.LogWarning("PlayerController: playerAudioSource가 연결되지 않았습니다.");
                if(attackSounds.Length == 0) Debug.LogWarning("PlayerController: attackSounds 배열이 비어 있습니다. 공격 사운드를 재생할 수 없습니다.");
            }
        }
        // --- 로직 변경: 갈고리 '발사' 로직 (마우스 클릭) ---
        if (Input.GetMouseButtonDown(0)) // 왼쪽 클릭으로 왼쪽 갈고리 발사/연결
        {
            if (hitSomething && canGrappleTarget) // hitSomething && canGrappleTarget 사용
            {
                leftGrapplePoint = hit.point;
                isLeftGrappling = true;
                if (leftLine != null) leftLine.positionCount = 2;
            }
        }
        if (Input.GetMouseButtonDown(1)) // 오른쪽 클릭으로 오른쪽 갈고리 발사/연결
        {
            if (hitSomething && canGrappleTarget) // hitSomething && canGrappleTarget 사용
            {
                rightGrapplePoint = hit.point;
                isRightGrappling = true;
                if (rightLine != null) rightLine.positionCount = 2;
            }
        }

        // --- 로직 변경: 갈고리 '해제' 로직 (마우스 버튼 떼기) ---
        if (Input.GetMouseButtonUp(0))
        {
            isLeftGrappling = false;
            if (leftLine != null) leftLine.positionCount = 0;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRightGrappling = false;
            if (rightLine != null) rightLine.positionCount = 0;
        }

        // --- 라인 렌더링 (연결 상태에 따라) ---
        if (isLeftGrappling && leftLine != null)
        {
            leftLine.SetPosition(0, leftGrappleStartPoint.position);
            leftLine.SetPosition(1, leftGrapplePoint);
        }
        if (isRightGrappling && rightLine != null)
        {
            rightLine.SetPosition(0, rightGrappleStartPoint.position);
            rightLine.SetPosition(1, rightGrapplePoint);
        }

        // --- 로직 변경: '비행(당기기)' 상태 결정 ---
        // 왼쪽 갈고리가 연결된 상태에서 Ctrl 키를 누르고 있는가?
        bool isPullingLeft = isLeftGrappling && Input.GetKey(KeyCode.LeftControl);
        // 오른쪽 갈고리가 연결된 상태에서 Space 키를 누르고 있는가?
        bool isPullingRight = isRightGrappling && Input.GetKey(KeyCode.Space);
        // 둘 중 하나라도 당기고 있다면 '비행' 상태이다.
        bool isFlying = isPullingLeft || isPullingRight;

        animator.SetBool("isFlying", isFlying);
        
        // --- 이동 로직 분기 ---
        if (!isFlying)
        {
            // 비행(당기기) 중이 아닐 때: 지상 이동 로직
            animator.SetBool("isRunning", HandleGroundMovement());
        }
        else
        {
            // 비행(당기기) 중일 때: 비행 로직
            animator.SetBool("isRunning", false);
            HandleFlyingMovement(isPullingLeft, isPullingRight);
        }

        // --- 시각 효과 업데이트 (isFlying 상태에 따라) ---
        UpdateEffects(isFlying);
        UpdateCameraFOV(isFlying);
        UpdatePlayerModelTilt(isFlying);
    }

    // === 새로운 코루틴 함수: 딜레이 후 공격 사운드 재생 ===
    private IEnumerator PlayAttackSoundWithDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기

        if (playerAudioSource != null && clip != null)
        {
            playerAudioSource.PlayOneShot(clip); // 사운드 재생
            Debug.Log($"[PlayerController] Played attack sound: {clip.name} after delay.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TitanNeck"))
        {
            currentTargetTitan = other.GetComponentInParent<MonsterCtrl>()?.gameObject;
            if (titanPromptTextUI != null)
                titanPromptTextUI.SetActive(true); // UI 표시
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TitanNeck"))
        {
            currentTargetTitan = null;
            
            if (titanPromptTextUI != null)
                titanPromptTextUI.SetActive(false); // UI 숨김
        }
    }
    private bool HandleGroundMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveInput = (Camera.main.transform.right * horizontalInput + Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized * verticalInput).normalized;

        if (moveInput.magnitude > 0.1f)
        {
            targetPlayerRotation = Quaternion.LookRotation(moveInput);
            playerRigidbody.MoveRotation(Quaternion.Slerp(playerRigidbody.rotation, targetPlayerRotation, rotationSpeed * Time.deltaTime));
            playerRigidbody.MovePosition(playerRigidbody.position + moveInput * moveSpeed * Time.deltaTime);
            return true;
        }
        return false;
    }

    // --- 로직 변경: 함수가 당기는 방향을 인자로 받도록 수정 ---
    private void HandleFlyingMovement(bool pullingLeft, bool pullingRight)
    {
        if (playerRigidbody == null) return;
        
        // --- 지속적인 당기는 힘 계산 ---
        Vector3 totalForceDirection = Vector3.zero;
        if (pullingLeft)
        {
            totalForceDirection += (leftGrapplePoint - transform.position).normalized;
        }
        if (pullingRight)
        {
            totalForceDirection += (rightGrapplePoint - transform.position).normalized;
        }
        playerRigidbody.AddForce(totalForceDirection.normalized * grappleForce, ForceMode.Acceleration);

        // --- 순간 가속 (Burst) 로직 ---
        if (isLeftGrappling && Input.GetKeyDown(KeyCode.LeftControl))
        {
            Vector3 burstDirection = ((leftGrapplePoint - transform.position).normalized * burstAccelerationForce) + (Vector3.up * burstUpwardForce);
            playerRigidbody.AddForce(burstDirection, ForceMode.Impulse);
        }
        if (isRightGrappling && Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 burstDirection = ((rightGrapplePoint - transform.position).normalized * burstAccelerationForce) + (Vector3.up * burstUpwardForce);
            playerRigidbody.AddForce(burstDirection, ForceMode.Impulse);
        }

        // --- 회전 로직 추가: 갈고리 줄과 몸을 정렬하려는 회전력 ---
        Vector3 totalTorque = Vector3.zero;
        float rotationCorrectionForce = 50.0f; // 회전 보정 힘의 강도. 이 값을 조절하여 회전 속도를 바꿀 수 있습니다.

        if (pullingLeft)
        {
            // 플레이어 중심에서 갈고리 지점까지의 이상적인 방향
            Vector3 idealDirection = (leftGrapplePoint - transform.position).normalized;
            // 실제 갈고리 줄의 현재 방향
            Vector3 currentLineDirection = (leftGrapplePoint - leftGrappleStartPoint.position).normalized;
            
            // 두 벡터의 외적을 통해 회전 축을 구함. 이 축을 중심으로 회전해야 두 벡터가 정렬됨.
            Vector3 rotationAxis = Vector3.Cross(currentLineDirection, idealDirection);
            // 두 벡터 사이의 각도를 구해서 회전력의 크기로 사용. 각도가 클수록 더 강하게 회전.
            float angleDifference = Vector3.Angle(currentLineDirection, idealDirection);

            // 계산된 회전력 추가
            totalTorque += rotationAxis * angleDifference * rotationCorrectionForce;
        }

        if (pullingRight)
        {
            // 오른쪽 갈고리에 대해서도 동일한 로직 적용
            Vector3 idealDirection = (rightGrapplePoint - transform.position).normalized;
            Vector3 currentLineDirection = (rightGrapplePoint - rightGrappleStartPoint.position).normalized;
            Vector3 rotationAxis = Vector3.Cross(currentLineDirection, idealDirection);
            float angleDifference = Vector3.Angle(currentLineDirection, idealDirection);
            
            totalTorque += rotationAxis * angleDifference * rotationCorrectionForce;
        }

        // 계산된 총 회전력을 리지드바디에 적용
        playerRigidbody.AddTorque(totalTorque, ForceMode.Acceleration);
        // --- 회전 로직 종료 ---


        // --- 커브 비행 로직 (A/D 키) ---
        float horizontalInputGrapple = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontalInputGrapple) > 0.1f)
        {
            Vector3 sideForce = playerCamera.transform.right * horizontalInputGrapple * adKeyCurveMultiplier;
            playerRigidbody.AddForce(Vector3.ClampMagnitude(sideForce, maxSideForce), ForceMode.Acceleration);
        }

        // --- 속도 제한 ---
        if (playerRigidbody.velocity.magnitude > maxGrappleSpeed)
        {
            playerRigidbody.velocity = playerRigidbody.velocity.normalized * maxGrappleSpeed;
        }
    }

    private void UpdateEffects(bool isCurrentlyFlying)
    {
        if (gasJetEffect != null)
        {
            if (isCurrentlyFlying && !gasJetEffect.isPlaying) gasJetEffect.Play();
            else if (!isCurrentlyFlying && gasJetEffect.isPlaying) gasJetEffect.Stop();
        }
        if (windEffect != null)
        {
            if (isCurrentlyFlying && !windEffect.isPlaying) windEffect.Play();
            else if (!isCurrentlyFlying && windEffect.isPlaying) windEffect.Stop();
        }
    }

    private void UpdateCameraFOV(bool isCurrentlyFlying)
    {
        if (playerCamera != null)
        {
            float targetFOV = isCurrentlyFlying ? grapplignFOV : normalFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
        }
    }

    private void UpdatePlayerModelTilt(bool isCurrentlyFlying)
    {
        if (playerModel != null)
        {
            Quaternion targetRotation = isCurrentlyFlying ? Quaternion.Euler(-maxTiltAngle, 0, 0) : Quaternion.identity;
            playerModel.localRotation = Quaternion.Slerp(playerModel.localRotation, targetRotation, tiltSpeed * Time.deltaTime);
        }
    }
    private void checkIfGrounded()
    {
        // 플레이어의 발밑에서 아래로 레이캐스트를 쏘아 바닥을 감지합니다.
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, Grappleable);
        animator.SetBool("isGrounded", isGrounded);
    }
    // --- 새로운 헬퍼 함수 추가 (클래스 내부, Update 함수 밖) ---
    // 이 함수가 있어야 grappleableTags 배열을 사용할 수 있습니다.
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
    private void UpdateNearestTitan()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestTitan = null;

        // "Monster" 태그가 붙은 모든 오브젝트를 찾음
        GameObject[] allTitans = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject titan in allTitans)
        {
            float distance = Vector3.Distance(transform.position, titan.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTitan = titan;
            }
        }

        if (closestTitan != null)
        {
            currentTargetTitan = closestTitan;
        }
    }
}