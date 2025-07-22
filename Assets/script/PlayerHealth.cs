using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 추가
using TMPro; // TextMeshPro 사용을 위해 추가

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

    [Tooltip("몬스터 손/발 태그들")]
    public string monsterHandTag = "MonsterHand"; // 몬스터 손 태그 (새로 추가)
    public string monsterFootTag = "MonsterFoot"; // 몬스터 발 태그 (새로 추가)

    [Tooltip("몬스터 손/발에 부딪혔을 때 받는 데미지 양")]
    public int monsterAttackDamage = 20; // 몬스터 공격 데미지 (새로 추가)

    // 게임 시작 시 초기화
    void Start()
    {
        currentHealth = maxHealth; // 게임 시작 시 현재 체력을 최대 체력으로 설정
        UpdateHealthUI(); // UI 업데이트
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
        gameObject.SetActive(false); 

        // GameManager 인스턴스가 존재하면 게임 종료 함수 호출
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }
        else
        {
            Debug.LogWarning("GameManager.Instance를 찾을 수 없습니다. 게임 종료 처리 실패.");
        }
    }

        void OnCollisionEnter(Collision collision)
    {
       // 충돌한 오브젝트의 태그 확인
        string collidedTag = collision.gameObject.tag;

        // 디버그 로그 추가: 어떤 태그와 충돌했는지 확인
        Debug.Log("Player collided with: " + collision.gameObject.name + ", Tag: " + collidedTag);

        // 1. 건물 충돌 데미지
        if (collidedTag == buildingTag)
        {
            Debug.Log("Player collided with Building: " + collision.gameObject.name + ", Taking " + buildingCollisionDamage + " damage.");
            TakeDamage(buildingCollisionDamage);
        }
        // 2. 몬스터 손/발 충돌 데미지
        else if (collidedTag == monsterHandTag || collidedTag == monsterFootTag)
        {
            Debug.Log("Player hit by Monster Part: " + collision.gameObject.name + ", Taking " + monsterAttackDamage + " damage.");
            TakeDamage(monsterAttackDamage);
        }
    }
}