using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyIA : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 2.5f;

    [Header("Detección")]
    public float hearingRange = 5f;
    public float visionRange = 4f;
    public LayerMask obstacleMask; // Configura esto en "Default" o crea una capa "Paredes"

    [Header("Ruta de Patrullaje")]
    public List<Transform> patrolPoints;
    private int currentPointIndex = 0;
    private float waitTimer;

    private Player playerScript;
    private bool isChasing = false;
    private bool isStunned = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerScript = Object.FindFirstObjectByType<Player>();
    }

    void Update()
    {
        if (isStunned || playerScript == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);
        bool canSeePlayer = CheckLineOfSight();

        // LÓGICA DE DETECCIÓN
        if (canSeePlayer || (playerScript.isMakingNoise && distanceToPlayer < hearingRange))
        {
            isChasing = true;
        }
        else
        {
            if (!canSeePlayer) isChasing = false;
        }

        if (isChasing)
            PerseguirPlayer();
        else
            Patrullar();
    }

    bool CheckLineOfSight()
    {
        Vector2 direction = (playerScript.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, playerScript.transform.position);

        if (distance > visionRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, visionRange, obstacleMask);

        if (hit.collider == null || hit.collider.CompareTag("Player"))
            return true;

        return false;
    }

    void PerseguirPlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, playerScript.transform.position, chaseSpeed * Time.deltaTime);
    }

    void Patrullar()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > 2.0f)
            {
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
                waitTimer = 0;
            }
        }
    }

    public void RecibirLuzLinterna()
    {
        if (!isStunned) StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        isChasing = false;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        sr.color = Color.yellow;

        yield return new WaitForSeconds(2f);

        sr.color = originalColor;
        isStunned = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡GAME OVER!");
            // Aquí puedes reiniciar la escena o activar tu animación de muerte
        }
    }
}