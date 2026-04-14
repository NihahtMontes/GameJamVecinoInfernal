using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public int dialogueId = 0;
    public bool triggerOnce = true; // solo dispara una vez

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        if (triggerOnce) _triggered = true;
        dialogue.StartDialogue(dialogueId);
    }
}