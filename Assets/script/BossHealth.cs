using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 20;
    public int currentHealth;

    [Header("UI Settings")]
    public GameObject bossHealthUI; // 🎯 이걸로 전체 체력 UI를 켜고 끔
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip hitSoundClip;
    public AudioClip dieSoundClip;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (bossHealthUI != null)
            bossHealthUI.SetActive(false); // 🎯 처음엔 UI 비활성화

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                Debug.LogWarning("BossHealth: AudioSource 컴포넌트가 없습니다.");
        }

        StartCoroutine(WaitAndShowUI());
    }

    IEnumerator WaitAndShowUI()
    {
        // 보스가 등장할 때까지 대기
        yield return new WaitUntil(() => gameObject.activeInHierarchy);

        yield return new WaitForSeconds(1f); // 약간의 연출 지연

        if (bossHealthUI != null)
            bossHealthUI.SetActive(true); // 보스 등장 후 UI 활성화
    }

    public void TakeDamage(int amount)
    {
        if (isDead || currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHealthUI();

        if (audioSource != null && hitSoundClip != null)
            audioSource.PlayOneShot(hitSoundClip);

        if (currentHealth <= 0)
            Die();
    }
    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (audioSource != null && dieSoundClip != null)
            audioSource.PlayOneShot(dieSoundClip);

        Debug.Log("🛑 Boss defeated!");

        if (bossHealthUI != null)
            bossHealthUI.SetActive(false);

        // === BossCtrl.InstantDeath() 호출 ===
        BossMonsterCtrl bossCtrl = GetComponent<BossMonsterCtrl>();
        if (bossCtrl != null)
        {
            StartCoroutine(bossCtrl.InstantDeath());
        }
        else
        {
            Debug.LogWarning("⚠️ BossCtrl 컴포넌트를 찾을 수 없습니다.");
        }
    }


    void UpdateHealthUI()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;

            Image fillImage = healthBarSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (currentHealth >= 131)
                    fillImage.color = Color.green;
                else if (currentHealth >= 61)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
        }

        if (healthText != null)
        {
            healthText.text = "BOSS: " + currentHealth + " / " + maxHealth;

            if (currentHealth >= 131)
                healthText.color = Color.green;
            else if (currentHealth >= 61)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.red;
        }
    }
}
