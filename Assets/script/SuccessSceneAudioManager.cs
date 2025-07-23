using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 필요

public class SuccessSceneAudioManager : MonoBehaviour
{
    public AudioSource dubbingAudioSource; // 더빙 오디오를 재생할 AudioSource
    public AudioSource successBGMAudioSource; // 배경 음악을 재생할 AudioSource

    void Start()
    {
        // 모든 AudioSource가 할당되었는지 확인
        if (dubbingAudioSource == null)
        {
            Debug.LogError("SuccessSceneAudioManager: Dubbing Audio Source가 할당되지 않았습니다!");
            return;
        }
        if (successBGMAudioSource == null)
        {
            Debug.LogError("SuccessSceneAudioManager: Success BGM Audio Source가 할당되지 않았습니다!");
            return;
        }

        // 더빙 오디오 재생 코루틴 시작
        StartCoroutine(PlayAudioSequence());
    }

    IEnumerator PlayAudioSequence()
    {
        // 1. 더빙 오디오 재생
        dubbingAudioSource.Play();
        Debug.Log("더빙 오디오 재생 시작.");

        // 2. 더빙 오디오가 끝날 때까지 기다림
        // isPlaying이 false가 될 때까지 기다립니다.
        // 짧은 오디오 클립의 경우 재생이 즉시 끝날 수 있으므로 약간의 딜레이를 주는 것이 좋습니다.
        yield return new WaitForSeconds(0.1f); // 최소 0.1초 대기 (오디오 재생 시작 보장)
        while (dubbingAudioSource.isPlaying)
        {
            yield return null; // 다음 프레임까지 기다림
        }

        Debug.Log("더빙 오디오 재생 완료.");

        // 3. 배경 음악 재생
        successBGMAudioSource.Play();
        Debug.Log("배경 음악 재생 시작.");
    }
}