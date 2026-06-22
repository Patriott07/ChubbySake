using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class QTESpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefabBundaranQTE; 
    [SerializeField] private float spawnRate = 0.3f; 
    private float nextSpawnTime = 0f;
    private RectTransform rectTransform;
    
    public GameObject prefabGarisUI; 
    public CanvasGroup canvasGroupBgBlack;
    private GameObject lastSpawnedCircle = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        GameEvents.OnDeleteLine?.Invoke(); // Menggunakan ?. agar aman jika event kosong
        
        // FIX 1: Set waktu spawn pertama relatif terhadap waktu unscaled saat ini
        nextSpawnTime = Time.unscaledTime + spawnRate;
        
        // Reset referensi lingkaran lama agar tidak membawa data dari sesi sebelumnya
        lastSpawnedCircle = null; 

        if (canvasGroupBgBlack != null)
        {
            canvasGroupBgBlack.DOFade(0.7f, 0.4f).SetUpdate(true);
        }
    }

    void Update()
    {
        if (Time.unscaledTime >= nextSpawnTime)
        {
            SpawnCircleAtRandomPosition();
            nextSpawnTime = Time.unscaledTime + spawnRate;
        }
    }

    void SpawnCircleAtRandomPosition()
    {
        if (prefabBundaranQTE == null) return;

        GameObject newCircle = Instantiate(prefabBundaranQTE, transform);
        RectTransform circleRect = newCircle.GetComponent<RectTransform>();

        if (circleRect != null)
        {
            float circleRadiusX = circleRect.rect.width / 2f;
            float circleRadiusY = circleRect.rect.height / 2f;

            float limitX = (rectTransform.rect.width / 3.5f) - circleRadiusX;
            float limitY = (rectTransform.rect.height / 3.5f) - circleRadiusY;

            float randomX = Random.Range(-limitX, limitX);
            float randomY = Random.Range(-limitY, limitY);

            circleRect.anchoredPosition = new Vector2(randomX, randomY);
        }

        // FIX 2: Cek validitas objek menggunakan perbandingan khusus Unity (Equals/implicit bool check)
        // Jika objek lama sudah dihancurkan karena dihover, jangan gambar garis dari posisi hantu.
        if (lastSpawnedCircle != null && lastSpawnedCircle.gameObject != null && prefabGarisUI != null)
        {
            CreateLineBetweenQTE(lastSpawnedCircle, newCircle);
        }

        // Simpan referensi QTE baru
        lastSpawnedCircle = newCircle;
    }

    void CreateLineBetweenQTE(GameObject fromCircle, GameObject toCircle)
{
    if (fromCircle == null || toCircle == null) return;

    RectTransform rectFrom = fromCircle.GetComponent<RectTransform>();
    RectTransform rectTo = toCircle.GetComponent<RectTransform>();

    if (rectFrom == null || rectTo == null) return;

    // FIX: Masukkan lineObj ke 'transform' (yaitu QTEUI / Parent dari lingkaran)
    GameObject lineObj = Instantiate(prefabGarisUI, transform);
    
    // Paksa garis berada di paling atas hierarki QTEUI (di belakang semua CircleQte)
    lineObj.transform.SetAsFirstSibling(); 

    RectTransform rectLine = lineObj.GetComponent<RectTransform>();

    // Karena sekarang garis berada di parent yang sama dengan lingkaran, 
    // kita gunakan posisi absolut anchoredPosition masing-masing
    Vector2 posA = rectFrom.anchoredPosition;
    Vector2 posB = rectTo.anchoredPosition;

    Vector2 direction = posB - posA; // Arah dari A ke B
    float distance = direction.magnitude;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    rectLine.anchoredPosition = posA + (direction / 2f);
    rectLine.sizeDelta = new Vector2(distance, 10f);
    rectLine.localRotation = Quaternion.Euler(0, 0, angle);

    // KUNCI PENTING: Agar garis tetap ikut hancur saat lingkaran dituju dihancurkan,
    // kita buat skrip kecil untuk menghancurkan garis ini saat toCircle mati, 
    // atau gunakan fungsi bawaan OnDestroy:
    var destroyTrigger = toCircle.AddComponent<DestroyTrigger>();
    destroyTrigger.objectToDestroy = lineObj;
}
}