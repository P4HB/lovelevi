using UnityEngine;
using System.Collections;

public class GateExplosion : MonoBehaviour
{
    public AudioClip explosionClip; // 폭발 사운드 (Inspector에서 연결)
    public AudioSource audioSource; // AudioSource (이것도 Inspector에서 연결)

    private bool hasExploded = false;

    public void TriggerExplosion()
    {
        if (hasExploded) return; // 한 번만 실행

        hasExploded = true;

        // 폭발 사운드 재생
        if (audioSource != null && explosionClip != null)
        {
            audioSource.PlayOneShot(explosionClip);
        }

        // 폭발 효과 (파티클 등) 추가 가능
        // e.g., Instantiate(explosionEffect, transform.position, Quaternion.identity);
    }
}