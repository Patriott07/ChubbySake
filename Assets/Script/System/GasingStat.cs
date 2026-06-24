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


    // COROUTINE
    // Variabel penampung untuk mendeteksi Coroutine yang sedang berjalan
    private Coroutine damageBuffCoroutine = null;
    private Coroutine invincibleBuffCoroutine = null;

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

        if (currentHp <= 0) RoundManager.instance.NewRound(gameObject.tag);

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




    // mungkin udah gak dipake #ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f
    // nanti di edit
    public void KurangiNyawaDanRespawn()
    {
        roundCount++;

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

    private IEnumerator ProsesRespawn()  // respawn system
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
    // nanti di edit (END)
    // udah gak berguna kayaknya !!!!!!!!!!!!!!!!!!!!!!!!(WARN) #ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f#ff0f0f






    private void GameOverOrKalah()
    {
        Debug.Log($"{gameObject.name} KALAH SEPENUHNYA! Game Over.");
        gameObject.SetActive(false);
    }

    public void ClaimRewards(float earnedGold, float earnedExp) // funtion for reward system
    {
        Debug.Log("Reward Claimed");
        gold += earnedGold;
        Exp += earnedExp;

        if (rewardText != null)
            rewardText.text = $"Gold : {gold} | Exp : {Exp}";
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


    // --- SYSTEM BUFF DAMAGE ---
    public void ApplyDamageBuff(float additionalDamage, float duration)
    {
        if (damageBuffCoroutine != null)
        {
            StopCoroutine(damageBuffCoroutine);
            // Kembalikan damage ke baseline sebelum menjalankan yang baru, 
            // agar penambahannya tidak melipat ganda (compounding bug)
            damage -= additionalDamage; 
        }

        // Jalankan ulang dari detik ke-0 dengan durasi penuh yang baru
        damageBuffCoroutine = StartCoroutine(DamageBuffCoroutine(additionalDamage, duration));
    }

    private IEnumerator DamageBuffCoroutine(float additionalDamage, float duration)
    {
        damage += (damage * additionalDamage); // Tambah damage gasing
        Debug.Log($"[BUFF] Damage meningkat sebesar +{(damage * additionalDamage)} selama {duration} detik!");

        yield return new WaitForSeconds(duration);

        damage -= (damage * additionalDamage); // Kembalikan damage ke semula setelah durasi habis
        Debug.Log("[BUFF] Efek Buff Damage telah habis. Damage kembali normal.");
    }

    // --- SYSTEM REFRESH BUFF KEBAL ---
    public void ApplyInvincibleBuff(float duration)
    {
        // KUNCI: Jika sedang dalam mode kebal dari potion sebelumnya, matikan coroutine lamanya
        if (invincibleBuffCoroutine != null)
        {
            StopCoroutine(invincibleBuffCoroutine);
        }

        // Jalankan ulang durasi kebal dari awal
        invincibleBuffCoroutine = StartCoroutine(InvincibleBuffCoroutine(duration));
    }

    private IEnumerator InvincibleBuffCoroutine(float duration)
    {
        isInvincibleAttack = true;
        Debug.Log($"[BUFF] Mode Kebal di-refresh kembali ke {duration} detik!");

        yield return new WaitForSeconds(duration);

        isInvincibleAttack = false;
        invincibleBuffCoroutine = null; // Reset referensi
        Debug.Log("[BUFF] Efek Kebal habis.");
    }
}
