using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private bool isTransitioning = false;

    // Para el fade — asigna en el Inspector un Image negro que cubra toda la pantalla
    public Image fadeImage;
    public float fadeDuration = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Empieza el fade de entrada (negro → transparente)
        if (fadeImage != null)
            StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (isTransitioning) return; // bloquea movimiento durante transición

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

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
    }

    void FixedUpdate()
    {
        if (isTransitioning) return;
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
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
}