using UnityEngine;
using System.Collections;

public class PowerItem : MonoBehaviour
{
    [Header("Duración del Modo Cazador")]
    public float duration = 10f; // 10 segundos según lo pedido

    void Start()
    {
        // Se destruye solo después de unos segundos si no lo recogen para no saturar la pantalla
        StartCoroutine(DestruirDespuesDeTiempo(25f)); 
    }

    void Update()
    {
        // Animación sencilla de rotación
        transform.Rotate(0, 0, 90f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActivateHunterMode(duration);
            }
            Destroy(gameObject);
        }
    }

    IEnumerator DestruirDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        Destroy(gameObject);
    }
}
