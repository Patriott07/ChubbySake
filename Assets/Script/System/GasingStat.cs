using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
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
    public float rpmRegenSpeed = 50f; // Jumlah RPM yang bertambah per detik saat bebas
    public float currentEnergyAttack = 0; // max 100
    public float maxEnergyAttack = 100;
    public float currentEnergyUltimate = 0; // max 100
    public float maxEnergyUltimate = 100;

    [Header("Stat Defense Per Part (Multiplier)")]
    [Tooltip("Semakin kecil nilainya, semakin kuat pertahanannya (misal: 0.5 berarti damage diskon 50%)")]
    public float defHead = 1.0f;
    public float defBody = 0.8f;  // Badan lebih tebal
    public float defHand = 1.2f;  // Tangan/Senjata lebih rentan menerima damage balik
    public float defLeg = 0.9f;

    [Header("Sistem Respawn")]
    public Transform titikRespawn; // Taruh empty GameObject di tengah arena untuk titik respawn
    private Rigidbody rb;

    [Header("Status Action Attack")]
    public bool isInvincibleAttack = false; // Efek kebal saat menyerang
    public float damageTambahanQTE;
    private bool isColliding = false; // Untuk cek apakah boleh regen RPM

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (GetComponent<GasingAI>() == null)
            ResetGasing();
    }

    void Update()
    {
        // 5. currentRPM bertambah jika TIDAK bersentuhan dengan enemy / obstacle
        if (!isColliding && currentRPM < maxRPM)
        {
            currentRPM += rpmRegenSpeed * Time.deltaTime;
            currentRPM = Mathf.Clamp(currentRPM, minRPM, maxRPM); // Gaboleh lebih dari max
        }
    }

    // Menginisialisasi ulang stat saat game mulai atau reset
    public void ResetGasing()
    {
        currentHp = maxHp;
        currentNyawa = maxNyawa;
        currentRPM = maxRPM / 4; // Mulai dengan RPM penuh
    }

    /// <summary>
    /// Fungsi utama yang dipanggil oleh child colliders saat terjadi benturan
    /// </summary>
    public void TerimaDamagePart(GasingPartCollider.PartType jenisPart, float kekuatanBenturan, Action<float> action)
    {
        // Jika sedang dalam mode Action Attack yang kebal, abaikan semua damage masuk
        if (isInvincibleAttack || kekuatanBenturan <= 0) return;

        // Jika darah sudah habis atau nyawa habis, abaikan kalkulasi
        if (currentHp <= 0 || currentNyawa <= 0) return;

        float multiplierDefense = 1.0f;

        // Tentukan modifier damage berdasarkan part yang kena tabrak
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

        // Rumus Damage Dasar: Kekuatan Benturan x Faktor Defense Part
        // Kamu bisa mengalikan dengan konstanta (misal 0.5f) jika dirasa terlalu sakit
        // float damageAkhir = kekuatanBenturan * multiplierDefense * 0.5f;
        float damageAkhir = kekuatanBenturan;

        // Kurangi HP
        currentHp -= damageAkhir;
        Debug.Log($"{gameObject.name} terkena damage sebesar {damageAkhir:F1} pada bagian {jenisPart}. Sisa HP: {currentHp:F1}");

        // Trigger efek guncangan kamera jika ini adalah Player
        if (gameObject.CompareTag("Player"))
        {
            ActionCamera cam = Camera.main.GetComponent<ActionCamera>();
            if (cam != null && damageAkhir > 2f)
            {
                // cam.TriggerShake(0.15f, damageAkhir * 0.05f);
            }
        }

        // Cek apakah gasing hancur/kalah karena HP habis
        if (currentHp <= 0)
        {
            KurangiNyawaDanRespawn();
        }

        action?.Invoke(damageAkhir);


    }

    public void DecreaseRPM()
    {
        // 2. currentRPM berkurang 200 sampai 300 secara acak
        float rpmMinus = UnityEngine.Random.Range(200f, 300f);
        currentRPM -= rpmMinus;
        currentRPM = Mathf.Clamp(currentRPM, minRPM, maxRPM); // Gaboleh kurang dari min
    }

    public void IncreaseEnergyAttack(float val)
    {
        currentEnergyAttack += val;
        currentEnergyAttack = Mathf.Clamp(currentEnergyAttack, 0, maxEnergyAttack); // Gaboleh kurang dari min
    }

    /// <summary>
    /// Fungsi untuk menangani pengurangan nyawa (dipanggil saat HP habis ATAU jatuh keluar arena)
    /// </summary>
    public void KurangiNyawaDanRespawn()
    {
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
        // Hentikan pergerakan fisik gasing sesaat saat mati
        rb.linearVelocity = Vector3.zero; // Catatan: Gunakan rb.velocity jika kamu pakai Unity versi lama
        rb.angularVelocity = Vector3.zero;

        // Pindahkan ke titik aman (tengah arena)
        if (titikRespawn != null)
        {
            transform.position = titikRespawn.position;
        }
        else
        {
            transform.position = Vector3.up * 2f; // Default jika lupa pasang titik respawn
        }

        // Reset HP kembali penuh untuk nyawa berikutnya
        currentHp = maxHp;
        currentRPM = maxRPM; // Reset RPM pas gasing respawn

        Debug.Log($"{gameObject.name} telah respawn di arena!");
        yield return null;
    }

    private void GameOverOrKalah()
    {
        Debug.Log($"{gameObject.name} KALAH SEPENUHNYA! Game Over.");
        // Nanti di sini kita panggil UI Game Over / Pemenang
        gameObject.SetActive(false);
    }


    void HealthPlayerDecrease(float playerHPDec)
    {
        currentHp -= playerHPDec;

        if (currentHp <= 0)
        {
            // fungdi apa gitu ? yang bertanggung jawab atas selesainya ronde
        }
    }


    // Mengatur kondisi deteksi tabrakan untuk fungsi pasif nomor 5
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