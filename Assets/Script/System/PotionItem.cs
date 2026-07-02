using UnityEngine;

public class PotionItem : MonoBehaviour
{
    [SerializeField] private PotionData data;
    [SerializeField] private GameObject viewModel;
    [SerializeField] private AudioClip pickupSound;

    private SpriteRenderer spriteRenderer;
    private Rigidbody rb;
    private bool isUsed = false;
    private bool hasLanded = false;

    [Header("Bobbing")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 30f;

    private Vector3 startLocalPos;
    private float startY;

    void Awake()
    {
        if (viewModel == null)
            viewModel = gameObject;

        spriteRenderer = viewModel.GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();

        startLocalPos = viewModel.transform.localPosition;
        startY = startLocalPos.y;
    }

    void Start()
    {
        if (data != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = data.potionSprite;
        }
    }

    void Update()
    {
        if (!hasLanded) return;

        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        Vector3 pos = viewModel.transform.localPosition;
        pos.y = newY;
        viewModel.transform.localPosition = pos;

        viewModel.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;
        hasLanded = true;

        if (rb != null)
            rb.isKinematic = true;

        startLocalPos = viewModel.transform.localPosition;
        startY = startLocalPos.y;
    }

    public void SetupPotion(PotionData newData)
    {
        data = newData;
        if (spriteRenderer == null && viewModel != null)
            spriteRenderer = viewModel.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sprite = data.potionSprite;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (isUsed) return;

            isUsed = true;

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            GasingStat playerStats = other.GetComponentInParent<GasingStat>();

            if (playerStats != null)
            {
                ApplyEffect(playerStats);
                GameEvents.OnPotionCollect?.Invoke();
                Destroy(gameObject);
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
                float healAmount = stats.maxHp * data.effectValue;
                stats.currentHp += healAmount;
                stats.currentHp = Mathf.Clamp(stats.currentHp, 0, stats.maxHp);

                if (uiHandler != null)
                    uiHandler.ShowNotificationLog($"{stats.name} gain {healAmount}hp");

                if (stats.CompareTag("Player"))
                    PostProcessingFX.Instance?.PlayHealEffect();

                Debug.Log($"[POTION] Memulihkan HP sebesar {healAmount}. HP sekarang: {stats.currentHp}");
                break;

            case PotionType.EnergyUlt:
                stats.currentEnergyUltimate += stats.maxEnergyUltimate * data.effectValue;
                stats.currentEnergyUltimate = Mathf.Clamp(stats.currentEnergyUltimate, 0, stats.currentEnergyUltimate);

                if (uiHandler != null)
                    uiHandler.ShowNotificationLog($"{stats.name} gain {stats.maxEnergyUltimate * data.effectValue} Soul Ultimate");

                Debug.Log($"[POTION] Menambah Energy Ult sebesar {data.effectValue}");
                break;

            case PotionType.EnergyAttack:
                stats.currentEnergyAttack += stats.maxEnergyAttack * data.effectValue;
                stats.currentEnergyAttack = Mathf.Clamp(stats.currentEnergyAttack, 0, stats.maxEnergyAttack);

                if (uiHandler != null)
                    uiHandler.ShowNotificationLog($"{stats.name} gain {stats.maxEnergyAttack * data.effectValue} Energy Attack");

                Debug.Log($"[POTION] Menambah Energy Attack sebesar {data.effectValue}");
                break;

            case PotionType.SpeadMove:
                if (movement != null)
                {
                    if (uiHandler != null)
                        uiHandler.ShowNotificationLog($"{stats.name} gain SpeedMove Bonus!");

                    movement.ApplySpeedBuff(data.effectValue, 5f);

                    if (stats.CompareTag("Player"))
                        PostProcessingFX.Instance?.PlaySpeedBoostEffect();
                }

                break;

            case PotionType.Damage:
                if (uiHandler != null)
                    uiHandler.ShowNotificationLog($"{stats.name} gain Attack Bonus!");

                stats.ApplyDamageBuff(data.effectValue, 10f);

                if (stats.CompareTag("Player"))
                    PostProcessingFX.Instance?.PlayAttackEffect();
                break;

            case PotionType.Kebal:
                if (uiHandler != null)
                    uiHandler.ShowNotificationLog($"{stats.name} Immortality for 3s!");

                stats.ApplyInvincibleBuff(data.effectValue);

                if (stats.CompareTag("Player"))
                    PostProcessingFX.Instance?.PlayHealEffect();
                break;

            case PotionType.Box:
                // Efek instant jika ini drop item box biasa
                break;
        }
    }
}