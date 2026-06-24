using UnityEngine;

public enum PotionType { Health, SpeadMove, Kebal, Damage,  EnergyAttack, EnergyUlt, Box }

[CreateAssetMenu(fileName = "New Potion Data", menuName = "QTE Game/Potion Data")]
public class PotionData : ScriptableObject
{
    public string potionName;
    public PotionType type;
    public float effectValue; // Jumlah heal atau penambahannya
    public Sprite potionSprite; // Visual botolnya
}