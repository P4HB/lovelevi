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

    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
    private Animator anim;

    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashDie = Animator.StringToHash("Die");

    private GameObject bloodEffect;
    private int hp = 100;

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

        // bloodEffect = Resources.Load<GameObject>("BloodSprayEffect");

        StartCoroutine(CheckMonsterState());
        StartCoroutine(MonsterAction());
    }

    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.3f);

            if (state == State.DIE) yield break;

            if (playerTr == null) continue;

            float distance = Vector3.Distance(playerTr.position, monsterTr.position);

            if (distance <= attackDist) state = State.ATTACK;
            else if (distance <= traceDist) state = State.TRACE;
            else state = State.IDLE;
        }
    }

IEnumerator MonsterAction()
{
    while (!isDie)
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

            case State.DIE:
                isDie = true;

                if (agent != null && agent.isActiveAndEnabled)
                    agent.isStopped = true;

                anim.SetTrigger(hashDie);
                GetComponent<Collider>().enabled = false;
                break;
        }

        yield return new WaitForSeconds(0.3f);
    }
}


    // void OnCollisionEnter(Collision coll)
    // {
    //     if (coll.collider.CompareTag("BULLET"))
    //     {
    //         Destroy(coll.gameObject);
    //         anim.SetTrigger(hashHit);

    //         Vector3 pos = coll.GetContact(0).point;
    //         Quaternion rot = Quaternion.LookRotation(-coll.GetContact(0).normal);
    //         ShowBloodEffect(pos, rot);

    //         hp -= 10;
    //         if (hp <= 0) state = State.DIE;
    //     }
    // }

    // void ShowBloodEffect(Vector3 pos, Quaternion rot)
    // {
    //     GameObject blood = Instantiate(bloodEffect, pos, rot, monsterTr);
    //     Destroy(blood, 1.0f);
    // }

    // void OnDrawGizmos()
    // {
    //     if (state == State.TRACE)
    //     {
    //         Gizmos.color = Color.blue;
    //         Gizmos.DrawWireSphere(transform.position, traceDist);
    //     }
    //     if (state == State.ATTACK)
    //     {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawWireSphere(transform.position, attackDist);
    //     }
    // }


    void OnPlayerDie()
    {
        StopAllCoroutines();
        agent.isStopped = true;
        anim.SetFloat(hashSpeed, Random.Range(0.8f, 1.2f));
        anim.SetTrigger(hashPlayerDie);
    }

    // --- OnTriggerEnter 함수 (수정된 부분) ---
    void OnTriggerEnter(Collider other) // 'coll' 대신 'other' 사용 (관례)
    {
        string collidedTag = other.gameObject.tag;
        // <<< 이 로그가 OnTriggerEnter가 호출되었음을 명확히 보여줍니다.
        Debug.Log($"<color=green>[Monster OnTriggerEnter]</color> Monster detected Trigger: {other.gameObject.name}, Tag: {collidedTag}");

        // 플레이어와 충돌했는지 확인 (PlayerHealth 스크립트 연결을 가정)
        if (other.CompareTag("Player"))
        {
            // 이 로직은 PlayerHealth에서 처리하므로, 여기서는 몬스터가 플레이어를 '감지'했다는 역할만 합니다.
            // 몬스터가 플레이어를 감지했음을 알려 AI 상태 전환 등에 사용될 수 있습니다.
            Debug.Log($"Monster detected Player (Trigger): {other.gameObject.name}");
        }
        else if (other.CompareTag("BULLET")) // 총알과 충돌 시
        {
            Destroy(other.gameObject); 
            anim.SetTrigger(hashHit); 
            
            // ... (bloodEffect, hp 감소 로직) ...
            
            Debug.Log($"Monster hit by BULLET! Monster HP: {hp}");
        }
    }

    // OnCollisionEnter 함수도 추가하여 어떤 충돌이 발생하는지 명확히 확인
    void OnCollisionEnter(Collision collision)
    {
        string collidedTag = collision.gameObject.tag;
        // <<< 이 로그가 OnCollisionEnter가 호출되었음을 명확히 보여줍니다.
        Debug.Log($"<color=blue>[Monster OnCollisionEnter]</color> Monster collided with: {collision.gameObject.name}, Tag: {collidedTag}");

        // 몬스터의 Rigidbody.isKinematic이 true이고 Collider.isTrigger가 false이면
        // OnCollisionEnter가 호출되지 않을 수 있습니다.
        // 하지만 혹시 모를 경우를 대비해 로그를 추가합니다.
    }

}


