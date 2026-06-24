using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
   public static Action<float, float> CallShake;

   // =========== FOR SYSTEM
   public static Action OnPotionSpawn;
   public static Action OnPotionCollect;
   // =========== FOR SYSTEM END

   // =========== FOR UI
   public static Action OnDeleteLine;
   // =========== FOR UI (END)

}
