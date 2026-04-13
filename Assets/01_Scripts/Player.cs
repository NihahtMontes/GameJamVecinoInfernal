using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private bool isTransitioning = false;

    [Header("UI & Transiciones")]
    // Para el fade — asigna en el Inspector un Image negro que cubra toda la pantalla
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Habilidades (Extraído de DaniloRomero)")]
    public float flashDistance = 2.0f;
    public KeyCode attackKey = KeyCode.Space;
    
    // Esta variable la leerá el enemigo
    [HideInInspector] public bool isMakingNoise = false;
    private EnemyIA enemyScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Busca al enemigo automáticamente al iniciar (si existe)
        enemyScript = Object.FindFirstObjectByType<EnemyIA>();

        // Empieza el fade de entrada (negro → transparente)
        if (fadeImage != null)
            StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (isTransitioning) return; // bloquea movimiento durante transición

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Comunicar ruido al enemigo
        isMakingNoise = (movement.magnitude > 0.1f);

        // Control de Animaciones
        if (movement.magnitude > 0)
        {
            animator.SetBool("IsMoving", true);
            if (movement.y != 0)
            {
                animator.SetFloat("MoveX", 0);
                animator.SetFloat("MoveY", movement.y);
            }
            else
            {
                animator.SetFloat("MoveX", movement.x);
                animator.SetFloat("MoveY", 0);
            }
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        // Ataque con linterna
        if (Input.GetKeyDown(attackKey))
        {
            AtacarConLinterna();
        }
    }

    void FixedUpdate()
    {
        if (isTransitioning) return;
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }

    void AtacarConLinterna()
    {
        // Si no se encontró al inicio, intentamos buscarlo de nuevo
        if (enemyScript == null) enemyScript = Object.FindFirstObjectByType<EnemyIA>();

        if (enemyScript != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemyScript.transform.position);
            if (distanceToEnemy <= flashDistance)
            {
                enemyScript.RecibirLuzLinterna();
                Debug.Log("¡Fantasma aturdido!");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Casa") && !isTransitioning)
        {
            StartCoroutine(TransicionAInterior());
        }
    }

    IEnumerator TransicionAInterior()
    {
        isTransitioning = true;
        animator.SetBool("IsMoving", false);

        // Fade transparente → negro
        yield return StartCoroutine(FadeOut());

        // Cargar escena Interior
        SceneManager.LoadScene("Interior");
    }

    IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        float timer = 0f;
        Color color = fadeImage.color;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        float timer = 0f;
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    // Dibujar el círculo de la linterna en el editor para visualización
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, flashDistance);
    }
}