using UnityEngine;
using data.structs;

public static class ProgressManager
{
    private const string SAVE_KEY = "BlastBlade_Progress";

    public static void Save(ProgressInGame data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[ProgressManager] Saved: gold={data.gold}, exp={data.exp}, level={data.level}");
    }

    public static ProgressInGame Load()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("[ProgressManager] No save found, creating new progress.");
            return new ProgressInGame();
        }
        var data = JsonUtility.FromJson<ProgressInGame>(json);
        Debug.Log($"[ProgressManager] Loaded: gold={data.gold}, exp={data.exp}, level={data.level}");
        return data;
    }

    public static void Delete()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[ProgressManager] Progress deleted.");
    }

    private const float EXP_PER_LEVEL = 10f;

    public static int CalculateLevel(float totalExp)
    {
        return Mathf.FloorToInt(totalExp / EXP_PER_LEVEL) + 1;
    }

    public static float ExpInCurrentLevel(float totalExp)
    {
        int level = CalculateLevel(totalExp);
        return totalExp - (level - 1) * EXP_PER_LEVEL;
    }

    public static float ExpForNextLevel(int currentLevel)
    {
        return EXP_PER_LEVEL;
    }

    public static void ApplyMatchRewards(ProgressInGame progress, float goldEarned, float expEarned)
    {
        progress.gold += goldEarned;
        progress.exp += expEarned;
        progress.level = CalculateLevel(progress.exp);
        Save(progress);
    }
}
