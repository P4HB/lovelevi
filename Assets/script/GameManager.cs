// GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위해 추가
using TMPro; // TextMeshProUGUI를 사용하기 위해 추가

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer Settings")]
    public bool isGameStarted = false;
    public float elapsedTime = 0f;
    public TextMeshProUGUI timerTextInstance; // 이 변수는 Start()에서 로드됩니다.

    [Header("Scene Settings")]
    public string mainGameSceneName = "game";
    public string endSceneName = "EndScene"; // 새로 추가: 게임 종료 씬 이름

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        elapsedTime = 0f;
        // UpdateTimerUI(); // Start()에서 호출되므로 여기서 제거합니다.
    }

    void Update()
    {
        if (isGameStarted)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name); // 씬 로드 로그 추가

        // "game" 씬이 로드되었을 때만 타이머 텍스트를 찾습니다.
        if (scene.name == mainGameSceneName)
        {
            // 씬에서 "TimerText"라는 이름의 TextMeshProUGUI 오브젝트를 찾습니다.
            GameObject timerTextObject = GameObject.Find("TimerText");
            if (timerTextObject != null)
            {
                timerTextInstance = timerTextObject.GetComponent<TextMeshProUGUI>();
                if (timerTextInstance == null)
                {
                    Debug.LogWarning("GameManager: 'TimerText' 오브젝트에 TextMeshProUGUI 컴포넌트가 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("GameManager: 'TimerText' 오브젝트를 씬에서 찾을 수 없습니다.");
            }
            isGameStarted = true; // 게임 시작
        }
        else
        {
            isGameStarted = false; // 다른 씬에서는 게임 시작 상태 아님
        }
    }

    void UpdateTimerUI()
    {
        if (timerTextInstance != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime % 60F);
            timerTextInstance.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // === 새로 추가된 게임 종료 함수 ===
    public void EndGame()
    {
        Debug.Log("Game Over! Loading End Scene.");
        isGameStarted = false; // 게임 시작 상태 해제
        SceneManager.LoadScene(endSceneName); // "EndScene"으로 전환
    }
}