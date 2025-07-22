using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera; // 메인 카메라 참조

    void Start()
    {
        // 메인 카메라를 찾습니다.
        mainCamera = Camera.main; 
        if (mainCamera == null)
        {
            Debug.LogWarning("LookAtCamera: Main Camera를 찾을 수 없습니다. 'MainCamera' 태그를 확인하세요.");
            enabled = false; // 카메라 없으면 스크립트 비활성화
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // HealthBar가 항상 카메라를 바라보도록 회전합니다.
        // Quaternion.LookRotation(transform.position - mainCamera.transform.position)을 사용하면
        // 오브젝트의 Z축이 카메라를 향하게 됩니다. (즉, 오브젝트가 카메라를 바라봄)
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);

        // 또는 Y축만 회전하여 항상 수직을 유지하게 하고 싶다면:
        // transform.rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
    }
}