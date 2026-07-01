using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using data.structs;

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

    [Header("Energy System")]
    public float currentEnergyAttack = 0;
    public float maxEnergyAttack = 100;
    public float currentEnergyUltimate = 0;
    public float maxEnergyUltimate = 100;

    [Header("Stat Defense Per Part (Multiplier)")]
    public float defHead = 1.0f;
    public float defBody = 0.8f;
    public float defHand = 1.2f;
    public float defLeg = 0.9f;

    [Header("Sistem Spawning & Fisika")]
    public Transform titikRespawn;
    private Rigidbody rb;
    private bool isColliding = false;

    [Header("Status Action Attack")]
    public bool isInvincibleAttack = false;
    public float damageTambahanQTE;

    [Header("Damage Pop-Up Settings")]
    [SerializeField] private DamageTextUI damageTextPrefab;
    [SerializeField] private float offsetText;

    [Header("Currency & Progression Store")]
    private float exp;
    private float gold;

    // Properti enkapsulasi agar skrip UI luar bisa membaca datanya tanpa merusak kodenya
    public float Gold => gold;
    public float Exp => exp;

    // Coroutine Management
    private Coroutine damageBuffCoroutine = null;
    private Coroutine invincibleBuffCoroutine = null;

    [HideInInspector] public bool isDying = false; // Flag penanda gasing sedang dalam proses mati

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (GetComponent<GasingAI>() == null)
            ResetGasing();
    }

    void Update()
    {
        // Regenerasi RPM otomatis saat gasing sedang tidak berbenturan kencang
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

    public void TerimaDamagePart(PartType jenisPart, float kekuatanBenturan, Action<float> action)
    {
        if (isInvincibleAttack || kekuatanBenturan <= 0) return;
        if (currentHp <= 0 || currentNyawa <= 0) return;

        float multiplierDefense = 1.0f;
        switch (jenisPart)
        {
            case PartType.Head: multiplierDefense = defHead; break;
            case PartType.Body: multiplierDefense = defBody; break;
            case PartType.Hand: multiplierDefense = defHand; break;
            case PartType.Leg: multiplierDefense = defLeg; break;
        }

        // Hitung akumulasi defense multiplier terhadap damage masuk
        float damageAkhir = kekuatanBenturan * multiplierDefense;
        currentHp -= damageAkhir;

        // Tembakkan Pop-up Damage Text di Canvas Dunia 3D
        SpawnDamageText(damageAkhir);

        Debug.Log($"{gameObject.name} terkena damage sebesar {damageAkhir:F1} pada bagian {jenisPart}. Sisa HP: {currentHp:F1}");

        if (currentHp <= 0)
            StartCoroutine(DoDead());

        action?.Invoke(damageAkhir);
    }

    private IEnumerator DoDead()
    {
        isDying = true; // KUNCI STATUS: Gasing ini sedang mati!

        Time.timeScale = 0f;
        GameEvents.CallShake?.Invoke(0.5f, 0.4f);

        HUDUIHandler uiHandler = FindObjectOfType<HUDUIHandler>();
        if (uiHandler != null)
        {
            if (CompareTag("Player")) uiHandler.ShowNotificationLog("<color=red>YOU WERE DESTROYED!</color>");
            else if (CompareTag("Enemy")) uiHandler.ShowNotificationLog("<color=orange>ENEMY ELIMINATED! +5 GOLD</color>");
        }

        yield return new WaitForSecondsRealtime(0.8f);

        Time.timeScale = 0.15f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(1.8f);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        isDying = false; // Buka kembali kuncinya tepat sebelum pindah ronde
        RoundManager.instance.NewRound(gameObject.tag);
    }

    private void SpawnDamageText(float dmgValue)
    {
        if (damageTextPrefab == null) return;

        Vector3 offset = new Vector3(
            UnityEngine.Random.Range(-offsetText, offsetText),
            2f,
            UnityEngine.Random.Range(-offsetText, offsetText)
        );

        DamageTextUI txt = Instantiate(damageTextPrefab, transform.position + offset, Quaternion.identity);
        txt.Setup(dmgValue, CompareTag("Player"));
    }

    public void DecreaseRPM()
    {
        float rpmMinus = UnityEngine.Random.Range(200f, 300f);
        currentRPM = Mathf.Clamp(currentRPM - rpmMinus, minRPM, maxRPM);
    }

    public void IncreaseEnergyAttack(float val)
    {
        currentEnergyAttack = Mathf.Clamp(currentEnergyAttack + val, 0, maxEnergyAttack);
    }

    public void ClaimRewards(float earnedGold, float earnedExp)
    {
        gold += earnedGold;
        exp += earnedExp;
        Debug.Log($"[REWARD] Gold +{earnedGold} | Exp +{earnedExp} claimed successfully.");
    }

    private void GameOverOrKalah()
    {
        Debug.Log($"{gameObject.name} KALAH SEPENUHNYA! Game Over.");
        gameObject.SetActive(false);
    }

    #region Collision Listeners
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
            isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
            isColliding = false;
    }
    #endregion

    #region Buff System Coroutines
    public void ApplyDamageBuff(float additionalDamage, float duration)
    {
        if (damageBuffCoroutine != null)
        {
            StopCoroutine(damageBuffCoroutine);
            damage -= additionalDamage;
        }
        damageBuffCoroutine = StartCoroutine(DamageBuffCoroutine(additionalDamage, duration));
    }

    private IEnumerator DamageBuffCoroutine(float additionalDamage, float duration)
    {
        additionalDamage *= damage;

        damage += additionalDamage;
        Debug.Log($"[BUFF] Damage +{additionalDamage} di-refresh untuk {duration} detik!");
        yield return new WaitForSeconds(duration);
        damage -= additionalDamage;
        damageBuffCoroutine = null;
    }

    public void ApplyInvincibleBuff(float duration)
    {
        if (invincibleBuffCoroutine != null)
            StopCoroutine(invincibleBuffCoroutine);

        invincibleBuffCoroutine = StartCoroutine(InvincibleBuffCoroutine(duration));
    }

    private IEnumerator InvincibleBuffCoroutine(float duration)
    {
        isInvincibleAttack = true;
        Debug.Log($"[BUFF] Kebal di-refresh untuk {duration} detik!");
        yield return new WaitForSeconds(duration);
        isInvincibleAttack = false;
        invincibleBuffCoroutine = null;
    }
    #endregion
}