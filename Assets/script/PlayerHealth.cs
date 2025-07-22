using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections; // 코루틴을 사용하지 않는다면 이 줄도 제거 가능

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("플레이어의 최대 체력입니다.")]
    public int maxHealth = 100; 
    
    [Tooltip("플레이어의 현재 체력입니다.")]
    public int currentHealth;   

    [Header("UI Settings")]
    [Tooltip("체력 바 UI (선택 사항)")]
    public Slider healthBarSlider; 
    
    [Tooltip("체력 텍스트 UI (선택 사항)")]
    public TextMeshProUGUI healthText; 

    // === 피격 효과 UI (삭제) ===
    // [Tooltip("피격 효과 테두리 이미지 (화면 테두리용 스프라이트를 할당)")]
    // public Image hitEffectBorderImage; // 삭제
    // public float hitEffectFadeInDuration = 0.1f; // 삭제
    // public float hitEffectStayDuration = 0.2f;   // 삭제
    // public float hitEffectFadeOutDuration = 0.5f; // 삭제
    // public float maxHitEffectAlpha = 0.6f; // 삭제

    [Header("Collision Damage")]
    [Tooltip("건물 충돌 시 받는 데미지 양")]
    public int buildingCollisionDamage = 10; 
    [Tooltip("데미지를 줄 건물 태그")]
    public string buildingTag = "Building"; 

    [Tooltip("몬스터 본체 충돌 시 받는 데미지 양")]
    public int monsterCollisionDamage = 20; 
    [Tooltip("데미지를 줄 몬스터 태그")]
    public string monsterTag = "Monster"; 

    // === 몬스터 부위 콜라이더 이름 (새로 추가) ===
    [Header("Monster Part Names")]
    public string monsterHandName = "Hitbox_Hand_L"; 
    public string monsterFootName = "Hitbox_Foot_L"; 
    public string[] damageableMonsterPartNames; 

    [Header("Audio Settings")] // 새로 추가
    [Tooltip("피격 사운드를 재생할 AudioSource 컴포넌트")] // 새로 추가
    public AudioSource hitAudioSource; // 새로 추가
    [Tooltip("피격 시 재생할 오디오 클립 (예: sick.mp3)")] // 새로 추가
    public AudioClip hitSoundClip; // 새로 추가

    [Tooltip("플레이어가 죽었을 때 재생할 오디오 클립 (예: die.mp3)")] // 새로 추가
    public AudioClip dieSoundClip; // 새로 추가


    // 게임 시작 시 초기화
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI(); 
        
        // AudioSource 컴포넌트가 Inspector에 연결되지 않았다면 자동으로 찾기 (안전 장치)
        if (hitAudioSource == null)
        {
            hitAudioSource = GetComponent<AudioSource>();
            if (hitAudioSource == null)
            {
                Debug.LogWarning("PlayerHealth: Player 오브젝트에 AudioSource 컴포넌트가 없습니다. 피격 사운드를 재생할 수 없습니다.");
            }
        }
    }

    // 데미지를 입었을 때 호출되는 함수
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= amount; 
        currentHealth = Mathf.Max(currentHealth, 0); 

        Debug.Log("Player took " + amount + " damage. Current Health: " + currentHealth);
        UpdateHealthUI(); 

        // 피격 사운드 재생 (기존 로직)
        if (hitAudioSource != null && hitSoundClip != null)
        {
            hitAudioSource.PlayOneShot(hitSoundClip); 
            Debug.Log("Playing hit sound: " + hitSoundClip.name);
        }
        else
        {
            if(hitAudioSource == null) Debug.LogWarning("PlayerHealth: hitAudioSource가 연결되지 않았습니다. 피격 사운드를 재생할 수 없습니다.");
            if(hitSoundClip == null) Debug.LogWarning("PlayerHealth: hitSoundClip이 연결되지 않았습니다. 피격 사운드를 재생할 수 없습니다.");
        }

        if (currentHealth <= 0)
        {
            Die(); 
        }
    }

    // 체력을 회복했을 때 호출되는 함수
    public void Heal(int amount)
    {
        if (currentHealth >= maxHealth) return; 

        currentHealth += amount; 
        currentHealth = Mathf.Min(currentHealth, maxHealth); 

        Debug.Log("Player healed " + amount + ". Current Health: " + currentHealth);
        UpdateHealthUI(); 
    }

    // UpdateHealthUI 함수 (기존과 동일, 색상 로직 포함)
    void UpdateHealthUI()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;

            // 체력에 따라 색상 변경 (0~30 빨강, 31~60 노랑, 61~100 초록)
            Image fillImage = healthBarSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (currentHealth >= 61) // 61 ~ 100
                {
                    fillImage.color = Color.green;
                }
                else if (currentHealth >= 31) // 31 ~ 60
                {
                    fillImage.color = Color.yellow;
                }
                else // 0 ~ 30
                {
                    fillImage.color = Color.red;
                }
            }
        }

        if (healthText != null)
        {
            healthText.text = "HP: " + Mathf.CeilToInt(currentHealth) + " / " + Mathf.CeilToInt(maxHealth); 
            
            // 텍스트 색상도 체력에 따라 변경 (HealthBar와 동일한 로직)
            if (currentHealth >= 61) // 61 ~ 100
            {
                healthText.color = Color.green;
            }
            else if (currentHealth >= 31) // 31 ~ 60
            {
                healthText.color = Color.yellow;
            }
            else // 0 ~ 30
            {
                healthText.color = Color.red;
            }
        }
    }

    // 플레이어가 죽었을 때 호출되는 함수
    void Die()
    {
        Debug.Log("Player has died!");
        
        // === PlayerController 스크립트 비활성화 (즉시 움직임 중단) ===
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false; // PlayerController 컴포넌트 비활성화
            Debug.Log("PlayerController disabled to stop movement.");
        }
        else
        {
            Debug.LogWarning("PlayerController component not found on Player GameObject to disable it.");
        }

        // 죽음 사운드 재생 (기존 로직)
        if (hitAudioSource != null && dieSoundClip != null)
        {
            hitAudioSource.PlayOneShot(dieSoundClip); 
            Debug.Log("Playing death sound: " + dieSoundClip.name);
        }
        else
        {
            if(hitAudioSource == null) Debug.LogWarning("PlayerHealth: hitAudioSource가 연결되지 않았습니다. 죽음 사운드를 재생할 수 없습니다.");
            if(dieSoundClip == null) Debug.LogWarning("PlayerHealth: dieSoundClip이 연결되지 않았습니다. 죽음 사운드를 재생할 수 없습니다.");
        }

        // 씬 로드 및 오브젝트 비활성화를 위한 코루틴 시작
        StartCoroutine(HandleDeathAndSceneLoad());
    }

    // === 죽음 처리 및 씬 로드를 위한 코루틴 (수정) ===
    private IEnumerator HandleDeathAndSceneLoad()
    {
        // 1. 죽음 사운드 재생 (PlayerHealth.Die()에서 이미 호출)
        // 여기서 다시 호출할 필요는 없지만, 만약 Die()에서 PlayOneShot이 실행되기 전에
        // 이 코루틴이 시작될 가능성이 있다면 여기에 한번 더 PlayOneShot을 넣을 수 있습니다.
        // 현재는 Die()에서 이미 호출되므로 생략합니다.

        // 2. 사운드 재생 시간을 벌기 위해 잠시 대기
        // 사운드 클립의 길이에 맞춰 기다리는 것이 가장 정확합니다.
        float delayTime = 1.5f; // 기본 대기 시간 (die.mp3 길이에 따라 조절)
        if (dieSoundClip != null && hitAudioSource != null && hitAudioSource.enabled)
        {
            delayTime = dieSoundClip.length; // 사운드 길이만큼 대기
            // 사운드가 아직 재생 중일 수 있으므로 짧은 지연을 추가 (선택 사항)
            if (delayTime < 0.1f) delayTime = 0.1f; // 최소 대기 시간
        }
        Debug.Log($"Waiting {delayTime} seconds before loading End Scene...");
        yield return new WaitForSeconds(delayTime); 

        // 3. GameManager.EndGame() 호출 (씬 전환 및 BGM 정지 로직 포함)
        if (GameManager.Instance != null)
        {
            Debug.Log("Calling GameManager.Instance.EndGame() for scene transition.");
            GameManager.Instance.EndGame(); // GameManager의 EndGame 함수 호출
        }
        else
        {
            Debug.LogError("FATAL ERROR: GameManager.Instance를 여전히 찾을 수 없습니다! 게임 종료 처리 실패.");
        }
        
        // 4. 모든 것이 처리된 후 플레이어 오브젝트 비활성화 (가장 마지막!)
        // 이렇게 해야 코루틴이 끝까지 실행될 수 있습니다.
        gameObject.SetActive(false); 
    }

    void OnCollisionEnter(Collision collision)
    {
       string collidedTag = collision.gameObject.tag;
       string collidedColliderName = collision.collider.name; 

       Debug.Log($"<color=blue>[OnCollisionEnter]</color> Player collided with: {collision.gameObject.name}, Tag: {collidedTag}, Collider: {collidedColliderName}");

       if (collidedTag == buildingTag)
       {
           Debug.Log($"Player collided with Building: {collision.gameObject.name}, Taking {buildingCollisionDamage} damage.");
           TakeDamage(buildingCollisionDamage);
       }
       else if (IsDamageableMonsterPart(collidedColliderName)) 
       {
           Debug.Log($"Player hit by Monster Part: {collidedColliderName}, Taking {monsterCollisionDamage} damage.");
           TakeDamage(monsterCollisionDamage); 
       }
    }

    // OnTriggerEnter 함수 (기존과 동일)
    void OnTriggerEnter(Collider other)
    {
        string collidedTag = other.gameObject.tag;
        Debug.Log($"<color=green>[OnTriggerEnter]</color> Player entered Trigger: {other.gameObject.name}, Tag: {collidedTag}, Collider: {other.name}");

        if (collidedTag == "Monster") 
        {
            Debug.Log($"Player touched Monster Body (Trigger): {other.gameObject.name}");
        }
    }

    // IsDamageableMonsterPart 함수 (기존과 동일)
    bool IsDamageableMonsterPart(string colliderName)
    {
        foreach (string name in damageableMonsterPartNames)
        {
            if (colliderName == name) 
            {
                return true;
            }
        }
        return false;
    }
}