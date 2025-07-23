using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    public float typingSpeed = 0.05f; // 한 글자씩 나타나는 속도 (초)
    [TextArea(3, 10)] // Inspector에서 여러 줄 입력 가능하도록 설정
    public string fullText; // 표시할 전체 텍스트 (Inspector에서 입력)

    // === 오디오 관련 변수 추가 ===
    private AudioSource audioSource; // AudioSource 컴포넌트 참조

    private TextMeshProUGUI tmpText;
    private Coroutine typingCoroutine;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText == null)
        {
            Debug.LogError("TypewriterEffect: TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. 이 스크립트는 TextMeshProUGUI가 있는 오브젝트에 붙여야 합니다.");
            enabled = false;
            return;
        }
        tmpText.text = "";

        // === AudioSource 컴포넌트 가져오기 ===
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("TypewriterEffect: AudioSource 컴포넌트를 찾을 수 없습니다. 소리 재생을 위해 AudioSource를 추가해주세요.");
        }
    }

    void Start()
    {
        StartTyping(); // 게임 시작 시 타이핑 효과를 바로 시작
    }

    public void StartTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(ShowText());

        // === 타이핑 시작 시 소리 재생 ===
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        tmpText.text = fullText;

        // === 타이핑 완료 시 소리 정지 ===
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    IEnumerator ShowText()
    {
        tmpText.text = "";
        int currentCharacterIndex = 0;

        while (currentCharacterIndex < fullText.Length)
        {
            tmpText.text += fullText[currentCharacterIndex];
            currentCharacterIndex++;

            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null; // 타이핑 완료 후 코루틴 참조 해제

        // === 타이핑 완료 시 소리 정지 (코루틴이 끝날 때) ===
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // 예시: 특정 키를 누르면 타이핑 시작/완료
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (typingCoroutine == null)
            {
                StartTyping();
            }
            else
            {
                CompleteTyping();
            }
        }
    }
}