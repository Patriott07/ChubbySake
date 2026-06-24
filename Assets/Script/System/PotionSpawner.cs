using System.Collections.Generic;
using UnityEngine;

public class PotionSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<GameObject> prefabBasePotions; // Prefab kosong yang ditempeli skrip PotionItem
    [SerializeField] private List<PotionData> listDaftarPotion; // Taruh aset ScriptableObject ramuanmu di sini
    [SerializeField] private float spawnInterval = 7f; // Jatuh setiap 7 detik sekali

    [Header("Arena Bounds")]
    [SerializeField] private float spawnRadiusLimit = 15f; // Batas radius cekungan wajan arena gasing kamu

    private float nextSpawnTime = 0f;
    private int maxPotion = 2;
    private int currentPotion = 0;
    void Start()
    {
        // Atur spawn pertama kali relatif dari waktu mulai game
        nextSpawnTime = Time.time + spawnInterval;
    }

    void OnEnable()
    {
        GameEvents.OnPotionSpawn += EventPotionSpawn;
        GameEvents.OnPotionCollect += EventPotionCollect;
    }

    void OnDisable()
    {
        GameEvents.OnPotionSpawn -= EventPotionSpawn;
        GameEvents.OnPotionCollect -= EventPotionCollect;
    }

    void EventPotionSpawn()
    {
        currentPotion += 1;
        currentPotion = Mathf.Clamp(currentPotion, 0, maxPotion);
    }

    void EventPotionCollect()
    {
        currentPotion -= 1;
        currentPotion = Mathf.Clamp(currentPotion, 0, maxPotion);
    }

    void Update()
    {
        // Jatuh otomatis seiring berjalannya waktu game normal
        if (Time.time >= nextSpawnTime && currentPotion <= maxPotion)
        {
            SpawnRandomPotion();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }



    void SpawnRandomPotion()
    {
        if (prefabBasePotions[0] == null || listDaftarPotion == null || listDaftarPotion.Count == 0) return;

        // 1. Hitung titik koordinat acak melingkar di arena 3D (Sumbu X dan Z, Y konstan di atas permukaan)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadiusLimit;
        Vector3 spawnPosition = new Vector3(randomCircle.x, transform.position.y, randomCircle.y); // Angka 2f adalah tinggi jatuh bebas

        // 2. Lahirkan objek ramuannya
        int randomIndex = Random.Range(0, listDaftarPotion.Count);
        GameObject newPotion = Instantiate(prefabBasePotions[randomIndex], spawnPosition, Quaternion.identity);
        GameEvents.OnPotionSpawn?.Invoke();

        // if (potionScript != null)
        // {
        //     // 3. Ambil jenis ramuan secara acak dari daftar koleksi ScriptableObject kamu
        //     int randomIndex = Random.Range(0, listDaftarPotion.Count);
        //     PotionData selectedData = listDaftarPotion[randomIndex];

        //     // Suntikkan data acak tersebut ke objek yang baru lahir
        //     // potionScript.SetupPotion(selectedData);
        // }
    }

    // Untuk membantu kamu melihat batas lingkaran spawn di editor Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadiusLimit);
    }
}