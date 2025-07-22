using UnityEngine;

public class MonsterPartHitbox : MonoBehaviour
{
    public int damageAmount = 20; // 이 부위가 플레이어에게 줄 데미지 양

    // OnCollisionEnter는 이 스크립트가 붙은 콜라이더가 isTrigger=false이고,
    // 충돌 상대방도 isTrigger=false이며, 최소 하나가 Rigidbody일 때 호출됨.
    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log($"<color=orange>[Hitbox OnCollisionEnter]</color> {this.gameObject.name} collided with: {collision.gameObject.name}");

        // 플레이어 태그 확인 (PlayerController의 Player 태그를 사용한다고 가정)
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어 오브젝트에서 PlayerHealth 컴포넌트를 가져옴
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            // PlayerHealth 컴포넌트가 있다면 데미지 적용
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"<color=red>Player hit by {this.gameObject.name} ({this.gameObject.tag}), Taking {damageAmount} damage.</color>");
            }
        }
    }
}