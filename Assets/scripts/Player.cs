using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))  // H키 눌러서 데미지 받기
            {
                TakeDamage(10);
            }
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log("남은 체력: " + currentHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }
    private void Die()
    {
        Debug.Log("플레이어가 죽었습니다.");
        // 애니메이션, gameover 연동
    }
}
