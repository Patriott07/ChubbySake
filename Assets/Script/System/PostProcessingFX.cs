using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PostProcessingFX : MonoBehaviour
{
    public static PostProcessingFX Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GasingStat playerStats;

    [Header("Vignette HP Settings")]
    [SerializeField] private float thresholdHealthPercent = 0.2f;
    [SerializeField] private float maxVignetteIntensity = 0.65f;
    [SerializeField] private Color lowHealthColor = new Color(0.8f, 0f, 0f);
    [SerializeField] private Color healColor = new Color(0f, 0.9f, 0.3f);
    [SerializeField] private float smoothSpeed = 3f;

    [Header("Energy Pulse Settings")]
    [SerializeField] private float pulseAmplitude = 0.15f;
    [SerializeField] private float pulseFrequency = 3f;

    private Volume volume;
    private Vignette vignette;
    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;

    private float originalVignetteIntensity;
    private Color originalVignetteColor;
    private float originalBloomIntensity;
    private Color originalBloomTint;
    private float originalChromaticAberrationIntensity;
    private float originalLensDistortionIntensity;
    private float originalSaturation;
    private float originalPostExposure;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerStats = player.GetComponent<GasingStat>();
        }

        volume = FindObjectOfType<Volume>();
        if (volume == null || volume.profile == null)
        {
            Debug.LogError("[PostProcessingFX] Volume not found!");
            return;
        }

        GetOrAddOverrides();
        StoreOriginalValues();
        EnableRequiredOverrideStates();

        Debug.Log("[PostProcessingFX] Initialized successfully.");
    }

    private void GetOrAddOverrides()
    {
        if (!volume.profile.TryGet(out vignette)) vignette = volume.profile.Add<Vignette>(true);
        if (!volume.profile.TryGet(out bloom)) bloom = volume.profile.Add<Bloom>(true);
        if (!volume.profile.TryGet(out chromaticAberration)) chromaticAberration = volume.profile.Add<ChromaticAberration>(true);
        if (!volume.profile.TryGet(out lensDistortion)) lensDistortion = volume.profile.Add<LensDistortion>(true);
        if (!volume.profile.TryGet(out colorAdjustments)) colorAdjustments = volume.profile.Add<ColorAdjustments>(true);
    }

    private void StoreOriginalValues()
    {
        originalVignetteIntensity = vignette.intensity.value;
        originalVignetteColor = vignette.color.value;
        originalBloomIntensity = bloom.intensity.value;
        originalBloomTint = bloom.tint.value;
        originalChromaticAberrationIntensity = chromaticAberration.intensity.value;
        originalLensDistortionIntensity = lensDistortion.intensity.value;
        originalSaturation = colorAdjustments.saturation.value;
        originalPostExposure = colorAdjustments.postExposure.value;
    }

    private void EnableRequiredOverrideStates()
    {
        vignette.intensity.overrideState = true;
        vignette.color.overrideState = true;
        vignette.smoothness.overrideState = true;

        bloom.intensity.overrideState = true;
        bloom.tint.overrideState = true;
        bloom.threshold.overrideState = true;

        chromaticAberration.intensity.overrideState = true;

        lensDistortion.intensity.overrideState = true;

        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.postExposure.overrideState = true;
    }

    void Update()
    {
        UpdateHealthVignette();
        UpdateEnergyPulse();
    }

    #region Continuous Effects

    private void UpdateHealthVignette()
    {
        if (playerStats == null || vignette == null) return;

        float hpPercent = playerStats.currentHp / playerStats.maxHp;
        float targetIntensity;
        Color targetColor;

        if (hpPercent <= thresholdHealthPercent)
        {
            float t = 1f - Mathf.Clamp01(hpPercent / thresholdHealthPercent);
            targetIntensity = Mathf.Lerp(originalVignetteIntensity, maxVignetteIntensity, t);
            targetColor = Color.Lerp(originalVignetteColor, lowHealthColor, t);
        }
        else
        {
            targetIntensity = originalVignetteIntensity;
            targetColor = originalVignetteColor;
        }

        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, smoothSpeed * Time.deltaTime);
        vignette.color.value = Color.Lerp(vignette.color.value, targetColor, smoothSpeed * Time.deltaTime);
    }

    private void UpdateEnergyPulse()
    {
        if (playerStats == null || bloom == null) return;

        if (playerStats.currentEnergyAttack >= playerStats.maxEnergyAttack)
        {
            float pulse = Mathf.Sin(Time.time * pulseFrequency) * pulseAmplitude + pulseAmplitude;
            bloom.intensity.value = Mathf.Lerp(bloom.intensity.value, originalBloomIntensity + pulse, smoothSpeed * Time.deltaTime);
        }
        else
        {
            bloom.intensity.value = Mathf.Lerp(bloom.intensity.value, originalBloomIntensity, smoothSpeed * 2f * Time.deltaTime);
        }
    }

    #endregion

    #region One-Shot Effects

    public void PlayAttackEffect()
    {
        StartCoroutine(AttackEffectRoutine());
    }

    private IEnumerator AttackEffectRoutine()
    {
        float duration = 0.4f;
        float elapsed = 0f;

        chromaticAberration.intensity.value = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float spike = Mathf.Sin(t * Mathf.PI);

            chromaticAberration.intensity.value = Mathf.Lerp(originalChromaticAberrationIntensity, 0.6f, spike);
            lensDistortion.intensity.value = Mathf.Lerp(originalLensDistortionIntensity, -0.25f, spike);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        chromaticAberration.intensity.value = originalChromaticAberrationIntensity;
        lensDistortion.intensity.value = originalLensDistortionIntensity;
    }

    public void PlayHealEffect()
    {
        StartCoroutine(HealEffectRoutine());
    }

    private IEnumerator HealEffectRoutine()
    {
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);

            vignette.color.value = Color.Lerp(healColor, originalVignetteColor, easeOut);
            vignette.intensity.value = Mathf.Lerp(originalVignetteIntensity + 0.35f, originalVignetteIntensity, easeOut);

            if (bloom != null)
            {
                float bloomBoost = Mathf.Lerp(0.4f, 0f, easeOut);
                bloom.tint.value = Color.Lerp(new Color(0.3f, 1f, 0.5f), originalBloomTint, easeOut);
                bloom.intensity.value = originalBloomIntensity + bloomBoost;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        vignette.color.value = originalVignetteColor;
        vignette.intensity.value = originalVignetteIntensity;
        if (bloom != null)
            bloom.tint.value = originalBloomTint;
    }

    public void PlayRewardEffect()
    {
        StartCoroutine(RewardEffectRoutine());
    }

    private IEnumerator RewardEffectRoutine()
    {
        float duration = 0.8f;
        float elapsed = 0f;
        Color goldColor = new Color(1f, 0.8f, 0.2f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);

            if (bloom != null)
            {
                bloom.intensity.value = originalBloomIntensity + Mathf.Lerp(0.7f, 0f, easeOut);
                bloom.tint.value = Color.Lerp(goldColor, originalBloomTint, easeOut);
            }

            vignette.color.value = Color.Lerp(goldColor, originalVignetteColor, easeOut);

            if (colorAdjustments != null)
                colorAdjustments.postExposure.value = Mathf.Lerp(originalPostExposure + 0.4f, originalPostExposure, easeOut);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (bloom != null)
        {
            bloom.intensity.value = originalBloomIntensity;
            bloom.tint.value = originalBloomTint;
        }
        vignette.color.value = originalVignetteColor;
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = originalPostExposure;
    }

    public void PlayDamageEffect()
    {
        StartCoroutine(DamageEffectRoutine());
    }

    private IEnumerator DamageEffectRoutine()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Color damageColor = new Color(1f, 0.2f, 0.2f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float spike = Mathf.Sin(t * Mathf.PI);

            chromaticAberration.intensity.value = Mathf.Lerp(originalChromaticAberrationIntensity, 0.5f, spike);
            vignette.intensity.value = originalVignetteIntensity + Mathf.Lerp(0f, 0.3f, spike);
            vignette.color.value = Color.Lerp(damageColor, originalVignetteColor, spike);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        chromaticAberration.intensity.value = originalChromaticAberrationIntensity;
        vignette.intensity.value = originalVignetteIntensity;
        vignette.color.value = originalVignetteColor;
    }

    public void PlaySpeedBoostEffect()
    {
        StartCoroutine(SpeedBoostEffectRoutine());
    }

    private IEnumerator SpeedBoostEffectRoutine()
    {
        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float fadeOut = 1f - Mathf.Pow(t, 2f);

            chromaticAberration.intensity.value = Mathf.Lerp(originalChromaticAberrationIntensity, 0.2f, fadeOut);
            lensDistortion.intensity.value = Mathf.Lerp(originalLensDistortionIntensity, -0.15f, fadeOut);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        chromaticAberration.intensity.value = originalChromaticAberrationIntensity;
        lensDistortion.intensity.value = originalLensDistortionIntensity;
    }

    #endregion
}
