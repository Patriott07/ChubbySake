using UnityEngine;
using data.structs;
using System.Collections;

public class GasingMovement : MonoBehaviour
{
    [SerializeField] private float shakeStrength, shakeDuration;
    private Rigidbody rb;
    private GasingStat stats; // Referensi ke script GasingStat

    [Header("Visualization")]
    public float speedRotateModel = 500f;
    // Tambahkan variabel LineRenderer dan panjang garis indikator
    [SerializeField] private LineRenderer directionLine;
    [SerializeField] private float lineLength = 2f;

    [Header("Movement Otomatis")]
    public float autoSpeed = 5f;
    public float changeDirectionInterval = 2f;
    private Vector3 randomDirection;

    [Header("Kontrol Player (Support)")]
    public float playerForceMultiplier = 0.1f; // Rasio 1:10
    [SerializeField] bool isControlSupport = false;

    [SerializeField] private float knockbackForce = 10f;

    [Header("Physics Cooldown")]
    private bool isInvincible = false;
    [SerializeField] private float hitCooldown = 0.2f; // Jeda waktu antar tabrakan


    // COROUTINE
    private Coroutine speedBuffCoroutine = null;
    private float originalSpeedBackup = 0f;
    private bool isSpeedBuffActive = false;

    void Awake()
    {

    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        stats = gameObject.GetComponent<GasingStat>();

        // Setup awal Line Renderer jika belum di-assign di Inspector
        if (directionLine == null)
        {
            directionLine = GetComponent<LineRenderer>();
        }

        if (directionLine != null)
        {
            directionLine.positionCount = 2; // Butuh 2 titik: Pangkal (gasing) dan Ujung (arah tujuan)
        }

        // Memulai pergerakan acak gasing
        StartCoroutine(ChangeDirectionRoutine());
    }

    void Update()
    {
        // Tetap pastikan gasing berputar secara visual
        if (stats != null)
        {
            // 3. currentRPM memengaruhi kecepatan rotasi visual
            float totalRotationSpeed = speedRotateModel + (2f * stats.currentRPM);
            transform.Rotate(Vector3.up * totalRotationSpeed * Time.deltaTime);

            // 4. currentRPM memengaruhi playerForceMultiplier (Min 0.04, Max 0.2)
            // Menggunakan persentase RPM saat ini dibandingkan dengan RPM maksimumnya
            // float rpmPercentage = (stats.currentRPM - stats.minRPM) / (stats.maxRPM - stats.minRPM);
            // playerForceMultiplier = Mathf.Lerp(0.002f, 0.045f, rpmPercentage);
        }
        else
        {
            // Fallback jika komponen stat tidak ada
            transform.Rotate(Vector3.up * speedRotateModel * Time.deltaTime);
        }

        // Update visualisasi garis arah di setiap frame
        DrawDirectionLine();
    }

    void FixedUpdate()
    {
        // 1. Gerakan Otomatis Gasing
        Vector3 autoMovement = randomDirection * autoSpeed;
        rb.AddForce(new Vector3(autoMovement.x, 0f, autoMovement.z), ForceMode.Force);

        if (!isControlSupport) return;

        // 2. Dorongan Kecil dari Player (Input WASD / Arrow)
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 playerInput = new Vector3(moveHorizontal, 0f, moveVertical).normalized;

        if (playerInput.magnitude > 0.1f)
        {
            // Memberikan dorongan kecil (dikali 0.1)
            rb.AddForce(playerInput * autoSpeed * playerForceMultiplier, ForceMode.Impulse);
        }
    }

    // Fungsi khusus menggambar garis penunjuk arah gerak gasing
    void DrawDirectionLine()
    {
        if (directionLine != null)
        {
            // Titik 0: Posisi gasing saat ini (sedikit diturunkan/dinaikkan di sumbu Y agar tidak tembus lantai)
            Vector3 startPos = transform.position + Vector3.up * 0.3f;

            // Titik 1: Posisi gasing ditambah arah gerak dikali panjang garis yang diinginkan
            Vector3 endPos = startPos + (new Vector3(randomDirection.x, 0f, randomDirection.z) * lineLength);

            directionLine.SetPosition(0, startPos);
            directionLine.SetPosition(1, endPos);
        }
    }

    IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            // Mengambil arah acak di bidang datar (X dan Z)
            float randomX = Random.Range(-1f, 1f);
            float randomZ = Random.Range(-1f, 1f);
            randomDirection = new Vector3(randomX, 0f, randomZ).normalized;

            // setiap dua detik bakal jalanin fungsi ini lg
            yield return new WaitForSeconds(changeDirectionInterval);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // JIKA SEDANG COOLDOWN, ABAIKAN TABRAKAN INI (Mencegah Spam Multi-Hit)
            if (isInvincible) return;

            Vector3 enemyPosition = collision.transform.position;
            Vector3 myPosition = transform.position;

