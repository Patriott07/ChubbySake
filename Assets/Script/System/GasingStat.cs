using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class GasingStat : MonoBehaviour
{
    [Header("Stat Dasar Gasing")]
    public string namaGasing = "Chibby Gasing";
    public float maxHp = 100f;
    public float currentHp;
    public int maxNyawa = 3;
    public int currentNyawa;

    [Header("Stat Defense Per Part (Multiplier)")]
    [Tooltip("Semakin kecil nilainya, semakin kuat pertahanannya (misal: 0.5 berarti damage diskon 50%)")]
    public float defHead = 1.0f;
    public float defBody = 0.8f;  // Badan lebih tebal
    public float defHand = 1.2f;  // Tangan/Senjata lebih rentan menerima damage balik
    public float defLeg = 0.9f;

    [Header("Sistem Respawn")]
    public Transform titikRespawn; // Taruh empty GameObject di tengah arena untuk titik respawn
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ResetGasing();
    }

    // Menginisialisasi ulang stat saat game mulai atau reset
    public void ResetGasing()
    {
        currentHp = maxHp;
        currentNyawa = maxNyawa;
    }

    /// <summary>
    /// Fungsi utama yang dipanggil oleh child colliders saat terjadi benturan
    /// </summary>
    public void TerimaDamagePart(GasingPartCollider.PartType jenisPart, float kekuatanBenturan)
    {
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
        float damageAkhir = kekuatanBenturan * multiplierDefense * 0.5f;
        
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
        
        if(currentHp <= 0)
        {
            // fungdi apa gitu ? yang bertanggung jawab atas selesainya ronde
        }
    }

    void Update()
    {
        
    }
}