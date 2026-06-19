using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem instance;
    private float enemyHP, playerHP;

    void Awake()
    {
        instance = this;
    }

    void HealthEnemyDecrease(float enemyHPDec)
    {
        enemyHP -= enemyHPDec;
    }


    // cara panggil = misal void ONCollisionEnter() { HealthEnemyDecrease(67); }
    // 

}
