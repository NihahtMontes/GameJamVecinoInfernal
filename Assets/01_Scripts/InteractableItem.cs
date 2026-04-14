using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    public Dialogue dialogue;
    public int dialogueId = 1;
    public bool disappearAfterUse = false; // ¿desaparece el objeto?

    private bool _used = false;
    private bool _playerNearby = false;

    void Update()
    {
        // El player presiona E cerca del objeto
        if (_playerNearby && !_used && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogue.IsActive()) return; // evita conflictos

            _used = true;
            dialogue.StartDialogue(dialogueId);

            if (disappearAfterUse)
                gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = false;
    }
}