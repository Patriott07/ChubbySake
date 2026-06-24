using UnityEngine;

public class PotionItem : MonoBehaviour
{
    [SerializeField] private PotionData data;
    private SpriteRenderer spriteRenderer;
    private bool isUsed = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Otomatis pasang gambar sesuai data ramuan
        if (data != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = data.potionSprite;
        }
    }

    // Fungsi untuk menyuntikkan data secara dinamis dari Spawner
    public void SetupPotion(PotionData newData)
    {
        data = newData;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.sprite = data.potionSprite;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Pastikan objek yang menabrak memiliki komponen stats game kamu
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (isUsed) return;

            isUsed = true;
            Debug.Log("Get Potion");
            GasingStat playerStats = other.GetComponentInParent<GasingStat>();

            if (playerStats != null)
            {
                ApplyEffect(playerStats);
                GameEvents.OnPotionCollect?.Invoke();
                Destroy(gameObject); // Hancurkan botol setelah diambil
            }
        }
    }

    private void ApplyEffect(GasingStat stats)
    {
        if (data == null) return;

        // Kita ambil juga referensi GasingMovement untuk memanipulasi speed jika ada
        GasingMovement movement = stats.gameObject.GetComponent<GasingMovement>();
        
        switch (data.type)
        {
            case PotionType.Health:
                // Efek Langsung (Instant)
                stats.currentHp += data.effectValue; // Menggunakan penjumlahan flat agar aman
                stats.currentHp = Mathf.Clamp(stats.currentHp, 0, stats.maxHp);
                Debug.Log($"[POTION] Memulihkan HP sebesar {data.effectValue}. HP sekarang: {stats.currentHp}");
                break;

            case PotionType.EnergyUlt:
                // Efek Langsung (Instant)
                // stats.currentEnergyUlt += data.effectValue; // Sesuaikan variabel Ulti kamu
                stats.currentEnergyUltimate = Mathf.Clamp(stats.currentEnergyUltimate + data.effectValue, 0, stats.currentEnergyUltimate);
                Debug.Log($"[POTION] Menambah Energy Ult sebesar {data.effectValue}");
                break;

            case PotionType.EnergyAttack:
                // Efek Langsung (Instant)
                stats.currentEnergyAttack = Mathf.Clamp(stats.currentEnergyAttack + data.effectValue, 0, stats.maxEnergyAttack);
                Debug.Log($"[POTION] Menambah Energy Attack sebesar {data.effectValue}");
                break;

            case PotionType.SpeadMove:
                // Efek Berdurasi: Kirim perintah Coroutine ke komponen pergerakan gasing
                if (movement != null)
                {
                    movement.ApplySpeedBuff(data.effectValue, 3f);
                }
                
                break;

            case PotionType.Damage:
                // Efek Berdurasi: Kirim perintah Coroutine ke stats gasing
                stats.ApplyDamageBuff(data.effectValue, 10f);
                break;

            case PotionType.Kebal:
                // Efek Berdurasi: Kirim perintah Coroutine ke stats gasing
                stats.ApplyInvincibleBuff(3f);
                break;

            case PotionType.Box:
                // Efek instant jika ini drop item box biasa
                break;
        }
    }
}