using UnityEngine;
using data.structs;
public class GasingPartCollider : MonoBehaviour
{
    // Tentukan jenis part ini melalui Inspector Unity (Misal: HEAD, BODY, HAND, LEG)
    public PartType jenisPart;

    // Referensi ke script utama di Parent gasing kita
    private GasingStat scriptUtamaParent;

    void Start()
    {
        // Mencari script GasingBase yang ada di objek paling atas (Parent)
        scriptUtamaParent = GetComponentInParent<GasingStat>();
    }

    // Fungsi ini akan otomatis dipanggil oleh Unity saat part ini mendeteksi tabrakan
    void OnCollisionEnter(Collision collision)
    {
        // Pastikan yang ditabrak adalah gasing lain (bukan lantai arena)
        if (collision.gameObject.CompareTag("Musuh") || collision.gameObject.CompareTag("Player"))
        {
            // Ambil informasi kekuatan tabrakan
            float kekuatanBenturan = collision.relativeVelocity.magnitude;

            Debug.Log(gameObject.name + " (Part: " + jenisPart + ") terkena tabrakan dengan kekuatan: " + kekuatanBenturan);

            // Kirim data tabrakan ini ke script utama di Parent untuk dikalkulasi ke HP/Stat
            if (scriptUtamaParent != null)
            {
                // scriptUtamaParent.TerimaDamagePart(jenisPart, kekuatanBenturan, null);
            }

            // Cari kamera utama dan trigger guncangan
        }
    }
}
