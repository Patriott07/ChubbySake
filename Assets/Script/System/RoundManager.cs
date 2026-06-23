using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private Transform spawnEnemyLoc, spawnPlayerLoc;
    [SerializeField] private GameObject enemyObj, playerObj;
    [SerializeField] private int roundCount = 0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI roundText;

    public static RoundManager instance;

    void Awake()
    {
        instance = this;
    }

    void NewRound()
    {
        roundCount++;
        Instantiate(enemyObj, spawnEnemyLoc.position, Quaternion.identity);
        Instantiate(playerObj, spawnPlayerLoc.position, Quaternion.identity);

        if (roundText != null) roundText.text = $"round : {roundCount.ToString()}";
    }
}
