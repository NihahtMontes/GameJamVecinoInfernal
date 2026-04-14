using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EnemyIA : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  VELOCIDADES
    // ─────────────────────────────────────────────────────────────
    [Header("Configuración de Movimiento")]
    public float patrolSpeed  = 1.5f;   // Patrullaje normal
    public float chaseSpeed   = 2.8f;   // Solo cuando VE al jugador
    // Al ESCUCHAR usa patrolSpeed (no se acelera, interrumpe ruta)

    // ─────────────────────────────────────────────────────────────
    //  DETECCIÓN
    // ─────────────────────────────────────────────────────────────
    [Header("Detección")]
    public float hearingRange = 5f;     // Sonido atraviesa paredes
    public float visionRange  = 4f;     // Vista bloqueada por paredes
    public LayerMask obstacleMask;      // Layer "Walls" / "Default"

    // ─────────────────────────────────────────────────────────────
    //  PATRULLAJE ALEATORIO
    // ─────────────────────────────────────────────────────────────
    [Header("Ruta de Patrullaje (FirePoints)")]
    public List<Transform> patrolPoints;

    private List<int> shuffledIndices = new List<int>();
    private int shuffleIndex = 0;
    private float waitTimer   = 0f;
    private float waitDuration = 1.5f;

    // ─────────────────────────────────────────────────────────────
    //  INDICADOR DE ESTADO SOBRE EL FANTASMA
    // ─────────────────────────────────────────────────────────────
    [Header("Indicador Visual (opcional — TextMeshPro hijo)")]
    public TextMeshPro detectionText;   // Arrastra el TMP hijo aquí

    // ─────────────────────────────────────────────────────────────
    //  ESTADO INTERNO
    // ─────────────────────────────────────────────────────────────
    [Header("Vida del Fantasma")]
    public int maxHealth = 8;
    private int currentHealth;

    private Player  playerScript;
    private bool    isChasing  = false;
    private bool    isStunned  = false;
    private bool    heardPlayer = false;
    private Vector2 lastHeardPosition;
    private Rigidbody2D rb;
    private Animator animator;

    // =============================================================
    void Start()
    {
        rb           = GetComponent<Rigidbody2D>();
        animator     = GetComponent<Animator>();
        playerScript = Object.FindFirstObjectByType<Player>();
        currentHealth = maxHealth;
        GenerarRecorridoAleatorio();
    }

    // =============================================================
    void Update()
    {
        if (isStunned || playerScript == null)
        {
            if (animator != null) animator.SetBool("IsMoving", false);
            return;
        }

        Vector2 posBefore = transform.position;

        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);
        bool  canSeePlayer     = CheckLineOfSight();
        bool  canHearPlayer    = playerScript.isMakingNoise && distanceToPlayer < hearingRange;

        // ── Indicador visual ──
        UpdateIndicator(canSeePlayer, canHearPlayer);

        // ── Pulso de peligro en GameManager ──
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateDangerLevel(distanceToPlayer);

        // ── Lógica de estado ──
        if (canSeePlayer)
        {
            isChasing   = true;
            heardPlayer = false;
        }
        else if (canHearPlayer)
        {
            // Escucha: interrumpe ruta, va al sonido a velocidad de patrullaje
            isChasing          = false;
            heardPlayer        = true;
            lastHeardPosition  = playerScript.transform.position;
        }
        else
        {
            isChasing   = false;
            heardPlayer = false;
        }

        // ── Acción según estado ──
        if (GameManager.Instance != null && GameManager.Instance.isHunterModeActive)
        {
            // El fantasma huye del jugador!
            isChasing = false;
            heardPlayer = false;
            HuirDelJugador();
        }
        else
        {
            if      (isChasing)  PerseguirPlayer();
            else if (heardPlayer) IrHaciaSonido();
            else                  Patrullar();
        }

        // ── Actualizar Animaciones ──
        if (animator != null)
        {
            Vector2 posAfter = transform.position;
            Vector2 movement = (posAfter - posBefore);
            
            if (movement.magnitude > 0.001f)
            {
                animator.SetBool("IsMoving", true);
                if (Mathf.Abs(movement.y) > Mathf.Abs(movement.x))
                {
                    animator.SetFloat("MoveX", 0f);
                    animator.SetFloat("MoveY", movement.y > 0 ? 1f : -1f);
                }
                else
                {
                    animator.SetFloat("MoveX", movement.x > 0 ? 1f : -1f);
                    animator.SetFloat("MoveY", 0f);
                }
            }
            else
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    // =============================================================
    //  LÍNEA DE VISIÓN (bloqueada por paredes)
    // =============================================================
    bool CheckLineOfSight()
    {
        float  dist      = Vector2.Distance(transform.position, playerScript.transform.position);
        if (dist > visionRange) return false;

        Vector2        dir = (playerScript.transform.position - transform.position).normalized;
        RaycastHit2D   hit = Physics2D.Raycast(transform.position, dir, visionRange, obstacleMask);

        return (hit.collider == null || hit.collider.CompareTag("Player"));
    }

    // =============================================================
    //  HUIR (Modo Cazador)
    // =============================================================
    void HuirDelJugador()
    {
        Vector2 dirLejos = (transform.position - playerScript.transform.position).normalized;
        Vector2 objetivo = (Vector2)transform.position + dirLejos;
        
        transform.position = Vector2.MoveTowards(
            transform.position, objetivo,
            patrolSpeed * Time.deltaTime);
    }

    // =============================================================
    //  PERSEGUIR (velocidad de caza — sólo cuando VE)
    // =============================================================
    void PerseguirPlayer()
    {
        transform.position = Vector2.MoveTowards(
            transform.position, playerScript.transform.position,
            chaseSpeed * Time.deltaTime);
    }

    // =============================================================
    //  IR AL SONIDO (velocidad de patrullaje — atraviesa paredes NO)
    // =============================================================
    void IrHaciaSonido()
    {
        transform.position = Vector2.MoveTowards(
            transform.position, lastHeardPosition,
            patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, lastHeardPosition) < 0.2f)
            heardPlayer = false;
    }

    // =============================================================
    //  PATRULLAR ALEATORIO
    // =============================================================
    void Patrullar()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        int realIndex = shuffledIndices[shuffleIndex];
        Transform goal = patrolPoints[realIndex];

        transform.position = Vector2.MoveTowards(
            transform.position, goal.position,
            patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, goal.position) < 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitDuration)
            {
                shuffleIndex++;
                if (shuffleIndex >= shuffledIndices.Count)
                    GenerarRecorridoAleatorio();     // re-barajar al completar ciclo

                waitDuration = Random.Range(0.8f, 2.5f); // pausa aleatoria en cada punto
                waitTimer    = 0f;
            }
        }
    }

    // Fisher-Yates shuffle sobre los índices
    void GenerarRecorridoAleatorio()
    {
        shuffledIndices.Clear();
        for (int i = 0; i < patrolPoints.Count; i++) shuffledIndices.Add(i);

        for (int i = shuffledIndices.Count - 1; i > 0; i--)
        {
            int j    = Random.Range(0, i + 1);
            int temp = shuffledIndices[i];
            shuffledIndices[i] = shuffledIndices[j];
            shuffledIndices[j] = temp;
        }
        shuffleIndex = 0;
    }

    // =============================================================
    //  INDICADOR VISUAL SOBRE EL FANTASMA
    // =============================================================
    void UpdateIndicator(bool sees, bool hears)
    {
        if (detectionText == null) return;
        if      (sees)  detectionText.text = "!";   // VE al jugador
        else if (hears) detectionText.text = "?";   // ESCUCHA al jugador
        else            detectionText.text = "";
    }

    // =============================================================
    //  RECIBIR LUZ DE LINTERNA → Daño o Aturdimiento
    // =============================================================
    public void RecibirLuzLinterna()
    {
        if (isStunned) return;

        if (GameManager.Instance != null && GameManager.Instance.isHunterModeActive)
        {
            StartCoroutine(DamageRoutine());
        }
        else
        {
            StartCoroutine(StunRoutine());
        }
    }

    IEnumerator DamageRoutine()
    {
        isStunned = true; // Pausarlo un momentito por el impacto
        currentHealth--;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr ? sr.color : Color.white;
        if (sr) sr.color = Color.red; // Efecto de daño sangriento

        yield return new WaitForSeconds(0.4f); // Pequeño retroceso/descanso

        if (sr) sr.color = originalColor;
        isStunned = false;

        if (currentHealth <= 0)
        {
            // Fantasma destruido, ganar partida
            if (GameManager.Instance != null)
                GameManager.Instance.TriggerWin();
            
            Destroy(gameObject);
        }
    }

    IEnumerator StunRoutine()
    {
        isStunned   = true;
        isChasing   = false;
        heardPlayer = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr ? sr.color : Color.white;
        if (sr) sr.color = Color.yellow;

        yield return new WaitForSeconds(3f); // 3 segundos de aturdimiento

        if (sr) sr.color = originalColor;
        isStunned = false;
    }

    // =============================================================
    //  CONTACTO CON EL JUGADOR → GAME OVER
    // =============================================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TriggerGameOver();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            TriggerGameOver();
    }

    void TriggerGameOver()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();
        else
            Debug.Log("¡GAME OVER! Asigna GameManager en la escena.");
    }

    // =============================================================
    //  GIZMOS — Visualizar rangos en el Editor
    // =============================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}