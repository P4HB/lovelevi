using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 추가
using TMPro; // TextMeshPro 사용을 위해 추가
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("플레이어의 최대 체력입니다.")]
    public int maxHealth = 100; // 플레이어의 최대 체력
    
    [Tooltip("플레이어의 현재 체력입니다.")]
    public int currentHealth;   // 플레이어의 현재 체력

    [Header("UI Settings")]
    [Tooltip("체력 바 UI (선택 사항)")]
    public Slider healthBarSlider; // 체력 바 UI (Unity UI Slider 컴포넌트)
    
    [Tooltip("체력 텍스트 UI (선택 사항)")]
    public TextMeshProUGUI healthText; // 체력 텍스트 UI (Unity UI Text 컴포넌트)

    [Header("Collision Damage")]
    [Tooltip("건물 충돌 시 받는 데미지 양")]
    public int buildingCollisionDamage = 10; // 건물 충돌 시 받는 데미지

    [Tooltip("데미지를 줄 건물 태그")]
    public string buildingTag = "Building"; // 데미지를 줄 건물 태그 (새로 추가)

    [Tooltip("몬스터 본체 충돌 시 받는 데미지 양")]
    public int monsterCollisionDamage = 20; 
    [Tooltip("데미지를 줄 몬스터 태그")]
    public string monsterTag = "Monster"; // 몬스터 본체 태그

    // === 몬스터 부위 콜라이더 이름 (새로 추가) ===
    [Header("Monster Part Names")]
    public string monsterHandName = "Hitbox_Hand_L"; // 예시: 왼쪽 손 이름
    public string monsterFootName = "Hitbox_Foot_L"; // 예시: 왼쪽 발 이름
    // (오른쪽 손발도 포함해야 하므로, 배열로 바꾸거나 더 많은 변수 선언)
    public string[] damageableMonsterPartNames; // <<< 여기에 모든 히트박스 이름 추가 (Inspector에서)

    // [Tooltip("몬스터 손/발 태그들")]
    // public string monsterHandTag = "MonsterHand"; // 몬스터 손 태그 (새로 추가)
    // public string monsterFootTag = "MonsterFoot"; // 몬스터 발 태그 (새로 추가)
    

    // 게임 시작 시 초기화
    void Start()
    {
        ResetHealth(); // 게임 시작 시 체력 초기화
    }

    // 데미지를 입었을 때 호출되는 함수
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; // 이미 죽었으면 더 이상 데미지 받지 않음

        currentHealth -= amount; // 현재 체력에서 데미지 양만큼 감소
        currentHealth = Mathf.Max(currentHealth, 0); // 체력이 0 미만으로 내려가지 않도록 보정

        Debug.Log("Player took " + amount + " damage. Current Health: " + currentHealth);
        UpdateHealthUI(); // UI 업데이트

        if (currentHealth <= 0)
        {
            Die(); // 체력이 0이 되면 죽음 처리
        }
    }

    // 체력을 회복했을 때 호출되는 함수
    public void Heal(int amount)
    {
        if (currentHealth >= maxHealth) return; // 이미 최대 체력이면 회복하지 않음

        currentHealth += amount; // 현재 체력에 회복 양만큼 증가
        currentHealth = Mathf.Min(currentHealth, maxHealth); // 체력이 최대 체력을 초과하지 않도록 보정

        Debug.Log("Player healed " + amount + ". Current Health: " + currentHealth);
        UpdateHealthUI(); // UI 업데이트
    }

    void UpdateHealthUI()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            // TextMeshProUGUI는 .text 속성을 사용합니다.
            healthText.text = "HP: " + currentHealth + " / " + maxHealth; 
        }
    }

    // 플레이어가 죽었을 때 호출되는 함수
    void Die()
    {
        Debug.Log("Player has died!");
        // 여기에 플레이어가 죽었을 때의 로직을 추가합니다.
        // 예: 플레이어 오브젝트 비활성화
        // gameObject.SetActive(false); 

        // GameManager 인스턴스에 안전하게 접근하는 로직 강화
        // 코루틴으로 한 프레임 지연시켜 GameManager가 초기화될 시간을 줍니다.
        StartCoroutine(HandleGameOverAfterDelay());
    }

    private IEnumerator HandleGameOverAfterDelay()
    {
        // 다음 프레임까지 기다립니다. (GameManager가 Awake/Start를 마칠 시간을 줍니다.)
        yield return null; 

        // 코루틴이 시작된 후, 그리고 씬 전환 직전에 플레이어 오브젝트를 비활성화합니다.
        gameObject.SetActive(false); // <<< 이 줄을 여기로 옮깁니다!

        // GameManager 인스턴스가 존재하면 게임 종료 함수 호출
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }
        else
        {
            Debug.LogError("FATAL ERROR: GameManager.Instance를 여전히 찾을 수 없습니다! 게임 종료 처리 실패.");
            // 최후의 수단: 직접 씬 로드 (GameManager 없이) - 이는 임시 방편입니다.
            // UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene"); 
        }
    }

        // --- OnCollisionEnter 함수 (Physics Layer Matrix에서 서로 충돌하고 Is Trigger가 false인 콜라이더끼리 충돌 시) ---
    void OnCollisionEnter(Collision collision)
    {
       string collidedTag = collision.gameObject.tag; // 충돌한 GameObject의 태그
       string collidedColliderName = collision.collider.name; // 충돌한 Collider의 이름 <<< 이 변수를 사용

       Debug.Log($"<color=blue>[OnCollisionEnter]</color> Player collided with: {collision.gameObject.name}, Tag: {collidedTag}, Collider: {collidedColliderName}");

       // 1. 건물 충돌 데미지
       if (collidedTag == buildingTag)
       {
           Debug.Log($"Player collided with Building: {collision.gameObject.name}, Taking {buildingCollisionDamage} damage.");
           TakeDamage(buildingCollisionDamage);
       }
       // 2. 몬스터 부위 충돌 데미지 (콜라이더 이름으로 판단)
       else if (IsDamageableMonsterPart(collidedColliderName)) // <<< 변경된 부분
       {
           Debug.Log($"Player hit by Monster Part: {collidedColliderName}, Taking {monsterCollisionDamage} damage."); // 데미지 메시지 수정
           TakeDamage(monsterCollisionDamage); // 데미지 적용
       }
    }

    // --- 새로운 헬퍼 함수 추가 (클래스 내부, OnCollisionEnter 함수 밖) ---
    bool IsDamageableMonsterPart(string colliderName)
    {
        foreach (string name in damageableMonsterPartNames)
        {
            if (colliderName == name) // 콜라이더 이름이 배열에 있는지 확인
            {
                return true;
            }
        }
        return false;
    }

    // --- OnTriggerEnter 함수 (Physics Layer Matrix에서 서로 충돌하고 최소 하나가 Is Trigger인 콜라이더끼리 충돌 시) ---
    void OnTriggerEnter(Collider other) // OnCollisionEnter와 다른 콜백입니다.
    {
        string collidedTag = other.gameObject.tag;
        // <<< 이 로그가 OnTriggerEnter가 호출되었음을 명확히 보여줍니다.
        Debug.Log($"<color=green>[OnTriggerEnter]</color> Player entered Trigger: {other.gameObject.name}, Tag: {collidedTag}, Collider: {other.name}");

        // 여기서는 데미지를 주지 않고, MonsterCtrl에서 Trigger를 통해 플레이어와 접촉했음을 알릴 수 있습니다.
        // 몬스터 본체(Is Trigger = true)와의 접촉을 여기서 감지할 수 있습니다.
        if (collidedTag == "Monster") // 몬스터 본체에 Is Trigger = true가 설정되어 있을 때
        {
            Debug.Log($"Player touched Monster Body (Trigger): {other.gameObject.name}");
            // 특정 로직 (예: 몬스터의 공격 애니메이션 트리거 등)
        }
    }

    // === 새로 추가된 체력 초기화 함수 ===
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        // 플레이어 오브젝트 활성화 (죽었을 때 비활성화했다면)
        // if (playerModel != null) playerModel.SetActive(true);
        // else gameObject.SetActive(true); // 전체 오브젝트 활성화

        UpdateHealthUI(); // UI 업데이트
        Debug.Log($"Player Health Reset: {currentHealth}/{maxHealth}");
    }

}