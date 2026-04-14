using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)] public string text;
    public bool blockPlayer = true;
}

[System.Serializable]
public class DialogueBlock
{
    public int id;
    public DialogueLine[] lines;
}

public class Dialogue : MonoBehaviour
{
    [Header("Bloques de diálogo")]
    public DialogueBlock[] dialogueBlocks;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public GameObject dialogueMark;

    [Header("Referencias")]
    public Player playerScript;

    [Header("Auto inicio")]
    public bool autoStart = false;
    public int autoStartId = 0;

    [Range(0.01f, 0.08f)] public float charDelay = 0.035f;

    private DialogueLine[] _currentLines;
    private int _lineIndex;
    private bool _isActive;
    private bool _isTyping;
    private Coroutine _typingCoroutine;

    void Start()
    {
        // Validar referencias
        if (dialoguePanel == null)
        {
            Debug.LogError("[Dialogue] Falta asignar 'Dialogue Panel' en el Inspector!");
            return;
        }
        if (dialogueText == null)
        {
            Debug.LogError("[Dialogue] Falta asignar 'Dialogue Text' en el Inspector!");
            return;
        }
        if (dialogueMark == null)
        {
            Debug.LogError("[Dialogue] Falta asignar 'Dialogue Mark' en el Inspector!");
            return;
        }

        Debug.Log($"[Dialogue] Start OK — autoStart={autoStart}, autoStartId={autoStartId}, bloques={dialogueBlocks?.Length ?? 0}");

        // Asegurarse que empieza oculto
        dialoguePanel.SetActive(false);
        dialogueMark.SetActive(false);

        // Auto arranque con pequeño delay para no pisar el FadeIn del Player
        if (autoStart)
            StartCoroutine(AutoStartDelay());
    }

    IEnumerator AutoStartDelay()
    {
        // Espera 1 frame para que Player.Start() termine
        yield return null;
        StartDialogue(autoStartId);
    }

    void Update()
    {
        if (!_isActive) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            if (_isTyping)
                SkipTyping();
            else
                NextLine();
        }
    }

    // ── API pública ─────────────────────────────────────────

    public void StartDialogue(int id)
    {
        if (_isActive) return;

        DialogueBlock block = System.Array.Find(dialogueBlocks, b => b.id == id);
        if (block == null || block.lines == null || block.lines.Length == 0)
        {
            Debug.LogWarning($"[Dialogue] No encontré bloque con ID {id} o está vacío.");
            return;
        }

        _currentLines = block.lines;
        _lineIndex = 0;
        _isActive = true;
        dialoguePanel.SetActive(true);
        dialogueMark.SetActive(false);
        ShowCurrentLine();
    }

    public bool IsActive() => _isActive;

    // ── Lógica interna ──────────────────────────────────────

    void ShowCurrentLine()
    {
        DialogueLine line = _currentLines[_lineIndex];

        if (playerScript != null)
            playerScript.canMove = !line.blockPlayer;

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeLine(line.text));
    }

    IEnumerator TypeLine(string fullText)
    {
        _isTyping = true;
        dialogueMark.SetActive(false);
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        _isTyping = false;
        dialogueMark.SetActive(true);
    }

    void SkipTyping()
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        dialogueText.text = _currentLines[_lineIndex].text;
        _isTyping = false;
        dialogueMark.SetActive(true);
    }

    void NextLine()
    {
        _lineIndex++;
        if (_lineIndex < _currentLines.Length)
            ShowCurrentLine();
        else
            EndDialogue();
    }

    void EndDialogue()
    {
        _isActive = false;
        dialoguePanel.SetActive(false);
        dialogueMark.SetActive(false);

        if (playerScript != null)
            playerScript.canMove = true;
    }
}