using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed = 3f; // Ajustada para que no sea tan veloz
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;

    [Header("Habilidades")]
    public float flashDistance = 2.0f;
    public KeyCode attackKey = KeyCode.Space;

    // Esta variable la leerá el enemigo
    [HideInInspector] public bool isMakingNoise = false;
    private EnemyIA enemyScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        // Busca al enemigo automáticamente al iniciar
        enemyScript = Object.FindFirstObjectByType<EnemyIA>();
    }

    void Update()
    {
        // Entrada de movimiento
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
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }

    void AtacarConLinterna()
    {
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

    // Dibujar el círculo de la linterna en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, flashDistance);
    }
}