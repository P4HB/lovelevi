using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    public enum State { IDLE, TRACE, ATTACK, DIE }
    public State state = State.IDLE;

    public float traceDist = 1000.0f;
    public float attackDist = 20.0f;
    public bool isDie = false;

    // === "X" 표시 관련 변수 ===
    [Header("UI & Effects")]
    public GameObject targetXMarkObject; 
    public float xMarkDisplayDistance = 20f; 

    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
    private Animator anim;

    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack"); // 오타 수정: int 대신 hashAttack
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashDie = Animator.StringToHash("Die");

    private GameObject bloodEffect;
    private int hp = 100; // CS0414 경고는 이 변수가 사용되지 않아서 뜨는 것이므로, 주석 처리하거나 사용하면 사라집니다.

    // void OnEnable()
    // {
    //     PlayerCtrl.OnPlayerDie += this.OnPlayerDie;
    // }

    // void OnDisable()
    // {
    //     PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
    // }

    void Start()
    {
        monsterTr = transform;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); 
        if (playerObj != null)
        {
            playerTr = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player 오브젝트를 찾을 수 없습니다. 'PLAYER' 태그 확인!");
        }

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // bloodEffect = Resources.Load<GameObject>("BloodSprayEffect"); // 사용하려면 주석 해제

        StartCoroutine(CheckMonsterState());
        StartCoroutine(MonsterAction());

        if (targetXMarkObject == null)
        {
            Debug.LogWarning("MonsterCtrl: targetXMarkObject가 연결되지 않았습니다. 인스펙터에 연결해주세요.");
        }
    }

    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.3f);

            if (state == State.DIE) yield break;

            if (playerTr == null) 
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTr = playerObj.transform;
                    Debug.Log("[MonsterCtrl] Player Transform Found via Tag.");
                }
                else
                {
                    Debug.LogWarning("MonsterCtrl: Player 오브젝트를 찾을 수 없어 거리 계산을 건너뜁니다. 'Player' 태그 확인 필요.");
                    continue;
                }
            }

            float distance = Vector3.Distance(playerTr.position, monsterTr.position);

            // "X" 표시 활성화/비활성화 로직
            if (targetXMarkObject != null)
            {
                if (distance <= xMarkDisplayDistance && !isDie) 
                {
                    if (!targetXMarkObject.activeSelf) 
                    {
                        targetXMarkObject.SetActive(true);
                    }
                }
                else 
                {
                    if (targetXMarkObject.activeSelf) 
                    {
                        targetXMarkObject.SetActive(false);
                    }
                }
            }

            // 몬스터 상태 변경 로직
            if (!isDie) 
            {
                if (distance <= attackDist) state = State.ATTACK;
                else if (distance <= traceDist) state = State.TRACE;
                else state = State.IDLE;
            }
        }
    }

    IEnumerator MonsterAction()
    {
        while (!isDie) // isDie가 true가 되면 코루틴 종료
        {
            switch (state)
            {
                case State.IDLE:
                    if (agent != null && agent.isActiveAndEnabled)
                        agent.isStopped = true;

                    anim.SetBool(hashTrace, false);
                    anim.SetBool(hashAttack, false);
                    break;

                case State.TRACE:
                    if (agent != null && agent.isActiveAndEnabled && playerTr != null)
                    {
                        if (agent.isOnNavMesh)
                        {
                            agent.SetDestination(playerTr.position);
                            agent.isStopped = false;
                        }
                        else 
                        {
                            agent.isStopped = true; 
                        }
                    }
                    else if (agent != null) 
                    {
                        agent.isStopped = true;
                    }

                    anim.SetBool(hashTrace, true);
                    anim.SetBool(hashAttack, false);
                    break;

                case State.ATTACK:
                    if (agent != null && agent.isActiveAndEnabled)
                        agent.isStopped = true;

                    anim.SetBool(hashTrace, false);
                    anim.SetBool(hashAttack, true);
                    break;

                case State.DIE: // 몬스터가 죽음 상태가 되었을 때
                    isDie = true; 

                    if (agent != null && agent.isActiveAndEnabled)
                        agent.isStopped = true; // NavMeshAgent 정지

                    // anim.SetTrigger(hashDie); // 죽음 애니메이션 트리거

                    anim.SetBool(hashDie, true);
                    // === 여기를 수정합니다: 애니메이션 이벤트 대신 코루틴으로 호출 ===
                    // 현재 재생될 (또는 전환될) Die 애니메이션 클립의 길이를 가져옵니다.
                    // 정확한 길이를 얻기 위해 약간의 지연을 줄 수 있습니다.
                    yield return new WaitForEndOfFrame(); // 다음 프레임까지 기다려 애니메이터 상태가 업데이트되도록 함
                    float dieAnimationLength = anim.GetCurrentAnimatorStateInfo(0).length;
                    StartCoroutine(ExecuteOnDeathAnimationEndAfterDelay(dieAnimationLength));
                    // =============================================================
                    
                    yield break; // MonsterAction 코루틴 종료
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    // --- OnDeathAnimationEnd() 함수 (이전과 동일) ---
    // 이 함수는 이제 애니메이션 이벤트 대신 코루틴으로 호출됩니다.
    public void OnDeathAnimationEnd()
    {
        Debug.Log("[MonsterCtrl] OnDeathAnimationEnd() - Death animation ended. Deactivating Monster.");
        
        // 몬스터의 모든 콜라이더 비활성화
        Collider[] allColliders = GetComponentsInChildren<Collider>(); 
        foreach (Collider col in allColliders)
        {
            col.enabled = false;
        }

        // Rigidbody를 Is Kinematic = true로 설정하여 물리 엔진에서 제거
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero; 
        }
        
        // NavMeshAgent가 있다면 비활성화
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.enabled = false;
        }

        // X 마크 오브젝트도 비활성화
        if (targetXMarkObject != null)
        {
            targetXMarkObject.SetActive(false);
        }

        // 몬스터 오브젝트 자체 비활성화 (가장 마지막)
        gameObject.SetActive(false); 

        // 몬스터가 죽었을 때 추가적인 게임 로직 (예: 점수 부여 등)
        // if (GameManager.Instance != null)
        // {
        //     GameManager.Instance.AddScore(100);
        // }
    }

    // === 새로 추가된 코루틴 함수: 일정 시간 후 OnDeathAnimationEnd 호출 ===
    private IEnumerator ExecuteOnDeathAnimationEndAfterDelay(float animationLength)
    {
        // 애니메이션이 재생될 시간을 기다립니다.
        // 죽음 애니메이션 길이에 약간의 여유 시간을 추가하여 애니메이션이 완전히 끝난 후 호출되도록 합니다.
        yield return new WaitForSeconds(animationLength + 0.1f); // 0.1초는 여유 시간, 필요에 따라 조절

        // OnDeathAnimationEnd 함수 호출
        OnDeathAnimationEnd(); 
    }
    public void Death(){
        if (isDie) return;

        state = State.DIE;
        isDie = true;
         StartCoroutine(ExecuteDeathNow()); // 바로 죽음 로직 실행
    }
    private IEnumerator ExecuteDeathNow()
    {
        // NavMesh 정지
        if (agent != null && agent.isActiveAndEnabled)
            agent.isStopped = true;
                Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true; // 죽어도 충돌 유지

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        anim.SetTrigger("Die");

        yield return new WaitForEndOfFrame(); // 애니메이터 상태 업데이트 기다림
        float dieAnimationLength = anim.GetCurrentAnimatorStateInfo(0).length;
        StartCoroutine(ExecuteOnDeathAnimationEndAfterDelay(dieAnimationLength));
    }
    public float DistanceToPlayer
    {
        get
        {
            if (playerTr == null) return float.MaxValue;
            return Vector3.Distance(transform.position, playerTr.position);
        }
    }
    // --- OnCollisionEnter 함수 (기존) ---
    void OnPlayerDie()
    {
        StopAllCoroutines();
        if (agent != null) 
            agent.isStopped = true;
        anim.SetFloat(hashSpeed, Random.Range(0.8f, 1.2f));
        anim.SetTrigger(hashPlayerDie);
    }
}