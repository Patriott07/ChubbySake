using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using TMPro;
using System;

public class GasingStat : MonoBehaviour
{
    [Header("Stat Dasar Gasing")]
    public string namaGasing = "Chibby Gasing";
    public float damage = 10f;
    public float maxHp = 1000f;
    public float currentHp;
    public int maxNyawa = 3;
    public int currentNyawa;
    public float currentRPM;
    public float maxRPM = 1200f;
    public float minRPM = 400f;
    public float rpmRegenSpeed = 50f;
    public float currentEnergyAttack = 0;
    public float maxEnergyAttack = 100;
    public float currentEnergyUltimate = 0;
    public float maxEnergyUltimate = 100;

    [Header("Stat Defense Per Part (Multiplier)")]
    public float defHead = 1.0f;
    public float defBody = 0.8f;
    public float defHand = 1.2f;
    public float defLeg = 0.9f;

    [Header("Sistem Respawn")]
    public Transform titikRespawn;
    private Rigidbody rb;

    [Header("Status Action Attack")]
    public bool isInvincibleAttack = false;
    public float damageTambahanQTE;
    private bool isColliding = false;

    [Header("Ronde UI System")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private int roundCount = 1;

    [Header("Reward System")]
    private float Exp, gold;
    [SerializeField] private TextMeshProUGUI rewardText;

    void Start()
    {
        if (roundText != null) roundText.text = $"Round : {roundCount.ToString()}";
        rb = GetComponent<Rigidbody>();

        if (GetComponent<GasingAI>() == null)
            ResetGasing();
    }

    void Update()
    {
        if (!isColliding && currentRPM < maxRPM)
        {
            currentRPM += rpmRegenSpeed * Time.deltaTime;
            currentRPM = Mathf.Clamp(currentRPM, minRPM, maxRPM);
        }
    }

    public void ResetGasing()
    {
        currentHp = maxHp;
        currentNyawa = maxNyawa;
        currentRPM = maxRPM / 4;
    }

    public void TerimaDamagePart(GasingPartCollider.PartType jenisPart, float kekuatanBenturan, Action<float> action)
    {
        if (isInvincibleAttack || kekuatanBenturan <= 0) return;

        if (currentHp <= 0 || currentNyawa <= 0) return;

        float multiplierDefense = 1.0f;

        switch (jenisPart)
        {
            case GasingPartCollider.PartType.Head:
                multiplierDefense = defHead;
                break;
            case GasingPartCollider.PartType.Body:
                multiplierDefense = defBody;
                break;
            case GasingPartCollider.PartType.Hand:
                multiplierDefense = defHand;
                break;
            case GasingPartCollider.PartType.Leg:
                multiplierDefense = defLeg;
                break;
        }

        float damageAkhir = kekuatanBenturan;

        currentHp -= damageAkhir;
        Debug.Log($"{gameObject.name} terkena damage sebesar {damageAkhir:F1} pada bagian {jenisPart}. Sisa HP: {currentHp:F1}");

        if (gameObject.CompareTag("Player"))
        {
            ActionCamera cam = Camera.main.GetComponent<ActionCamera>();
            if (cam != null && damageAkhir > 2f)
            {
            }
        }

        if (currentHp <= 0)
        {
            KurangiNyawaDanRespawn();
        }

        action?.Invoke(damageAkhir);
    }

    public void DecreaseRPM()
    {
        float rpmMinus = UnityEngine.Random.Range(200f, 300f);
        currentRPM -= rpmMinus;
        currentRPM = Mathf.Clamp(currentRPM, minRPM, maxRPM);
    }

    public void IncreaseEnergyAttack(float val)
    {
        currentEnergyAttack += val;
        currentEnergyAttack = Mathf.Clamp(currentEnergyAttack, 0, maxEnergyAttack);
    }

    public void KurangiNyawaDanRespawn()
    {
        roundCount++;
        if (roundText != null) roundText.text = $"Round : {roundCount.ToString()}";

        if (gameObject.CompareTag("Enemy"))
        {
            GameObject playerTarget = GameObject.FindGameObjectWithTag("Player");
            if (playerTarget != null)
            {
                GasingStat playerStat = playerTarget.GetComponent<GasingStat>();
                if (playerStat != null) playerStat.ClaimRewards(5f, 3.5f);
            }
        }
        else if (gameObject.CompareTag("Player"))
        {
            GameObject enemyTarget = GameObject.FindGameObjectWithTag("Enemy");
            if (enemyTarget != null)
            {
                GasingStat enemyStat = enemyTarget.GetComponent<GasingStat>();
                if (enemyStat != null) enemyStat.ClaimRewards(5f, 3.5f);
            }
        }

        currentNyawa--;
        Debug.Log($"{gameObject.name} kehilangan 1 Nyawa! Sisa Nyawa: {currentNyawa}");

        if (currentNyawa > 0)
        {
            StartCoroutine(ProsesRespawn());
        }
        else
        {
            GameOverOrKalah();
        }
    }

    private IEnumerator ProsesRespawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (titikRespawn != null)
        {
            transform.position = titikRespawn.position;
        }
        else
        {
            transform.position = Vector3.up * 2f;
        }

        currentHp = maxHp;
        currentRPM = maxRPM;

        Debug.Log($"{gameObject.name} telah respawn di arena!");
        yield return null;
    }

    private void GameOverOrKalah()
    {
        Debug.Log($"{gameObject.name} KALAH SEPENUHNYA! Game Over.");
        gameObject.SetActive(false);
    }

    public void ClaimRewards(float earnedGold, float earnedExp)
    {
        gold += earnedGold;
        Exp += earnedExp;
        
        if (rewardText != null)
        {
            rewardText.text = $"Gold : {gold} | Exp : {Exp}";
        }
    }

    void HealthPlayerDecrease(float playerHPDec)
    {
        currentHp -= playerHPDec;

        if (currentHp <= 0)
        {
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
        {
            isColliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
        {
            isColliding = false;
        }
    }
}
