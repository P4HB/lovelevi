using UnityEngine;
using System.Collections;

public class BossTrigger : MonoBehaviour
{
    public GameObject colossalTitanPrefab;
    public GameObject armoredTitanPrefab;

    public Transform colossalSpawnPoint;
    public Transform armoredSpawnPoint;

    private float elapsedTime = 0f;
    private bool colossalSpawned = false;
    private bool armoredSpawned = false;


    private float colossalSpawnTime = 1000f;
    private float armoredDelayAfterColossal = 20f;
    private float gumingoutMusic = 2f;
    private GameObject spawnedColossal;

    // === 연출 관련 ===
    public Light mainLight;
    public GameObject explosionEffectPrefab;
    public Transform explosionSpawnPoint;
    public GameObject explosionEffect2Prefab;
    public Transform colossalexplosionpoint;
    public GameObject gateWallObject;
    public Transform playerTransform; // 플레이어 본체
    public Transform playerMoveTarget; // 순간이동 위치 (BossCinematicSpot)
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour ThirdPersonCamera;
    public Camera mainCamera;
    public Camera cinematicCamera;
    public AudioSource musicSource;
    public AudioClip colossalMusic;
    void Awake(){
        cinematicCamera.enabled = false;
        mainCamera.enabled = true;
    }
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= gumingoutMusic)
        {
            StartCoroutine(PlayMusicOnceAfterDelay(gumingoutMusic));
        }
        if (!colossalSpawned && elapsedTime >= colossalSpawnTime)
        {
            StartCoroutine(ColossalCinematicSequence());
            colossalSpawned = true;
        }

        if (colossalSpawned && !armoredSpawned && elapsedTime >= (colossalSpawnTime + armoredDelayAfterColossal))
        {
            Instantiate(armoredTitanPrefab, armoredSpawnPoint.position, Quaternion.identity);
            Debug.Log("🛡️ 갑옷 타이탄 등장!");
            armoredSpawned = true;

            if (spawnedColossal != null)
            {
                Destroy(spawnedColossal);
                Debug.Log("💥 초대형 타이탄 철수!");
            }
        }
    }

    IEnumerator PlayMusicOnceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!musicSource.isPlaying) // 혹시 모르니 중복 방지
        {
            musicSource.clip = colossalMusic;
            musicSource.Play();
            Debug.Log("🎶 음악 재생 시작 (코루틴)");
        }
    }
    IEnumerator ColossalCinematicSequence()
    {
        // 1. 카메라 전환
        if (mainCamera != null) mainCamera.enabled = false;
        if (cinematicCamera != null) cinematicCamera.enabled = true;
        // 2. 조작 정지
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (ThirdPersonCamera != null) ThirdPersonCamera.enabled = false;

        Debug.Log("🎥 시네마틱 카메라 전환 + 조작 정지");

        // 3. 하늘 번쩍 + 타이탄 등장 연출 등
        yield return StartCoroutine(ColossalEntranceSequence()); // 기존 이펙트 연출 코루틴

        // 4. 대기
        yield return new WaitForSeconds(8f);

        // 5. 원래 카메라로 복귀
        if (mainCamera != null) mainCamera.enabled = true;
        if (cinematicCamera != null) cinematicCamera.enabled = false;
        // 6. 조작 복구
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (ThirdPersonCamera != null) ThirdPersonCamera.enabled = true;

        Debug.Log("🔁 시네마틱 종료, 카메라 + 조작 복귀");
    }
    IEnumerator ColossalEntranceSequence()
    {

        if (playerTransform != null && playerMoveTarget != null)
        {
            playerTransform.position = playerMoveTarget.position;
            playerTransform.rotation = playerMoveTarget.rotation;
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log("🛑 플레이어 물리 속도 초기화 완료");
            }
            if (playerMovementScript != null)
            {
                playerMovementScript.enabled = false;
                Debug.Log("🧊 플레이어 이동 잠금 시작 (8초)");

                StartCoroutine(ReEnableMovementAfterDelay(8f));
            }
            Debug.Log("🎬 플레이어 시점: 보스 연출 위치로 이동");
        }
        yield return new WaitForSeconds(1f);
        float originalIntensity = mainLight.intensity;
        Color originalColor = mainLight.color;
        mainLight.intensity = 8f;
        mainLight.color = new Color(1f, 0.9f, 0.5f);
        Instantiate(explosionEffect2Prefab, colossalexplosionpoint.position, Quaternion.identity);
        yield return new WaitForSeconds(2f);
        mainLight.intensity = originalIntensity;
        mainLight.color = originalColor;
        // 1초 대기 후 타이탄 등장 + 폭발

        // 폭발 이펙트
        spawnedColossal = Instantiate(colossalTitanPrefab, colossalSpawnPoint.position, colossalSpawnPoint.rotation);
        Debug.Log("💥 초대형 타이탄 등장!");
        yield return new WaitForSeconds(5f);
        Instantiate(explosionEffectPrefab, explosionSpawnPoint.position, Quaternion.identity);
        Debug.Log("💥 성문 폭발!");
        yield return new WaitForSeconds(1f);
        // 성문 제거
        if (gateWallObject != null)
        {
            Destroy(gateWallObject);
            Debug.Log("🚪 성문 제거 완료");
        }

        // 타이탄 등장


    }
    IEnumerator ReEnableMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
            Debug.Log("🔓 플레이어 이동 잠금 해제");
        }
    }
}
