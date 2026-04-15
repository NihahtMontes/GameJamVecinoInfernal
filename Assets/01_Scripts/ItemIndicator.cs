using UnityEngine;

/// <summary>
/// ItemIndicator — Adjunta este script al Player.
/// Controla dos flechas que orbitan al jugador y apuntan hacia los ítems activos.
/// </summary>
public class ItemIndicator : MonoBehaviour
{
    [Header("Flechas (hijos del Player)")]
    public Transform flashlightArrow; // Sprite flecha → linterna (blanca/azul)
    public Transform powerArrow;      // Sprite flecha → poder (amarilla)

    [Header("Configuración")]
    [Tooltip("Distancia del centro del jugador a la que orbitan las flechas")]
    public float orbitRadius = 0.8f;

    void Update()
    {
        if (GameManager.Instance == null) return;

        // ── Flecha de recarga de linterna ──
        UpdateArrow(flashlightArrow, GameManager.Instance.currentItem);

        // ── Flecha del ítem de poder ──
        UpdateArrow(powerArrow, GameManager.Instance.currentPowerItem);
    }

    void UpdateArrow(Transform arrow, GameObject target)
    {
        if (arrow == null) return;

        // Si el ítem no existe → ocultar la flecha
        if (target == null)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        // Ítem existe → mostrar y apuntar
        arrow.gameObject.SetActive(true);

        // Dirección del jugador al ítem
        Vector2 dir = (target.transform.position - transform.position).normalized;

        // Rotar la flecha para que apunte en esa dirección
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrow.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // -90 porque el sprite apunta "arriba"

        // Posicionar la flecha orbitando alrededor del jugador
        arrow.position = transform.position + (Vector3)(dir * orbitRadius);
    }
}
