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
    public int currentPotion = 0;
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
        // FIX 1: Gunakan tanda < (kurang dari), jangan <= (kurang dari sama dengan)
        // Jadi kalau currentPotion sudah bernilai 2, dia langsung memblokir spawn baru
        if (Time.time >= nextSpawnTime && currentPotion < maxPotion)
        {
            SpawnRandomPotion();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnRandomPotion()
    {
        if (prefabBasePotions == null || prefabBasePotions.Count == 0 || listDaftarPotion == null || listDaftarPotion.Count == 0) return;
        
        // FIX 2: Proteksi ganda, jika jumlah sudah mencapai atau melewati batas, batalkan proses instantiate
        if (currentPotion >= maxPotion) return;

        // 1. Hitung titik koordinat acak melingkar di arena 3D
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadiusLimit;
        Vector3 spawnPosition = new Vector3(randomCircle.x, transform.position.y, randomCircle.y); 

        // 2. Lahirkan objek ramuannya secara acak sesuai list index prefab kamu
        int randomIndex = Random.Range(0, prefabBasePotions.Count);
        GameObject newPotion = Instantiate(prefabBasePotions[randomIndex], spawnPosition, Quaternion.identity);
        
        // Picu event untuk menambah hitungan currentPotion
        GameEvents.OnPotionSpawn?.Invoke();
    }
    
    // Untuk membantu kamu melihat batas lingkaran spawn di editor Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadiusLimit);
    }
}