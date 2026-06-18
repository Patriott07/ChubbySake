using UnityEngine;
using System.Collections.Generic;

public class ActionCamera : MonoBehaviour
{
    [Header("Targets (Player & Musuh)")]
    public Transform playerTarget;
    // Daftar semua musuh aktif di arena
    public List<Transform> musuhTargets = new List<Transform>();

    [Header("Position Tuning")]
    public Vector3 offset = new Vector3(0f, 12f, -8f);
    public float smoothTime = 0.3f;
    private Vector3 velocity;

    [Header("Auto Zoom Tuning (Berdasarkan Jarak)")]
    public float minZoom = 30f;      // Field of View saat dekat (Zoom In)
    public float maxZoom = 60f;      // Field of View saat jauh (Zoom Out)
    public float zoomLimitBatas = 15f; // Jarak maksimum antar gasing sebelum zoom mentok
    public float zoomSmoothTime = 0.2f;
    private float zoomVelocity;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Cari otomatis semua GameObject dengan Tag "Musuh" saat game mulai
        GameObject[] musuhObjs = GameObject.FindGameObjectsWithTag("Musuh");
        foreach (GameObject go in musuhObjs)
        {
            musuhTargets.Add(go.transform);
        }
    }

    void LateUpdate()
    {
        if (playerTarget == null) return;

        // 1. Ambil daftar target yang masih aktif/hidup di arena
        List<Transform> targetAktif = DapatkanTargetAktif();

        // 2. Hitung titik tengah (Center Point) di antara semua gasing
        Vector3 centerPoint = HitungCenterPoint(targetAktif);

        // 3. Gerakkan kamera secara halus menuju titik tengah + offset
        Vector3 targetPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // 4. Kamera selalu menatap tajam ke titik tengah pertarungan
        transform.LookAt(centerPoint);

        // 5. Auto Zoom dinamis berdasarkan jarak terjauh antar objek
        if (targetAktif.Count > 1)
        {
            float jarakTerjauh = HitungJarakTerjauh(targetAktif, centerPoint);
            float targetZoom = Mathf.Lerp(minZoom, maxZoom, jarakTerjauh / zoomLimitBatas);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetZoom, ref zoomVelocity, zoomSmoothTime);
        }
    }

    // Fungsi untuk menyaring musuh yang mungkin sudah mati/hancur (Inactive)
    List<Transform> DapatkanTargetAktif()
    {
        List<Transform> list = new List<Transform>();
        list.Add(playerTarget); // Player selalu masuk hitungan

        for (int i = 0; i < musuhTargets.Count; i++)
        {
            if (musuhTargets[i] != null && musuhTargets[i].gameObject.activeInHierarchy)
            {
                list.Add(musuhTargets[i]);
            }
        }
        return list;
    }

    // Menghitung koordinat rata-rata di antara semua objek aktif
    Vector3 HitungCenterPoint(List<Transform> targets)
    {
        if (targets.Count == 1) return targets[0].position;

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 1; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.center;
    }

    // Mencari seberapa jauh gasing yang paling ujung dari titik tengah
    float HitungJarakTerjauh(List<Transform> targets, Vector3 centerPoint)
    {
        float jarakMaks = 0f;
        foreach (Transform t in targets)
        {
            float jarak = Vector3.Distance(t.position, centerPoint);
            if (jarak > jarakMaks) jarakMaks = jarak;
        }
        return jarakMaks;
    }
}