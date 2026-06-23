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

    // Param loose obj is a string with tag of the looser inside
    public void NewRound(string looseObj)
    {
        roundCount++;

        playerObj.transform.position = spawnPlayerLoc.position;
        enemyObj.transform.position = spawnEnemyLoc.position;

        enemyObj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        enemyObj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        enemyObj.GetComponent<GasingStat>().ResetGasing();

        playerObj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        playerObj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        playerObj.GetComponent<GasingStat>().ResetGasing();

        if (roundText != null) roundText.text = $"round : {roundCount.ToString()}";

        if (looseObj == "Enemy")
        {
            // If player is win
            playerObj.GetComponent<GasingStat>().ClaimRewards(5f, 3.5f);
        }
        else if (looseObj == "Player")
        {
            // If player is loose
            enemyObj.GetComponent<GasingStat>().ClaimRewards(5f, 3.5f);
        }
    }
}
