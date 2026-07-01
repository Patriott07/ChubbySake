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
        HUDUIHandler uiHandler = FindObjectOfType<HUDUIHandler>();

        switch (data.type)
        {
            case PotionType.Health:
                // Efek Langsung (Instant)
                float healAmount = stats.maxHp * data.effectValue;
                stats.currentHp += healAmount;
                stats.currentHp = Mathf.Clamp(stats.currentHp, 0, stats.maxHp);

                if (uiHandler != null)
                {
                    uiHandler.ShowNotificationLog($"{stats.name} gain {healAmount}hp");
                }

                Debug.Log($"[POTION] Memulihkan HP sebesar {healAmount}. HP sekarang: {stats.currentHp}");
                break;

            case PotionType.EnergyUlt:
                // Efek Langsung (Instant)
                stats.currentEnergyUltimate += stats.maxEnergyUltimate * data.effectValue; // Sesuaikan variabel Ulti kamu
                stats.currentEnergyUltimate = Mathf.Clamp(stats.currentEnergyUltimate, 0, stats.currentEnergyUltimate);

                if (uiHandler != null)
                {
                    // stats.gameObject.name bakal memunculkan nama gasing ("RUSIDD" atau "GEMINI0040")
                    uiHandler.ShowNotificationLog($"{stats.name} gain {stats.maxEnergyUltimate * data.effectValue} Soul Ultimate");
                }
                Debug.Log($"[POTION] Menambah Energy Ult sebesar {data.effectValue}");
                break;

            case PotionType.EnergyAttack:
                // Efek Langsung (Instant)
                stats.currentEnergyAttack += stats.maxEnergyAttack * data.effectValue;
                stats.currentEnergyAttack = Mathf.Clamp(stats.currentEnergyAttack, 0, stats.maxEnergyAttack);

                if (uiHandler != null)
                {
                    // stats.gameObject.name bakal memunculkan nama gasing ("RUSIDD" atau "GEMINI0040")
                    uiHandler.ShowNotificationLog($"{stats.name} gain {stats.maxEnergyAttack * data.effectValue} Energy Attack");
                }

                Debug.Log($"[POTION] Menambah Energy Attack sebesar {data.effectValue}");
                break;

            case PotionType.SpeadMove:
                // Efek Berdurasi: Kirim perintah Coroutine ke komponen pergerakan gasing
                if (movement != null)
                {
                    if (uiHandler != null)
                    {
                        // stats.gameObject.name bakal memunculkan nama gasing ("RUSIDD" atau "GEMINI0040")
                        uiHandler.ShowNotificationLog($"{stats.name} gain SpeedMove Bonus!");
                    }
                    movement.ApplySpeedBuff(data.effectValue, 3f);
                }

                break;

            case PotionType.Damage:
                // Efek Berdurasi: Kirim perintah Coroutine ke stats gasing
                if (uiHandler != null)
                {
                    // stats.gameObject.name bakal memunculkan nama gasing ("RUSIDD" atau "GEMINI0040")
                    uiHandler.ShowNotificationLog($"{stats.name} gain Attack Bonus!");
                }

                stats.ApplyDamageBuff(data.effectValue, 10f);
                break;

            case PotionType.Kebal:
                // Efek Berdurasi: Kirim perintah Coroutine ke stats gasing
                if (uiHandler != null)
                {
                    // stats.gameObject.name bakal memunculkan nama gasing ("RUSIDD" atau "GEMINI0040")
                    uiHandler.ShowNotificationLog($"{stats.name} Immortality for 3s!");
                }
                stats.ApplyInvincibleBuff(3f);
                break;

            case PotionType.Box:
                // Efek instant jika ini drop item box biasa
                break;
        }
    }
}