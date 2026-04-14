using UnityEngine;
using System;

/// <summary>
/// FlashlightItem — Item de recarga de linterna.
///
/// SETUP EN UNITY:
/// 1. Crea un GameObject con este script, un SpriteRenderer y un CircleCollider2D en modo Trigger.
/// 2. Usa cualquier sprite (ej: un círculo amarillo o una batería).
/// 3. Asegúrate de que el tag del Player sea "Player".
/// 4. Convierte este GameObject en un Prefab y asígnalo al GameManager.
/// </summary>
public class FlashlightItem : MonoBehaviour
{
    [Header("Cantidad de cargas que restaura")]
    public int chargesRestored = 5;

    // Evento para notificar al GameManager cuando se recogió
    public event Action OnPickedUp;

    private SpriteRenderer sr;
    private float          pulseTimer = 0f;

    // =============================================================
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // =============================================================
    void Update()
    {
        // ── Efecto visual: parpadeo + escala pulsante ──
        pulseTimer += Time.deltaTime * 3f;

        if (sr != null)
        {
            // Alpha oscilante entre 0.5 y 1.0
            float alpha = Mathf.Sin(pulseTimer) * 0.25f + 0.75f;
            Color c     = sr.color;
            c.a         = alpha;
            sr.color    = c;

            // Escala pulsante sutil
            float scale         = 1f + Mathf.Sin(pulseTimer * 2f) * 0.12f;
            transform.localScale = Vector3.one * scale;
        }
    }

    // =============================================================
    //  RECOGIDA POR EL JUGADOR
    // =============================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
            player.AddFlashlightCharges(chargesRestored);

        OnPickedUp?.Invoke();
        Destroy(gameObject);
    }
}
