using UnityEngine;
using TMPro; // TextMeshPro용 추가

public class FadeIn : MonoBehaviour
{
    public float fadeInDuration = 2f; 
    private TextMeshProUGUI textToFade; // TextMeshProUGUI로 변경
    private float timer;
    private Color startColor;
    private Color endColor;

    void OnEnable() // 오브젝트가 활성화될 때마다 실행되도록 OnEnable 사용
    {
        textToFade = GetComponent<TextMeshProUGUI>(); // TextMeshProUGUI 컴포넌트 가져오기
        if (textToFade == null)
        {
            Debug.LogError("TextMeshProUGUI 컴포넌트가 부착되지 않았습니다!");
            enabled = false;
            return;
        }

        // 현재 텍스트 색상을 기준으로 시작 및 끝 색상 설정
        startColor = new Color(textToFade.color.r, textToFade.color.g, textToFade.color.b, 0f); // 투명
        endColor = new Color(textToFade.color.r, textToFade.color.g, textToFade.color.b, 1f); // 불투명

        textToFade.color = startColor; // 시작 시 투명
        timer = 0f;
    }

    void Update()
    {
        // Time.timeScale이 0일 때도 작동해야 하므로 Time.unscaledDeltaTime 사용
        if (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            textToFade.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
        else
        {
            textToFade.color = endColor;
            enabled = false; // 페이드인 완료 후 스크립트 비활성화
        }
    }
}