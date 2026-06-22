using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public class QTECircle : MonoBehaviour, IPointerEnterHandler
{
    private float nilaiBonus = 10f; // Bonus damage dari lingkaran ini
    [SerializeField] TMP_Text textNumber;
    private GasingActionAttack actionAttackScript;

    void Start()
    {
        // Cari script utama di objek Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        nilaiBonus = Random.Range(10, 70);
        textNumber.text = nilaiBonus.ToString();
        if (player != null)
        {
            actionAttackScript = player.GetComponent<GasingActionAttack>();
        }

        // Hancurkan diri sendiri dalam beberapa detik jika tidak disentuh agar UI tidak penuh
        Destroy(gameObject, 2.5f); 
    }

    // 5. Efek ketika Mouse masuk/Hover ke dalam area bundaran UI ini
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cek apakah player juga sedang menahan tombol kiri mouse (Hold)
        if (Input.GetMouseButton(0))
        {
            if (actionAttackScript != null)
            {
                actionAttackScript.TambahBonusDamage(nilaiBonus);
            }

            // Hancurkan lingkaran karena berhasil diselesaikan
            Destroy(gameObject);
        }
    }
}