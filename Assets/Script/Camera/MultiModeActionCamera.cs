using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class MultiModeActionCamera : MonoBehaviour
{
    public enum CameraMode { IsometricAction, HardLockPlayer, TopDown, CinematicOrbit, CloseUpAction }

    [Header("Switch Kontrol")]
    public CameraMode modeSaatIni = CameraMode.IsometricAction;
    [Tooltip("Tekan tombol ini di keyboard untuk ganti mode kamera")]
    public KeyCode switchKey = KeyCode.C;

    [Header("Targets (Player & Musuh)")]
    public Transform playerTarget;
    public List<Transform> musuhTargets = new List<Transform>();

    [Header("1. Settings Isometric Action")]
    public Vector3 isoOffset = new Vector3(0f, 12f, -8f);
    public float minZoom = 35f;
    public float maxZoom = 60f;
    public float zoomLimitBatas = 15f;

    [Header("2. Settings Hard Lock Player")]
    public Vector3 lockOffset = new Vector3(0f, 10f, -6f);

    [Header("3. Settings Top Down")]
    public float tinggiTopDown = 18f;

    [Header("4. Settings Cinematic Orbit")]
    public Transform pusatArena; // Taruh objek kosong di tengah arena
    public float radiusOrbit = 15f;
    public float tinggiOrbit = 8f;
    public float kecepatanOrbit = 15f;

    [Header("5. Settings Close Up Action")]
    public Vector3 closeUpOffset = new Vector3(0f, 3f, -4f);

    [Header("General Tuning")]
    public float smoothSpeed = 5f;

    private Camera cam;
    private float currentOrbitAngle = 0f;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Cari otomatis musuh dengan Tag "Musuh"
        GameObject[] musuhObjs = GameObject.FindGameObjectsWithTag("Musuh");
        foreach (GameObject go in musuhObjs)
        {
            musuhTargets.Add(go.transform);
        }

        // Jika pusat arena belum diset, otomatis gunakan koordinat (0,0,0)
        if (pusatArena == null)
        {
            GameObject go = new GameObject("PusatArenaDefault");
            go.transform.position = Vector3.zero;
            pusatArena = go.transform;
        }
    }

    void Update()
    {
        // Deteksi input tombol 'C' untuk ganti ke mode berikutnya
        if (Input.GetKeyDown(switchKey))
        {
            NextCameraMode();
        }
    }

    void LateUpdate()
    {
        if (playerTarget == null) return;

        // Reset FOV ke standar 45 jika tidak sedang di mode 1 (Isometric Action)
        if (modeSaatIni != CameraMode.IsometricAction)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 45f, smoothSpeed * Time.deltaTime);
        }

        // Eksekusi behaviour berdasarkan mode yang aktif
        switch (modeSaatIni)
        {
            case CameraMode.IsometricAction:
                JalankanIsometricAction();
                break;
            case CameraMode.HardLockPlayer:
                JalankanHardLockPlayer();
                break;
            case CameraMode.TopDown:
                JalankanTopDown();
                break;
            case CameraMode.CinematicOrbit:
                JalankanCinematicOrbit();
                break;
            case CameraMode.CloseUpAction:
                JalankanCloseUpAction();
                break;
        }
    }

    public void ChangeCameraDirectly(CameraMode cameraMode)
    {
        switch (cameraMode)
        {
            case CameraMode.IsometricAction:
                JalankanIsometricAction();
                break;
            case CameraMode.HardLockPlayer:
                JalankanHardLockPlayer();
                break;
            case CameraMode.TopDown:
                JalankanTopDown();
                break;
            case CameraMode.CinematicOrbit:
                JalankanCinematicOrbit();
                break;
            case CameraMode.CloseUpAction:
                JalankanCloseUpAction();
                break;
        }
    }

    // Fungsi siklus ganti mode kamera (Mode 1 -> 2 -> 3 -> 4 -> 5 -> Balik ke 1)
    void NextCameraMode()
    {
        int totalMode = System.Enum.GetValues(typeof(CameraMode)).Length;
        int nextIndex = ((int)modeSaatIni + 1) % totalMode;
        modeSaatIni = (CameraMode)nextIndex;

        Debug.Log("Kamera berganti ke mode: " + modeSaatIni.ToString());
    }

    #region BEHAVIOUR LOGIC

    // MODE 1: Kamera dinamis mengikuti titik tengah pertempuran + Auto Zoom
    void JalankanIsometricAction()
    {
        List<Transform> targetAktif = DapatkanTargetAktif();
        Vector3 centerPoint = HitungCenterPoint(targetAktif);

        Vector3 targetPos = centerPoint + isoOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(centerPoint);

        if (targetAktif.Count > 1)
        {
            float jarakTerjauh = HitungJarakTerjauh(targetAktif, centerPoint);
            float targetZoom = Mathf.Lerp(minZoom, maxZoom, jarakTerjauh / zoomLimitBatas);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetZoom, smoothSpeed * Time.deltaTime);
        }
    }

    // MODE 2: Mengunci kaku hanya pada player gasing
    void JalankanHardLockPlayer()
    {
        Vector3 targetPos = playerTarget.position + lockOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(playerTarget.position + Vector3.up * 0.5f);
    }

    // MODE 3: Kamera tegak lurus dari langit melihat lurus ke bawah
    void JalankanTopDown()
    {
        Vector3 targetPos = new Vector3(playerTarget.position.x, tinggiTopDown, playerTarget.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(90f, 0f, 0f), smoothSpeed * Time.deltaTime);
    }

    // MODE 4: Kamera berputar mengelilingi pusat arena secara sinematik
    void JalankanCinematicOrbit()
    {
        currentOrbitAngle += kecepatanOrbit * Time.deltaTime;
        if (currentOrbitAngle >= 360f) currentOrbitAngle -= 360f;

        Quaternion rotation = Quaternion.Euler(0f, currentOrbitAngle, 0f);
        Vector3 posisiOrbit = pusatArena.position - (rotation * Vector3.forward * radiusOrbit);
        posisiOrbit.y = tinggiOrbit;

        transform.position = Vector3.Lerp(transform.position, posisiOrbit, smoothSpeed * Time.deltaTime);
        transform.LookAt(pusatArena.position);
    }

    // MODE 5: Kamera sangat dekat di belakang arah hadap gasing player (Vibe Intense)
    void JalankanCloseUpAction()
    {
        // Mengikuti posisi dan arah rotasi gasing belakang player secara dramatis
        // Vector3 targetPos = playerTarget.position + (playerTarget.rotation * closeUpOffset);
        // transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        // transform.LookAt(playerTarget.position + Vector3.up * 1f);

        // KUNCI UTAMA: Kita pakai Vector3 offset murni tanpa mengalikan playerTarget.rotation.
        // Dengan begini, meskipun gasing berputar 360 derajat, posisi kamera tetap tenang di belakang.
        Vector3 targetPos = playerTarget.position + closeUpOffset;

        // Lakukan pergerakan interpolasi linear yang halus menuju posisi target
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Kamera tetap fokus membidik ke arah tubuh gasing player (dinaikkan sedikit sumbu Y-nya agar enak dilihat)
        Vector3 lookAtTarget = playerTarget.position + Vector3.up * 0.5f;

        // Rotasi kamera mengikuti sudut bidik secara halus agar tidak patah-patah saat gasing terpental
        Quaternion targetRotation = Quaternion.LookRotation(lookAtTarget - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }

    #endregion

    #region HELPER FUNCTIONS (Kalkulasi Titik Tengah)
    List<Transform> DapatkanTargetAktif()
    {
        List<Transform> list = new List<Transform> { playerTarget };
        foreach (Transform t in musuhTargets)
        {
            if (t != null && t.gameObject.activeInHierarchy) list.Add(t);
        }
        return list;
    }

    Vector3 HitungCenterPoint(List<Transform> targets)
    {
        if (targets.Count == 1) return targets[0].position;
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 1; i < targets.Count; i++) bounds.Encapsulate(targets[i].position);
        return bounds.center;
    }

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

    public void CameraShake(float duration, float strength)
    {
        StartCoroutine(Shake(duration, strength));
    }

    private IEnumerator Shake(float duration, float strength)
    {
        Vector3 originalPosition = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
        transform.localPosition = originalPosition;
    }
    #endregion

    void OnEnable()
    {
        GameEvents.CallShake += CameraShake;
    }
    void OnDisable()
    {
        GameEvents.CallShake -= CameraShake;
    }

}