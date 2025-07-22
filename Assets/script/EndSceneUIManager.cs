// EndSceneUIManager.cs

using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class EndSceneUIManager : MonoBehaviour
{
    public Button replayButton; 

    void Start()
    {
        Debug.Log("[EndSceneUIManager] Start method called."); // 스크립트 시작 로그

        if (replayButton == null)
        {
            Debug.LogError("EndSceneUIManager: ReplayButton이 Inspector에 연결되지 않았습니다! 씬에서 찾아봅니다.");
            // 씬에서 직접 "ReplayButton"이라는 이름의 버튼을 찾아봅니다.
            replayButton = GameObject.Find("ReplayButton")?.GetComponent<Button>();
            if (replayButton == null)
            {
                Debug.LogError("EndSceneUIManager: 씬에서 'ReplayButton' 오브젝트를 찾을 수 없거나 Button 컴포넌트가 없습니다. Replay 버튼이 작동하지 않을 것입니다.");
                return; // 버튼을 찾지 못하면 여기서 함수 종료
            }
            Debug.Log("[EndSceneUIManager] ReplayButton을 씬에서 찾았습니다.");
        }

        // GameManager 인스턴스가 존재하면 ReplayButton 이벤트 연결
        if (GameManager.Instance != null)
        {
            // 이전에 연결된 모든 리스너를 제거하고 새로 추가하여 중복 연결 방지 (안전 장치)
            replayButton.onClick.RemoveAllListeners(); 
            replayButton.onClick.AddListener(GameManager.Instance.RestartGame);
            Debug.Log("[EndSceneUIManager] ReplayButton의 OnClick 이벤트에 GameManager.RestartGame()이 성공적으로 연결되었습니다.");
        }
        else
        {
            Debug.LogError("[EndSceneUIManager] GameManager.Instance를 찾을 수 없습니다! ReplayButton이 작동하지 않습니다. GameManager가 IntroScene에 있고 DontDestroyOnLoad가 올바르게 설정되었는지 확인하세요.");
        }
    }
}