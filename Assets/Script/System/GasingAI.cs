using UnityEngine;
using System.Collections;

public class GasingAI : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard, Impossible }

    [Header("AI Settings")]
    public Difficulty tingkatKesulitan = Difficulty.Medium;

    // Tempat menyimpan referensi transform Player
    private Transform playerTransform;
    private Rigidbody rb;
    private GasingMovement movementScript;
    private GasingStat stats;

    [Header("Tuning Kesulitan AI")]
    [Tooltip("Seberapa sering AI memperbarui arah ke target (detik). Semakin kecil, semakin agresif.")]
    private float aiDecisionInterval = 1f;
    [Tooltip("Persentase peluang AI bakal beneran ngejar player saat interval kecerdasan berbunyi.")]
    private float chaseChance = 0.5f;

    [Header("Attack Settings")]
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float attackDashSpeed = 40f;
    [SerializeField] private float energyGainRate = 5f; // Energi yang bertambah per detik secara otomatis
    private bool isAttacking = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<GasingMovement>();
        stats = GetComponent<GasingStat>();

        // Mencari objek player di arena berdasarkan Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // Setup stat awal berdasarkan tingkat kesulitan yang dipilih
        InisialisasiKesulitan();

        // Overwrite coroutine bawaan GasingMovement agar dikontrol oleh kecerdasan AI
        if (movementScript != null)
        {
            movementScript.StopAllCoroutines(); // Matikan gerakan acak murni bawaannya
            StartCoroutine(AIDecisionRoutine());
        }
    }

    void Update()
    {
        if (stats == null) return;

        // 1. Simulasi pengisian energi otomatis seiring berjalannya waktu (tanpa terpengaruh slow-mo jika pakai unscaledDeltaTime)
        if (stats.currentEnergyAttack < stats.maxEnergyAttack)
        {
            stats.IncreaseEnergyAttack(energyGainRate * Time.unscaledDeltaTime);
        }

        // 2. Cek apakah energi sudah mencapai batas Maksimal
        if (stats.currentEnergyAttack >= stats.maxEnergyAttack)
        {
            ExecuteAutomaticAttack();
        }
    }

    float CalculateRandomBonusDamage(Difficulty mode)
    {
        // Menggunakan teknik pencarian acak yang rentangnya disesuaikan dengan mode permainan
        switch (mode)
        {
            case Difficulty.Easy:
                return Random.Range(5f, 15f); // Damage bonus relatif kecil/aman
            case Difficulty.Medium:
                return Random.Range(15f, 30f);
            case Difficulty.Hard:
                return Random.Range(30f, 50f); // Damage bonus sangat sakit
            case Difficulty.Impossible:
                return Random.Range(50f, 200f); // Damage bonus sangat sakit
            default:
                return 0f;
        }
    }

    void ExecuteAutomaticAttack()
    {
        // Cegah double attack jika coroutine sedang berjalan
        if (isAttacking) return;

        // Picu sequence attack menggunakan Coroutine
        StartCoroutine(StartEnemyAttackSequence());
    }

    private IEnumerator StartEnemyAttackSequence()
    {
        isAttacking = true;

        // 1. Hentikan pergerakan otomatis AI & reset kecepatan fisik saat bersiap
        if (movementScript != null) movementScript.enabled = false;
        rb.linearVelocity = Vector3.zero;

        // Ambil kalkulasi damage dasar + bonus acak berdasarkan tingkat kesulitan
        float bonusDamage = CalculateRandomBonusDamage(tingkatKesulitan);
        stats.damageTambahanQTE = bonusDamage; // Set bonus damage ke stat musuh

        // Efek visual background (opsional jika musuh juga pakai efek menggelapkan layar)
        // if (canvasGroupBgBlack != null) canvasGroupBgBlack.DOFade(0.7f, 0.4f).SetUpdate(true);

        // 2. Game berjalan sangat lambat (Efek Slow-mo Dramatis)
        Time.timeScale = 0.05f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // KUNCI POSISI SEBELUM DELAY (Ambil arah target saat ini)
        Vector3 attackDir = Vector3.forward; // Default fallback
        if (playerTransform != null)
        {
            attackDir = (playerTransform.position - transform.position).normalized;
            attackDir.y = 0; // Kunci sumbu Y agar tidak terbang/menembus wajan arena
        }

        Debug.Log($"[AI Attack] Bersiap menerjang! Mengunci arah ke player. Masuk jeda 1 detik...");

        // 3. JEDA 1 DETIK (Menggunakan unscaledTime karena game sedang slow-mo)
        float chargeTimer = 0f;
        while (chargeTimer < 0.6f)
        {
            attackDir = (playerTransform.position - transform.position).normalized;
            chargeTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Kembalikan visual background
        // if (canvasGroupBgBlack != null) canvasGroupBgBlack.DOFade(0f, 0.4f).SetUpdate(true);

        // Kembalikan waktu game menjadi normal TEPAT sebelum menerjang maju
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // 4. LAKUKAN TERJANGAN LURUS (NON-HOMING)
        stats.isInvincibleAttack = true; // Aktifkan kebal selama menerjang

        float currentDashTime = 0f;
        while (currentDashTime < dashDuration)
        {
            // Melesat konstan ke arah 'attackDir' yang sudah dikunci sebelumnya (TIDAK diupdate tiap frame)
            rb.linearVelocity = attackDir * attackDashSpeed;

            currentDashTime += Time.deltaTime;
            yield return null;
        }

        // 5. SELESAI ATTACK - Reset State & Kembalikan Kontrol Pergerakan AI
        rb.linearVelocity = Vector3.zero;
        if (movementScript != null) movementScript.enabled = true;

        stats.isInvincibleAttack = false; // Matikan status kebal
        stats.currentEnergyAttack = 0f;    // Reset energi kembali ke 0
        stats.damageTambahanQTE = 0f;      // Reset bonus damage

        isAttacking = false;
        Debug.Log("[AI Attack] Terjangan selesai, AI kembali ke mode normal.");
    }

    void InisialisasiKesulitan()
    {
        switch (tingkatKesulitan)
        {
            case Difficulty.Easy:
                aiDecisionInterval = 1.5f; // Lambat mikir
                chaseChance = 0.3f;        // Jarang ngejar, sering gabut
                if (movementScript != null) movementScript.autoSpeed = 3f;
                if (stats != null)
                {
                    stats.damage *= 0.8f;
                    stats.maxHp *= 0.8f;
                    stats.rpmRegenSpeed *= 0.8f;
                }
                break;

            case Difficulty.Medium:
                aiDecisionInterval = 0.8f;
                chaseChance = 0.6f;
                if (movementScript != null) movementScript.autoSpeed = 5f;
                if (stats != null)
                {
                    stats.damage *= 1.05f;
                    stats.maxHp *= 1.05f;
                    stats.rpmRegenSpeed *= 1.05f;
                }
                break;

            case Difficulty.Hard:
                aiDecisionInterval = 0.3f; // Cepat banget mikir
                chaseChance = 0.9f;        // Hampir selalu ngejar target
                if (movementScript != null) movementScript.autoSpeed = 6.5f;
                if (stats != null)
                {
                    stats.damage *= 1.2f;
                    stats.maxHp *= 2f;
                    stats.maxRPM *= 1.25f;
                    stats.minRPM *= 1.25f;
                    stats.rpmRegenSpeed *= 1.5f;
                    stats.maxEnergyAttack *= 0.75f;
                    stats.maxEnergyUltimate *= 0.9f;
                }
                break;

            case Difficulty.Impossible:
                aiDecisionInterval = 0.05f; // Instan tanpa jeda
                chaseChance = 1f;          // 100% ngejar terus tanpa ampun
                if (movementScript != null) movementScript.autoSpeed = 8f; // Super cepat
                if (stats != null)
                {
                    stats.damage *= 1.5f;
                    stats.maxHp *= 3f;
                    stats.maxRPM *= 1.55f;
                    stats.minRPM *= 1.8f;
                    stats.rpmRegenSpeed *= 2f;
                    stats.maxEnergyAttack *= 0.55f;
                    stats.maxEnergyUltimate *= 0.8f;
                }
                break;
        }

        // Reset HP musuh agar sinkron dengan setelan kesulitan baru
        if (stats != null) stats.ResetGasing();
    }

    IEnumerator AIDecisionRoutine()
    {
        // Beri jeda sedikit di awal match agar tidak langsung nyerang pas spawn
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (playerTransform != null && playerTransform.gameObject.activeInHierarchy)
            {
                // Lempar dadu acak untuk menentukan apakah AI mau menyerang atau gerak bebas
                if (Random.value <= chaseChance)
                {
                    // KEJAR PLAYER: Hitung arah menuju posisi koordinat player
                    Vector3 arahKePlayer = (playerTransform.position - transform.position).normalized;
                    arahKePlayer.y = 0; // Kunci sumbu Y agar tidak terbang

                    // Suntikkan arah target ke variabel pergerakan di GasingMovement
                    SetMovementDirection(arahKePlayer);
                }
                else
                {
                    // GABUT / GERAK ACAK: Lakukan gerakan melenceng random
                    Vector3 arahAcak = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                    SetMovementDirection(arahAcak);
                }
            }
            else
            {
                // Jika player mati/tidak ada, AI otomatis berputar acak santai di tempat
                Vector3 arahSantai = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                SetMovementDirection(arahSantai);
            }

            yield return new WaitForSeconds(aiDecisionInterval);
        }
    }

    // Fungsi pembantu untuk mengakses paksa variabel arah di GasingMovement
    void SetMovementDirection(Vector3 dir)
    {
        if (movementScript != null)
        {
            // Kita gunakan pencarian lewat Reflection / helper karena randomDirection di skrip utamamu bersifat private
            System.Reflection.FieldInfo field = typeof(GasingMovement).GetField("randomDirection",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(movementScript, dir);
            }
        }
    }
}