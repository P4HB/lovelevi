// GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer Settings")]
    public bool isGameStarted = false;
    public float elapsedTime = 0f;
    public TextMeshProUGUI timerTextInstance;
    public float successSceneTransitionTime = 60f; // 10초 설정

    [Header("Scene Settings")]
    public string mainGameSceneName = "game";
    public string endSceneName = "EndScene";
    public string successSceneName = "SuccessScene";

    [Header("Audio Settings")]
    public AudioSource bgmAudioSource;
    public AudioClip gameSceneBGM;
    public AudioClip introSceneBGM;
    public AudioClip endSceneBGM;
    public AudioClip successSceneBGM; // Success Scene BGM 추가

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (bgmAudioSource == null)
            {
                bgmAudioSource = GetComponent<AudioSource>();
                if (bgmAudioSource == null)
                {
                    bgmAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
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
    }

    void Update()
    {
        if (isGameStarted)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI(); // 게임 중에는 계속 타이머 업데이트

            // 10초가 지나면 Success Scene으로 이동
            if (elapsedTime >= successSceneTransitionTime)
            {
                Debug.Log($"[GameManager] {successSceneTransitionTime}초 경과! Success Scene으로 이동합니다.");
                isGameStarted = false; // 게임 종료 처리
                SceneManager.LoadScene(successSceneName);
            }
        }
    }

    public void GoToGameScene()
    {
        Debug.Log("Game Started! Loading Main Game Scene.");
        Time.timeScale = 1f;

        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }
        
        SceneManager.LoadScene(mainGameSceneName);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Scene Loaded: {scene.name}");

        if (scene.name == mainGameSceneName)
        {
            // 게임 씬 로드 시 타이머 UI 찾기
            GameObject timerTextObj = GameObject.Find("TimerText");
            if (timerTextObj != null)
            {
                timerTextInstance = timerTextObj.GetComponent<TextMeshProUGUI>();
                if (timerTextInstance == null)
                {
                    Debug.LogError("[GameManager] 'TimerText' 오브젝트에 TextMeshProUGUI 컴포넌트가 없습니다!");
                }
            }
            else
            {
                Debug.LogError("[GameManager] 'TimerText' 오브젝트를 씬에서 찾을 수 없습니다!");
            }

            isGameStarted = true;
            Time.timeScale = 1f; // 게임 속도 정상화

            // BGM 재생
            Debug.Log("[GameManager] --- Attempting BGM Playback in Game Scene ---");
            if (bgmAudioSource == null)
            {
                Debug.LogError("[GameManager] BGM AudioSource is NULL. Cannot play BGM.");
            }
            else if (gameSceneBGM == null)
            {
                Debug.LogError("[GameManager] Game Scene BGM AudioClip is NULL. Cannot play BGM.");
            }
            else
            {
                Debug.Log($"[GameManager] BGM AudioSource found: {bgmAudioSource.name}");
                Debug.Log($"[GameManager] Game Scene BGM AudioClip assigned: {gameSceneBGM.name}");
                Debug.Log($"[GameManager] BGM AudioSource current status - Playing: {bgmAudioSource.isPlaying}, Volume: {bgmAudioSource.volume}, Mute: {bgmAudioSource.mute}");

                bgmAudioSource.clip = gameSceneBGM;
                bgmAudioSource.Play();
                Debug.Log($"[GameManager] BGM Play() called. After call status - Is Playing: {bgmAudioSource.isPlaying}");
            }
        }
        else if (scene.name == endSceneName)
        {
            isGameStarted = false;
            Time.timeScale = 0f; // 게임 일시 정지

            // BGM 재생
            Debug.Log("[GameManager] --- Attempting BGM Playback in End Scene ---");
            if (bgmAudioSource == null)
            {
                Debug.LogError("[GameManager] BGM AudioSource is NULL. Cannot play BGM.");
            }
            else if (endSceneBGM == null)
            {
                Debug.LogWarning("[GameManager] End Scene BGM AudioClip is NULL. No BGM for End Scene.");
            }
            else
            {
                Debug.Log($"[GameManager] BGM AudioSource found: {bgmAudioSource.name}");
                Debug.Log($"[GameManager] End Scene BGM AudioClip assigned: {endSceneBGM.name}");
                bgmAudioSource.clip = endSceneBGM;
                bgmAudioSource.Play();
                Debug.Log($"[GameManager] BGM Play() called. After call status - Is Playing: {bgmAudioSource.isPlaying}");
            }
        }
        else if (scene.name == "IntroScene") // IntroScene 로드 시
        {
            isGameStarted = false; // 게임 시작 상태 아님
            elapsedTime = 0f; // 타이머 초기화
            Time.timeScale = 1f; // 시간 스케일 초기화

            // Intro Scene BGM 재생 (선택 사항)
            Debug.Log("[GameManager] --- Attempting BGM Playback in Intro Scene ---");
            if (bgmAudioSource == null)
            {
                Debug.LogError("[GameManager] BGM AudioSource is NULL. Cannot play BGM in Intro Scene.");
            }
            else if (introSceneBGM == null)
            {
                Debug.LogWarning("[GameManager] Intro Scene BGM AudioClip is NULL. No BGM for Intro Scene.");
            }
            else
            {
                Debug.Log($"[GameManager] BGM AudioSource found: {bgmAudioSource.name}");
                Debug.Log($"[GameManager] Intro Scene BGM AudioClip assigned: {introSceneBGM.name}");
                bgmAudioSource.clip = introSceneBGM;
                bgmAudioSource.Play();
                Debug.Log($"[GameManager] BGM Play() called. After call status - Is Playing: {bgmAudioSource.isPlaying}");
            }
        }
        else if (scene.name == successSceneName) // SuccessScene 로드 시
        {
            isGameStarted = false; // 게임 종료 상태
            Time.timeScale = 0f; // 게임 일시 정지 (필요하다면)

            // BGM 재생 (선택 사항)
            Debug.Log("[GameManager] --- Attempting BGM Playback in Success Scene ---");
            if (bgmAudioSource == null)
            {
                Debug.LogError("[GameManager] BGM AudioSource is NULL. Cannot play BGM in Success Scene.");
            }
            else if (successSceneBGM == null)
            {
                Debug.LogWarning("[GameManager] Success Scene BGM AudioClip is NULL. No BGM for Success Scene.");
            }
            else
            {
                Debug.Log($"[GameManager] BGM AudioSource found: {bgmAudioSource.name}");
                Debug.Log($"[GameManager] Success Scene BGM AudioClip assigned: {successSceneBGM.name}");
                bgmAudioSource.clip = successSceneBGM;
                bgmAudioSource.Play();
                Debug.Log($"[GameManager] BGM Play() called. After call status - Is Playing: {bgmAudioSource.isPlaying}");
            }
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

    public void EndGame()
    {
        Debug.Log("Game Over! Loading End Scene.");
        isGameStarted = false;
        Time.timeScale = 1f; 

        // EndScene으로 전환 시 BGM은 EndScene BGM으로 자동 전환되므로 여기서 멈출 필요는 없습니다.
        // 하지만 명시적으로 멈추고 싶다면 아래 주석을 해제하세요.
        // if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        // {
        //     bgmAudioSource.Stop();
        // }

        SceneManager.LoadScene(endSceneName);
    }

    // === 게임 재시작 함수 (수정) ===
    public void RestartGame()
    {
        Debug.Log("[GameManager] Replay button clicked! Initiating game restart.");

        elapsedTime = 0f; // 타이머 초기화
        isGameStarted = false; // 타이머 정지 (새 씬 로드 후 다시 true 됨)

        // 시간 스케일을 1로 복구 (매우 중요!)
        // 씬 로드 전에 반드시 1로 설정되어야 씬 로드 과정이 멈추지 않습니다.
        Time.timeScale = 1f; 
        Debug.Log($"[GameManager] Time.timeScale set to: {Time.timeScale}");

        // 배경 음악이 있다면 멈춤 (EndScene BGM)
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
            Debug.Log("[GameManager] Stopped BGM before scene load.");
        }
        
        // 메인 게임 씬 로드
        Debug.Log($"[GameManager] Attempting to load scene: {mainGameSceneName}");
        SceneManager.LoadScene(mainGameSceneName);

        Debug.Log("[GameManager] SceneManager.LoadScene() called."); // 이 로그가 뜨는지 확인
    }
}