            Vector3 knockbackDirection = (myPosition - enemyPosition).normalized;
            Vector3 enemyKnockbackDir = (enemyPosition - myPosition).normalized;

            GameEvents.CallShake.Invoke(shakeDuration, shakeStrength);
            Rigidbody rb = GetComponent<Rigidbody>();

            // Aktifkan cooldown langsung setelah tabrakan pertama terdeteksi
            StartCoroutine(CollisionCooldownRoutine());

            StartCoroutine(SlowMotionEffect());
            if (rb != null)
            {
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

                // Menjalankan fungsi Coroutine untuk efek slow-mo
            }
            else
            {
                Debug.LogWarning("Objek ini tidak memiliki Rigidbody untuk knockback!");
            }

            // 3. KNOCKBACK UNTUK MUSUH (ENEMY)
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(enemyKnockbackDir * (knockbackForce * 0.2f), ForceMode.Impulse);
            }
            else
            {
                Debug.LogWarning("Musuh tidak punya Rigidbody! Pastikan Enemy memiliki Rigidbody.");
            }

            // DAMAGE
            // Trigger sistem damage mandiri (mengurangi HP & RPM)
            GasingStat enemyStats = collision.gameObject.GetComponent<GasingStat>();

            if (stats != null)
            {
                // Anggap kekuatan benturan dasar berdasarkan kecepatan gerak gasing (misal: 10f)
                stats.TerimaDamagePart(PartType.Body, enemyStats.damage, null);
                stats.TerimaDamagePart(PartType.Body, stats.damage + (0.02f * enemyStats.currentRPM), null);

                if (enemyStats.damageTambahanQTE > 0)
                {
                    stats.TerimaDamagePart(PartType.Body, enemyStats.damageTambahanQTE, null);
                    enemyStats.damageTambahanQTE = 0;
                }
                enemyStats.DecreaseRPM();
                stats.IncreaseEnergyAttack(UnityEngine.Random.Range(100, 150));
            }

            // Trigger musuh agar menerima damage juga jika musuh punya GasingStat
            if (enemyStats != null)
            {
                enemyStats.TerimaDamagePart(PartType.Body, stats.damage, null);
                enemyStats.TerimaDamagePart(PartType.Body, stats.damage + (0.02f * stats.currentRPM), null);
                if (stats.damageTambahanQTE > 0)
                {
                    enemyStats.TerimaDamagePart(PartType.Body, stats.damageTambahanQTE, null);
                    stats.damageTambahanQTE = 0;
                    Debug.Log($"ENEMY GET BONUS ATTACK : {stats.damageTambahanQTE}");
                }
                stats.DecreaseRPM();
                enemyStats.IncreaseEnergyAttack(UnityEngine.Random.Range(5, 8));
            }
        }
    }

    // 3. Tambahkan Coroutine baru ini di bagian bawah script kamu
    private IEnumerator CollisionCooldownRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(hitCooldown);
        isInvincible = false;
    }

    private IEnumerator SlowMotionEffect()
    {
        // Jika gasing ini (atau gasing lawan) sedang dalam proses mati, batalkan slow-mo tabrakan biasa!
        if (stats != null && stats.isDying) yield break;

        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(0.75f);
        
        // FIX KEBOCORAN: Hanya kembalikan ke waktu normal jika TIDAK ADA gasing yang sedang mati
        if (stats != null && !stats.isDying)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    public void ApplySpeedBuff(float speedMultiplier, float duration)
    {
        // KUNCI: Jika sedang nge-buff speed, hentikan detikan lamanya
        if (speedBuffCoroutine != null)
        {
            StopCoroutine(speedBuffCoroutine);
            // Kembalikan ke speed awal dari backup agar tidak dikalikan terus-menerus menjadi super cepat
            autoSpeed = originalSpeedBackup;
        }

        speedBuffCoroutine = StartCoroutine(SpeedBuffCoroutine(speedMultiplier, duration));
    }

    private IEnumerator SpeedBuffCoroutine(float speedMultiplier, float duration)
    {
        // Ambil backup kecepatan asli HANYA jika ini adalah ramuan pertama
        if (!isSpeedBuffActive)
        {
            originalSpeedBackup = autoSpeed;
            isSpeedBuffActive = true;
        }

        autoSpeed *= speedMultiplier;
        Debug.Log($"[BUFF] Speed x{speedMultiplier}. Timer di-refresh kembali ke {duration} detik!");

        yield return new WaitForSeconds(duration);

        // Setelah durasi penuh selesai tanpa gangguan ramuan baru, kembalikan ke awal
        autoSpeed = originalSpeedBackup;
        isSpeedBuffActive = false;
        speedBuffCoroutine = null; // Reset referensi
        Debug.Log("[BUFF] Efek Speed kembali normal.");
    }
}
