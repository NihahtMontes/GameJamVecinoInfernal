using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  SEGUIMIENTO
    // ─────────────────────────────────────────────────────────────
    [Header("Seguimiento")]
    public Transform target;           // Arrastra el Player aquí en el Inspector
    public float smoothSpeed = 5f;

    // ─────────────────────────────────────────────────────────────
    //  ZOOM CON SCROLL DEL MOUSE
    // ─────────────────────────────────────────────────────────────
    [Header("Zoom (Scroll del Mouse)")]
    public float minZoom       = 3f;   // Zoom máximo (más cerca)
    public float maxZoom       = 12f;  // Zoom mínimo (más lejos → ver toda la casa)
    public float zoomSpeed     = 3f;
    public float zoomSmooth    = 6f;

    private Camera cam;
    private float  targetZoom;

    // =============================================================
    void Start()
    {
        cam        = GetComponent<Camera>();
        targetZoom = cam != null ? cam.orthographicSize : 5f;
    }

    // LateUpdate para que ocurra después de que el Player se haya movido
    void LateUpdate()
    {
        if (target == null) return;

        // ── Seguir al jugador (suavizado) ──
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // ── Zoom con scroll ──
        if (cam == null) return;

        float scroll   = Input.GetAxis("Mouse ScrollWheel"); // positivo = acercar
        targetZoom    -= scroll * zoomSpeed * 10f;           // scroll arriba = zoom out
        targetZoom     = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        // Lerp suave hacia el objetivo de zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSmooth * Time.deltaTime);
    }
}