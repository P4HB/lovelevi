using UnityEngine;
using UnityEngine.UI;

public class ButtonAppearEffect : MonoBehaviour
{
    public float moveDistance = 200f; // 버튼이 아래에서 올라올 거리
    public float moveDuration = 0.5f; // 올라오는 데 걸리는 시간
    public float startDelay = 0f;     // 애니메이션 시작 전 지연 시간

    private RectTransform buttonRectTransform; // 이 스크립트가 부착된 버튼의 RectTransform
    private Image buttonImage;               // 버튼의 Image 컴포넌트 (알파 값 조작용)
    private Vector2 targetPosition;          // 버튼이 최종적으로 도달할 위치
    private Vector2 initialPosition;         // 버튼의 시작 위치 (화면 아래)

    private float timer;
    private bool hasStartedMoving = false; // 애니메이션이 시작되었는지 확인하는 플래그

    void Start()
    {
        buttonRectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>(); // Image 컴포넌트 가져오기

        if (buttonRectTransform == null)
        {
            Debug.LogError("ButtonAppearEffect: RectTransform 컴포넌트가 부착되지 않았습니다! 이 스크립트는 UI 오브젝트(예: Button)에 부착해야 합니다.");
            enabled = false;
            return;
        }
        if (buttonImage == null) // Image 컴포넌트가 없으면 알파 값 조작 불가
        {
            Debug.LogWarning("ButtonAppearEffect: Image 컴포넌트가 부착되지 않았습니다! Fade-in 효과가 적용되지 않습니다.");
            // enabled = false; // Image가 없어도 움직임은 필요하므로 스크립트를 끄지는 않습니다.
        }

        // 버튼의 최종 위치 (Inspector에서 설정된 원래 위치)
        targetPosition = buttonRectTransform.anchoredPosition;

        // 버튼의 시작 위치 (최종 위치에서 moveDistance만큼 아래)
        initialPosition = targetPosition - Vector2.up * moveDistance;
        
        // 애니메이션 시작 전 초기 위치로 설정
        buttonRectTransform.anchoredPosition = initialPosition;
        
        // 초기 알파 값을 0(완전 투명)으로 설정
        if (buttonImage != null)
        {
            Color tempColor = buttonImage.color;
            tempColor.a = 0f;
            buttonImage.color = tempColor;
        }

        timer = 0f;

        // startDelay 후 애니메이션 시작
        Invoke("StartMoving", startDelay);
    }

    void StartMoving()
    {
        hasStartedMoving = true;
    }

    void Update()
    {
        if (hasStartedMoving && timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);

            // 1. 위치 보간 (아래에서 위로)
            buttonRectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, t);

            // 2. 알파 값 보간 (서서히 나타남)
            if (buttonImage != null)
            {
                Color currentColor = buttonImage.color;
                currentColor.a = Mathf.Lerp(0f, 1f, t); // 알파 값을 0에서 1로 보간
                buttonImage.color = currentColor;
            }
        }
        else if (hasStartedMoving) // 애니메이션이 완료되었을 때
        {
            // 최종 위치 및 알파 값으로 정확히 설정
            buttonRectTransform.anchoredPosition = targetPosition;
            if (buttonImage != null)
            {
                Color finalColor = buttonImage.color;
                finalColor.a = 1f;
                buttonImage.color = finalColor;
            }
            enabled = false; // 스크립트 비활성화
        }
    }
}