using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// GameManager — Singleton que controla el estado global del juego.
///
/// SETUP EN UNITY:
/// 1. Crea un GameObject vacío en la escena llamado "GameManager".
/// 2. Agrega este script como componente.
/// 3. En el Canvas, crea los paneles de UI y asígnalos en el Inspector.
/// 4. Asigna los spawn points (GameObjects vacíos) para el item de linterna.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ─────────────────────────────────────────────────────────────
    //  TIMER — UNA NOCHE (3 minutos)
    // ─────────────────────────────────────────────────────────────
    [Header("Timer — Una Noche (180 segundos)")]
    public float nightDuration = 180f;  // 3 minutos = 1 noche completa
    private float timeRemaining;
    private bool  gameActive = false;

    // ─────────────────────────────────────────────────────────────
    //  UI — TIMER
    // ─────────────────────────────────────────────────────────────
    [Header("UI — Timer de la Noche")]
    public TextMeshProUGUI timerText;       // Ej: "🌙 02:47"
    public Image           nightProgressBar; // Barra fill de progreso de la noche

    // ─────────────────────────────────────────────────────────────
    //  UI — LINTERNA
    // ─────────────────────────────────────────────────────────────
    [Header("UI — Linterna")]
    public TextMeshProUGUI flashlightChargesText;  // Ej: "⚡ 7/10"
    [Tooltip("Array de 10 Image icons para las cargas (opcional)")]
    public Image[] chargeIcons;

    // ─────────────────────────────────────────────────────────────
    //  UI — PANELES GAME OVER / WIN
    // ─────────────────────────────────────────────────────────────
    [Header("UI — Paneles")]
    public GameObject gameOverPanel;   // Panel "GAME OVER" (inactivo al inicio)
    public GameObject winPanel;        // Panel "¡SOBREVIVISTE!" (inactivo al inicio)

    // ─────────────────────────────────────────────────────────────
    //  EFECTO PELIGRO — Overlay rojo pulsante
    // ─────────────────────────────────────────────────────────────
    [Header("UI — Pulso de Peligro")]
    [Tooltip("Image roja que cubre toda la pantalla (alpha 0 al inicio)")]
    public Image dangerOverlay;
    [Tooltip("Distancia desde la que se activa el efecto rojo")]
    public float dangerDistance = 5f;

    private float dangerAlpha = 0f;

    // ─────────────────────────────────────────────────────────────
    //  ITEM DE RECARGA — Spawn cada 30 segundos
    // ─────────────────────────────────────────────────────────────
    [Header("Item de Recarga de Linterna")]
    public GameObject      flashlightItemPrefab;  // Prefab con FlashlightItem.cs
    public Transform[]     itemSpawnPoints;        // GameObjects vacíos en el mapa
    public float           itemSpawnInterval = 30f;

    private float      itemSpawnTimer = 0f;
    private GameObject currentItem   = null;

    // ─────────────────────────────────────────────────────────────
    //  MODO CAZADOR (Power Item)
    // ─────────────────────────────────────────────────────────────
    [Header("Item de Poder (Cazador)")]
    public GameObject powerItemPrefab;
    public float      powerSpawnInterval = 15f; // Aparece cada 15 segundos
    
    [HideInInspector] public bool isHunterModeActive = false;
    private float      hunterModeTimer     = 0f;
    private float      powerSpawnTimer     = 0f;
    private GameObject currentPowerItem    = null;

    // =============================================================
    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        timeRemaining = nightDuration;
        gameActive    = true;

        // Ocultar paneles al inicio
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (winPanel)      winPanel.SetActive(false);

        // Overlay transparente
        if (dangerOverlay) dangerOverlay.color = new Color(1f, 0f, 0f, 0f);

        // UI inicial
        UpdateTimerUI();
    }

    void Update()
    {
        if (!gameActive) return;

        // ── Cuenta regresiva ──
        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        // ── Spawn de item ──
        itemSpawnTimer += Time.deltaTime;
        if (itemSpawnTimer >= itemSpawnInterval)
        {
            itemSpawnTimer = 0f;
            SpawnFlashlightItem();
        }

        // ── Spawn de Power Item ──
        powerSpawnTimer += Time.deltaTime;
        if (powerSpawnTimer >= powerSpawnInterval)
        {
            powerSpawnTimer = 0f;
            SpawnPowerItem();
        }

        // ── Temporizador Modo Cazador ──
        if (isHunterModeActive)
        {
            hunterModeTimer -= Time.deltaTime;
            if (hunterModeTimer <= 0f)
            {
                isHunterModeActive = false;
            }
        }

        // ── Pulso de peligro / Caza ──
        if (dangerOverlay != null)
        {
            float pulse = (Mathf.Sin(Time.time * 3.5f) * 0.5f + 0.5f); // 0–1 oscilante
            
            // Si somos cazador: amarillo. Si nos persiguen: rojo.
            Color baseColor = isHunterModeActive ? Color.yellow : Color.red;
            
            baseColor.a = dangerAlpha * pulse * 0.45f;
            dangerOverlay.color = baseColor;
        }

        // ── Victoria si sobrevive la noche ──
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            TriggerWin();
        }
    }

    // =============================================================
    //  ACTUALIZAR TIMER UI
    // =============================================================
    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        if (timerText) timerText.text = $"{minutes:00}:{seconds:00}";

        // Barra de progreso de la noche (vacía al terminar → llena al inicio)
        if (nightProgressBar)
            nightProgressBar.fillAmount = timeRemaining / nightDuration;
    }

    // =============================================================
    //  NIVEL DE PELIGRO — llamado por EnemyIA cada frame
    // =============================================================
    public void UpdateDangerLevel(float distanceToEnemy)
    {
        // Normalizamos: 0 = lejos, 1 = encima
        float target = 1f - Mathf.Clamp01(distanceToEnemy / dangerDistance);
        dangerAlpha  = Mathf.Lerp(dangerAlpha, target, Time.deltaTime * 2.5f);
    }

    // =============================================================
    //  ACTUALIZAR UI LINTERNA
    // =============================================================
    public void UpdateFlashlightUI(int current, int max)
    {
        if (flashlightChargesText)
            flashlightChargesText.text = $"{current}/{max}";

        // Íconos individuales (si están asignados)
        if (chargeIcons != null)
        {
            for (int i = 0; i < chargeIcons.Length; i++)
            {
                if (chargeIcons[i] == null) continue;
                // Activas = blanco, gastadas = gris oscuro
                chargeIcons[i].color = i < current
                    ? Color.white
                    : new Color(0.15f, 0.15f, 0.15f, 0.5f);
            }
        }
    }

    // =============================================================
    //  GAME OVER
    // =============================================================
    public void TriggerGameOver()
    {
        if (!gameActive) return;
        gameActive       = false;
        Time.timeScale   = 0f;

        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    // =============================================================
    //  VICTORIA
    // =============================================================
    public void TriggerWin()
    {
        if (!gameActive) return;
        gameActive       = false;
        Time.timeScale   = 0f;

        if (winPanel) winPanel.SetActive(true);
    }

    // =============================================================
    //  REINICIAR JUEGO
    // =============================================================
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // =============================================================
    //  SPAWN ITEM LINTERNA
    // =============================================================
    void SpawnFlashlightItem()
    {
        if (flashlightItemPrefab == null) return;
        if (itemSpawnPoints    == null || itemSpawnPoints.Length == 0) return;

        // Eliminar item anterior si no fue recogido
        if (currentItem != null) Destroy(currentItem);

        int       idx      = Random.Range(0, itemSpawnPoints.Length);
        Vector3   spawnPos = itemSpawnPoints[idx].position;
        currentItem        = Instantiate(flashlightItemPrefab, spawnPos, Quaternion.identity);
    }

    // =============================================================
    //  SPAWN POWER ITEM
    // =============================================================
    void SpawnPowerItem()
    {
        if (powerItemPrefab == null) return;
        if (itemSpawnPoints == null || itemSpawnPoints.Length == 0) return;

        if (currentPowerItem != null) Destroy(currentPowerItem);

        int       idx      = Random.Range(0, itemSpawnPoints.Length);
        Vector3   spawnPos = itemSpawnPoints[idx].position;
        currentPowerItem   = Instantiate(powerItemPrefab, spawnPos, Quaternion.identity);
    }

    // =============================================================
    //  ACTIVAR MODO CAZADOR
    // =============================================================
    public void ActivateHunterMode(float duration)
    {
        isHunterModeActive = true;
        hunterModeTimer    = duration;
    }
}
