using UnityEngine;

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

    private GameObject spawnedColossal; // 생성된 초w대형을 추적

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // 1. 초대형 타이탄 등장
        if (!colossalSpawned && elapsedTime >= colossalSpawnTime)
        {
            spawnedColossal = Instantiate(colossalTitanPrefab, colossalSpawnPoint.position, colossalSpawnPoint.rotation);
            Debug.Log("💥 초대형 타이탄 등장!");
            colossalSpawned = true;
        }

        // 2. 갑옷 타이탄: 초대형 등장 후 10초 지나야 생성됨
        if (colossalSpawned && !armoredSpawned && elapsedTime >= (colossalSpawnTime + armoredDelayAfterColossal))
        {
            Instantiate(armoredTitanPrefab, armoredSpawnPoint.position, Quaternion.identity);
            Debug.Log("🛡️ 갑옷 타이탄 등장!");
            armoredSpawned = true;

            // 3. 초대형 타이탄 제거
            if (spawnedColossal != null)
            {
                Destroy(spawnedColossal);
                Debug.Log("💥 초대형 타이탄 철수!");
            }
        }
    }
}