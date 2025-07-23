using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossMonsterCtrl : MonoBehaviour
{
    // 몬스터의 상태 정의
    public enum State { IDLE, TRACE, ATTACK, DIE }
    public State state = State.IDLE;

    // 기본 추적 및 공격 설정
    public float traceDist = 200.0f;
    public float attackDist = 20.0f;
    public bool isDie = false;
    // 보스 몬스터 전용 스킬 설정
    [Header("Boss Skill Settings")]
    public float burstSpeed = 20.0f;           // 폭주 시 속도
    public float burstDuration = 2.0f;         // 폭주 지속 시간 (2초)
    public float burstCooldown = 5.0f;         // 폭주 쿨타임 (5초)
    public float attackDelay = 2.5f;           // 공격 후 딜레이

    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
    private Animator anim;

    // 애니메이터 파라미터 해시값 미리 추출
    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    // private readonly int hashAttack = Animator.StringToHash("IsAttack"); // 더 이상 사용하지 않음
    private readonly int hashPunch = Animator.StringToHash("Punch");        // 펀치 공격 트리거
    private readonly int hashKick = Animator.StringToHash("Kick");          // 킥 공격 트리거
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashDie = Animator.StringToHash("Die");

    private float normalSpeed; // 몬스터의 평상시 속도
    private bool isAttacking = false; // 공격 중인지 확인하는 플래그

    void Start()
    {
        monsterTr = transform;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        // 플레이어 오브젝트 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTr = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다. 'Player' 태그를 확인해주세요!");
        }
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // NavMeshAgent의 초기 속도를 normalSpeed에 저장
        normalSpeed = agent.speed;

        // 3개의 코루틴을 동시에 실행
        StartCoroutine(CheckMonsterState()); // 몬스터 상태 체크
        StartCoroutine(MonsterAction());     // 상태에 따른 행동 처리
        StartCoroutine(SpeedBurstRoutine()); // 보스 스킬 (속도 폭주) 처리
    }

    // 몬스터의 상태를 주기적으로 체크하는 코루틴
    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            // 0.3초마다 상태 체크
            yield return new WaitForSeconds(0.3f);

            // 몬스터가 죽었거나 플레이어가 없으면 중단
            if (state == State.DIE || playerTr == null) yield break;

            // 플레이어와의 거리 계산
            float distance = Vector3.Distance(playerTr.position, monsterTr.position);

            // 공격 가능 거리이고, 현재 다른 공격이 진행중이 아닐 때 ATTACK 상태로 변경
            if (distance <= attackDist && !isAttacking)
            {
                state = State.ATTACK;
            }
            // 추적 가능 거리일 때 TRACE 상태로 변경
            else if (distance <= traceDist)
            {
                state = State.TRACE;
            }
            // 그 외에는 IDLE 상태
            else
            {
                state = State.IDLE;
            }
        }
    }

    // 몬스터의 상태에 따라 실제 행동을 처리하는 코루틴
    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            // 상태에 따라 분기 처리
            switch (state)
            {
                case State.IDLE:
                    // 추적 중지
                    if (agent.isActiveAndEnabled) agent.isStopped = true;
                    anim.SetBool(hashTrace, false);
                    break;

                case State.TRACE:
                    // 플레이어 위치로 추적 재개
                    if (agent.isActiveAndEnabled && playerTr != null && agent.isOnNavMesh)
                    {
                        agent.SetDestination(playerTr.position);
                        agent.isStopped = false;
                    }
                    anim.SetBool(hashTrace, true);
                    break;

                case State.ATTACK:
                    // 공격 중 플래그 설정
                    isAttacking = true;

                    // 추적 중지
                    if (agent.isActiveAndEnabled) agent.isStopped = true;
                    
                    // 공격 직전에 플레이어를 바라보게 함
                    transform.LookAt(playerTr.position);

                    // 0 또는 1의 랜덤 숫자를 뽑아 펀치 또는 킥 공격 실행
                    if (Random.Range(0, 2) == 0)
                    {
                        anim.SetTrigger(hashPunch);
                        Debug.Log("Boss used PUNCH!");
                    }
                    else
                    {
                        anim.SetTrigger(hashKick);
                        Debug.Log("Boss used KICK!");
                    }

                    // 공격 애니메이션이 끝날 때까지 대기 (공격 딜레이)
                    yield return new WaitForSeconds(attackDelay);
                    
                    // 공격이 끝났으므로 플래그를 해제하고 다시 IDLE 상태로 돌아가 거리 체크
                    isAttacking = false;
                    state = State.IDLE; // 공격 후 잠시 대기 상태로
                    anim.SetBool(hashTrace, false); // 추적 애니메이션도 꺼줌
                    break;

                case State.DIE:
                    isDie = true;
                    if (agent.isActiveAndEnabled) agent.isStopped = true;
                    anim.SetTrigger(hashDie);
                    GetComponent<Collider>().enabled = false;
                    // 죽으면 모든 코루틴 종료
                    StopAllCoroutines();
                    break;
            }

            // 0.3초 대기 후 다음 행동 체크
            yield return new WaitForSeconds(0.3f);
        }
    }

    // [NEW] 5초마다 2초간 속도를 폭발적으로 증가시키는 코루틴
    IEnumerator SpeedBurstRoutine()
    {
        // 몬스터가 죽지 않은 동안 계속 반복
        while (!isDie)
        {
            // 쿨타임(5초)만큼 대기
            yield return new WaitForSeconds(burstCooldown);

            // 추적 상태일 때만 속도 폭주 발동
            if (state == State.TRACE)
            {
                Debug.Log("SPEED BURST START!");
                agent.speed = burstSpeed;       // 속도를 burstSpeed로 변경
                // 여기에 이펙트나 사운드를 추가하면 더 좋습니다.

                // 폭주 지속 시간(2초)만큼 대기
                yield return new WaitForSeconds(burstDuration);

                Debug.Log("SPEED BURST END!");
                agent.speed = normalSpeed;      // 속도를 원래대로 복구
            }
        }
    }
    public IEnumerator InstantDeath(){
        // 죽음 애니메이션이 끝날 때까지 대기
        if (agent != null && agent.isActiveAndEnabled)
            agent.isStopped = true;

        anim.SetTrigger("Die");

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        // 몬스터 비활성화
        gameObject.SetActive(false);
    }
    // 플레이어가 죽었을 때 호출될 함수 (이벤트 기반)
    void OnPlayerDie()
    {
        StopAllCoroutines();
        if(agent.isActiveAndEnabled) agent.isStopped = true;
        anim.SetTrigger(hashPlayerDie);
    }
}