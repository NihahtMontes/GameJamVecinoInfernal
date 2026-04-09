using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.magnitude > 0)
        {
            animator.SetBool("IsMoving", true);

            // Prioridad vertical: si hay movimiento en Y, ignora X para la animación
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
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }
}