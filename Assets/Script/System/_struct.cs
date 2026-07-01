using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Numerics;
namespace data.structs
{
    public enum PartType { Head, Body, Hand, Leg }

    [System.Serializable]
    public class PlayerData
    {
        public string nameplayer;
        public int age;
        public int totalGold;
        public int currentLevel;
        public CustomizationCollection customizationCollection;
    }



    [System.Serializable]
    public class CustomizationCollection
    {
        // List berisi ID gasing yang sudah dibeli/dimiliki player
        public List<string> unlockedGasingIDs = new List<string>();
        public List<string> unlockedCustomizationIDs = new List<string>();
        // List berisi ID aksesoris/customization (warna, skin, dll) yang sudah dimiliki
    }

    

    [System.Serializable]
    public class Custome
    {
        public int ID;
        public string name;
        public string description;
        public Attributes attributeEffect;
        public Transform transformOffset; // from pivot parent

    }
    [System.Serializable]
    public class GameState
    {
        public PlayerData playerData;
    }

    [System.Serializable]
    public class Attributes
    {
        public float damage;
        public float maxHp;
        public float currentHp;
        public int maxNyawa;
        public int currentNyawa;
        public float currentRPM;
        public float maxRPM;
        public float minRPM;
        public float rpmRegenSpeed;
        public float currentEnergyAttack;
        public float maxEnergyAttack;
        public float currentEnergyUltimate;
        public float maxEnergyUltimate;

        // movement script
        public float knockbackForce = 10f;
        public float hitCooldown = 0.2f;

        public float autoSpeed = 5f;
        public float changeDirectionInterval = 2f;
    }


}


