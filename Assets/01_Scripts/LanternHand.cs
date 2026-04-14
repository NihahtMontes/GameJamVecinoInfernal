using UnityEngine;

/// <summary>
/// LanternHand — Sprite en la mano del jugador que apunta hacia el cursor.
///
/// SETUP EN UNITY:
/// 1. Crea un GameObject vacío HIJO del Player.
/// 2. Llámalo "LanternHand".
/// 3. Agrega un SpriteRenderer con el sprite de la linterna/mano.
/// 4. Agrega este script como componente.
/// 5. Arrastra este GameObject al campo "Lantern Hand" del Player en el Inspector.
/// 6. Asegúrate de que el SpriteRenderer esté desactivado al inicio
///    (lo activará el script automáticamente cuando se use).
/// </summary>
public class LanternHand : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Distancia del sprite de mano respecto al centro del Player")]
    public float handOffset = 0.45f;

    [Tooltip("Offset de rotación para corregir la orientación del sprite (grados)")]
    public float rotationOffset = -90f;

    // Estado
    private SpriteRenderer sr;
    private float          hideTimer  = 0f;
    private bool           isVisible  = false;
    private Vector2        flashDir   = Vector2.right;

    // =============================================================
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        HideLantern(); // Oculta al inicio
    }

    // =============================================================
    void Update()
    {
        if (!isVisible) return;

        // ── Actualizar dirección hacia el cursor ──
        if (Camera.main != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            flashDir = ((Vector2)mouseWorldPos - (Vector2)transform.parent.position).normalized;
        }

        // ── Rotar hacia el cursor ──
        float angle           = Mathf.Atan2(flashDir.y, flashDir.x) * Mathf.Rad2Deg;
        transform.rotation    = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        // ── Posicionar en la dirección del cursor (al lado del Player) ──
        transform.localPosition = (Vector3)(flashDir * handOffset);

        // ── Auto-ocultar después del timer ──
        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
            HideLantern();
    }

    // =============================================================
    //  MOSTRAR LINTERNA
    // =============================================================
    public void ShowLantern(float duration, Vector2 direction)
    {
        isVisible   = true;
        hideTimer   = duration;
        flashDir    = direction;

        if (sr != null) sr.enabled = true;
    }

    // =============================================================
    //  OCULTAR LINTERNA
    // =============================================================
    public void HideLantern()
    {
        isVisible = false;
        if (sr != null) sr.enabled = false;
    }
}
