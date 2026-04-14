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

    // ── NUEVO: el Dialogue lo controla ──────────────────────
    [HideInInspector] public bool canMove = true;
    // ────────────────────────────────────────────────────────

    [Header("UI & Transiciones")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Habilidades (Extraído de DaniloRomero)")]
    public float flashDistance = 2.0f;
    public KeyCode attackKey = KeyCode.Space;

    [HideInInspector] public bool isMakingNoise = false;
    private EnemyIA enemyScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyScript = Object.FindFirstObjectByType<EnemyIA>();

        if (fadeImage != null)
            StartCoroutine(FadeIn());
    }

    void Update()
    {
        // Bloquea si está en transición O si el diálogo lo pidió
        if (isTransitioning || !canMove)
        {
            // Asegura que el personaje quede quieto visualmente
            movement = Vector2.zero;
            animator.SetBool("IsMoving", false);
            isMakingNoise = false;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        isMakingNoise = (movement.magnitude > 0.1f);

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

        // ── CAMBIO: el atacar con linterna ya no usa Space ──
        // Space ahora lo usa el diálogo para avanzar líneas.
        // Si querés mantener Space para la linterna cuando NO
        // hay diálogo activo, dejalo así. Si preferís otra tecla
        // cambiá attackKey en el Inspector.
        if (Input.GetKeyDown(attackKey))
        {
            AtacarConLinterna();
        }
    }

    void FixedUpdate()
    {
        if (isTransitioning || !canMove) return;
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }

    void AtacarConLinterna()
    {
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
        yield return StartCoroutine(FadeOut());
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, flashDistance);
    }
}