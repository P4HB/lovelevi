using UnityEngine;
using TMPro; // TextMeshPro를 사용하려면 이 네임스페이스가 필요합니다.
using System.Collections; // 코루틴을 사용하려면 이 네임스페이스가 필요합니다.

public class TextFadeInEffect : MonoBehaviour
{
    public float fadeInDuration = 5.0f; // 텍스트가 완전히 나타나는 데 걸리는 시간 (초)

    private TextMeshProUGUI tmpText; // TextMeshProUGUI 컴포넌트 참조
    private Coroutine fadeInCoroutine; // 현재 실행 중인 페이드 인 코루틴 참조

    void Awake()
    {
        // 이 스크립트가 붙은 오브젝트에서 TextMeshProUGUI 컴포넌트를 가져옵니다.
        tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText == null)
        {
            Debug.LogError("TextFadeInEffect: TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. 이 스크립트는 TextMeshProUGUI가 있는 오브젝트에 붙여야 합니다.");
            enabled = false; // 컴포넌트를 찾지 못하면 스크립트 비활성화
            return;
        }

        // 시작 시 텍스트를 완전히 투명하게 만듭니다.
        Color currentColor = tmpText.color;
        currentColor.a = 0f; // 알파 값을 0으로 설정 (완전 투명)
        tmpText.color = currentColor;
    }

    void Start()
    {
        StartFadeIn(); // 게임 시작 시 페이드 인 효과를 바로 시작
    }

    // 외부에서 페이드 인 효과를 시작할 때 호출할 함수
    public void StartFadeIn()
    {
        // 이미 페이드 인 중이라면 기존 코루틴을 중지하고 다시 시작
        if (fadeInCoroutine != null)
        {
            StopCoroutine(fadeInCoroutine);
        }
        fadeInCoroutine = StartCoroutine(FadeInText());
    }

    IEnumerator FadeInText()
    {
        float timer = 0f;
        Color startColor = tmpText.color;
        startColor.a = 0f; // 시작 알파는 0
        tmpText.color = startColor;

        Color targetColor = tmpText.color;
        targetColor.a = 1f; // 목표 알파는 1 (완전 불투명)

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration; // 0에서 1까지 진행도 계산

            // 알파 값을 0에서 1로 선형 보간합니다.
            Color newColor = tmpText.color;
            newColor.a = Mathf.Lerp(startColor.a, targetColor.a, progress);
            tmpText.color = newColor;

            yield return null; // 다음 프레임까지 기다립니다.
        }

        // 페이드 인 완료 후 알파 값을 확실히 1로 설정합니다.
        Color finalColor = tmpText.color;
        finalColor.a = 1f;
        tmpText.color = finalColor;

        fadeInCoroutine = null; // 코루틴 참조 해제
    }

    // 예시: 특정 키를 누르면 페이드 인 시작 (디버그용)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) // F 키를 누르면 페이드 인 시작
        {
            StartFadeIn();
        }
    }
}