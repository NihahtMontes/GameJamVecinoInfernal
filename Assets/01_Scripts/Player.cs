using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  MOVIMIENTO
    // ─────────────────────────────────────────────────────────────
    [Header("Configuración de Movimiento")]
    public float speed = 3f;
    private Rigidbody2D rb;
    private Animator    animator;
    private Vector2     movement;
    private bool        isTransitioning = false;

    // ─────────────────────────────────────────────────────────────
    //  FADE / TRANSICIÓN
    // ─────────────────────────────────────────────────────────────
    [Header("UI & Transiciones")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    // ─────────────────────────────────────────────────────────────
    //  LINTERNA
    // ─────────────────────────────────────────────────────────────
    [Header("Linterna")]
    public int   maxFlashlightCharges = 10;      // Contador máximo
    public float flashDistance        = 3.5f;    // Alcance del rayo
    public float flashlightDuration   = 1f;      // Duración del flash (segundos)
    [Tooltip("Cono de luz: ángulo en grados desde la dirección del cursor")]
    public float flashConeAngle       = 45f;

    [Header("LanternHand — objeto hijo del Player")]
    public LanternHand lanternHand;              // Arrastra aquí el child object

    // Estado interno de linterna
    [HideInInspector] public int  currentCharges;
    private bool  flashlightActive = false;
    private float flashlightTimer  = 0f;

    // ─────────────────────────────────────────────────────────────
    //  RUIDO (leído por EnemyIA)
    // ─────────────────────────────────────────────────────────────
    [HideInInspector] public bool isMakingNoise = false;

    // ─────────────────────────────────────────────────────────────
    //  REFERENCIAS
    // ─────────────────────────────────────────────────────────────
    private EnemyIA enemyScript;

    // =============================================================
    void Start()
    {
        rb           = GetComponent<Rigidbody2D>();
        animator     = GetComponent<Animator>();
        enemyScript  = Object.FindFirstObjectByType<EnemyIA>();
        currentCharges = maxFlashlightCharges;

        if (fadeImage != null) StartCoroutine(FadeIn());

        // Actualizar UI inicial
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateFlashlightUI(currentCharges, maxFlashlightCharges);
    }

    // =============================================================
    void Update()
    {
        if (isTransitioning) return;

        // ── Movimiento ──
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        isMakingNoise = movement.magnitude > 0.1f; // Ruido al moverse

        // ── Animaciones ──
        if (movement.magnitude > 0)
        {
            if (animator) animator.SetBool("IsMoving", true);
            if (movement.y != 0)
            {
                if (animator) animator.SetFloat("MoveX", 0);
                if (animator) animator.SetFloat("MoveY", movement.y);
            }
            else
            {
                if (animator) animator.SetFloat("MoveX", movement.x);
                if (animator) animator.SetFloat("MoveY", 0);
            }
        }
        else
        {
            if (animator) animator.SetBool("IsMoving", false);
        }

        // ── Linterna: Click Izquierdo del Mouse ──
        if (Input.GetMouseButtonDown(0) && currentCharges > 0 && !flashlightActive)
            ActivarLinterna();

        // ── Timer de linterna activa (1 segundo) ──
        if (flashlightActive)
        {
            flashlightTimer -= Time.deltaTime;
            if (flashlightTimer <= 0f)
                DesactivarLinterna();
        }
    }

    // =============================================================
    void FixedUpdate()
    {
        if (isTransitioning) return;
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }

    // =============================================================
    //  LINTERNA
    // =============================================================
    void ActivarLinterna()
    {
        flashlightActive = true;
        flashlightTimer  = flashlightDuration;
        currentCharges--;

        // Actualizar UI
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateFlashlightUI(currentCharges, maxFlashlightCharges);

        // Calcular dirección del cursor en el mundo
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 flashDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        // Activar sprite de mano con dirección
        if (lanternHand != null)
            lanternHand.ShowLantern(flashlightDuration, flashDir);

        // ¿El fantasma está en el cono de luz?
        if (enemyScript == null)
            enemyScript = Object.FindFirstObjectByType<EnemyIA>();

        if (enemyScript != null)
        {
            float  distToEnemy = Vector2.Distance(transform.position, enemyScript.transform.position);
            Vector2 dirToEnemy = ((Vector2)enemyScript.transform.position - (Vector2)transform.position).normalized;
            float   angle      = Vector2.Angle(flashDir, dirToEnemy);

            if (distToEnemy <= flashDistance && angle <= flashConeAngle)
                enemyScript.RecibirLuzLinterna();
        }
    }

    void DesactivarLinterna()
    {
        flashlightActive = false;
        if (lanternHand != null) lanternHand.HideLantern();
    }

    // Llamado por FlashlightItem al recogerlo
    public void AddFlashlightCharges(int amount)
    {
        currentCharges = Mathf.Min(currentCharges + amount, maxFlashlightCharges);
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateFlashlightUI(currentCharges, maxFlashlightCharges);
    }

    // =============================================================
    //  TRANSICIÓN DE ESCENA
    // =============================================================
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Casa") && !isTransitioning)
            StartCoroutine(TransicionAInterior());
    }

    IEnumerator TransicionAInterior()
    {
        isTransitioning = true;
        if (animator) animator.SetBool("IsMoving", false);
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
            timer    += Time.deltaTime;
            color.a   = Mathf.Clamp01(timer / fadeDuration);
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
            timer    += Time.deltaTime;
            color.a   = Mathf.Clamp01(1f - timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    // =============================================================
    //  GIZMOS
    // =============================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, flashDistance);
    }
}