using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public float fadeInDuration = 2f; // Fade-in 지속 시간 (초)
    private Image imageToFade;
    private float timer;
    private Color startColor;
    private Color endColor;

    void Start()
    {
        // Image 컴포넌트 가져오기
        imageToFade = GetComponent<Image>();
        if (imageToFade == null)
        {
            Debug.LogError("Image 컴포넌트가 부착되지 않았습니다!");
            enabled = false; // 컴포넌트 비활성화
            return;
        }

        // 시작 색상 (투명)
        startColor = new Color(imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, 0f);
        // 종료 색상 (현재 색상의 불투명도 1)
        endColor = new Color(imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, 1f);

        // 시작 색상 적용
        imageToFade.color = startColor;
        timer = 0f;
    }

    void Update()
    {
        if (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            imageToFade.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
        else
        {
            // Fade-in 완료
            imageToFade.color = endColor;
            enabled = false; // 더 이상 업데이트할 필요 없음
        }
    }
}