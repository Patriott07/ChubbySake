using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class QTECircle : MonoBehaviour, IPointerEnterHandler
{
    private float nilaiBonus = 10f;
    [SerializeField] private TMP_Text textNumber;
    private GasingActionAttack actionAttackScript;

    void Start()
    {
        // Cari script utama di objek Player
        // GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Pilih nilai acak bulat (menggunakan int agar teksnya tidak desimal panjang)
        nilaiBonus = Mathf.Round(Random.Range(10f, 70f));

        if (textNumber != null)
            textNumber.text = $"+{nilaiBonus}"; // Kasi variasi visual tanda +

        // if (QTESpawner.Instance != null)
        // {
        //     actionAttackScript = player.GetComponent<GasingActionAttack>();
        // }

        // Hancurkan diri sendiri jika diabaikan player agar UI bersih
        // FIX: Menggunakan DestroyTrigger bawaan kemarin agar garis penghubungnya ikut hancur alami!
        Destroy(gameObject, 2f);
    }

    // Pemicu saat kursor Mouse/Pointer HOVER masuk ke dalam area bundaran UI ini
    public void OnPointerEnter(PointerEventData eventData)
    {
        // FIX LOGIC: Hapus kondisi Input.GetMouseButton(0). Cukup arahkan kursor (Hover) 
        // maka data damage langsung terkirim instant dan akurat walaupun game lagi slow-mo berat!

        if (Input.GetMouseButton(0))
        {
            if (QTESpawner.Instance != null)
            {
                QTESpawner.Instance.gasingActionAttack.TambahBonusDamage(nilaiBonus);
                Debug.Log($"[QTE SUCCESS] Berhasil mengirim damage +{nilaiBonus} ke Player script.");
            }
        }

        // Hancurkan lingkaran karena berhasil diselesaikan
        Destroy(gameObject);
    }
}