using UnityEngine;

/// <summary>
/// AudioManager — Singleton persistente entre escenas.
/// Controla todos los AudioSources del juego.
///
/// SETUP EN UNITY (una sola vez):
/// 1. Crea un GameObject vacio llamado "AudioManager".
/// 2. Agrega este script.
/// 3. Asigna los 6 clips de audio en el Inspector.
/// 4. Arrastra el AudioManager desde la escena al campo DontDestroyOnLoad
///    (o dejalo suelto, el script lo hace automaticamente).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Clips de Audio (arrastra desde 05_Sonidos)")]
    public AudioClip sonSuspenso;     // Loop permanente de fondo
    public AudioClip sonPersecucion;  // Loop durante persecucion
    public AudioClip sonPasos;        // Loop mientras el player se mueve
    public AudioClip sonGameOver;     // Una vez al perder
    public AudioClip sonGanar;        // Una vez al ganar
    public AudioClip sonTeclado;      // Loop mientras se escribe dialogo

    // Sources separados para cada "canal"
    private AudioSource srcSuspenso;
    private AudioSource srcPersecucion;
    private AudioSource srcPasos;
    private AudioSource srcEvento;    // GameOver y Ganar (one-shot)
    private AudioSource srcTeclado;

    // Estado
    private bool isPersecutionPlaying = false;
    private bool isPasosPlaying       = false;
    private bool isTecladoPlaying     = false;

    // =====================================================================
    void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Crear los AudioSources como componentes de este mismo objeto
        srcSuspenso    = CrearSource(1f,   true);
        srcPersecucion = CrearSource(0.85f, true);
        srcPasos       = CrearSource(0.6f,  true);
        srcEvento      = CrearSource(1f,   false);
        srcTeclado     = CrearSource(0.5f,  true);
    }

    void Start()
    {
        // Iniciar suspenso de fondo automaticamente
        PlaySuspenso();
    }

    // =====================================================================
    //  HELPERS
    // =====================================================================
    AudioSource CrearSource(float volumen, bool loop)
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.volume      = volumen;
        src.loop        = loop;
        src.playOnAwake = false;
        return src;
    }

    // =====================================================================
    //  SUSPENSO (fondo permanente)
    // =====================================================================
    public void PlaySuspenso()
    {
        if (sonSuspenso == null || srcSuspenso.isPlaying) return;
        srcSuspenso.clip = sonSuspenso;
        srcSuspenso.Play();
    }

    // =====================================================================
    //  PERSECUCION (loop mientras haya chase)
    // =====================================================================
    public void StartPersecucion()
    {
        if (isPersecutionPlaying || sonPersecucion == null) return;
        isPersecutionPlaying   = true;
        srcPersecucion.clip    = sonPersecucion;
        srcPersecucion.Play();
        // Bajar suspenso mientras hay persecucion
        srcSuspenso.volume     = 0.2f;
    }

    public void StopPersecucion()
    {
        if (!isPersecutionPlaying) return;
        isPersecutionPlaying   = false;
        srcPersecucion.Stop();
        srcSuspenso.volume     = 1f;
    }

    // =====================================================================
    //  PASOS (loop mientras el player se mueve)
    // =====================================================================
    public void StartPasos()
    {
        if (isPasosPlaying || sonPasos == null) return;
        isPasosPlaying      = true;
        srcPasos.clip       = sonPasos;
        srcPasos.Play();
    }

    public void StopPasos()
    {
        if (!isPasosPlaying) return;
        isPasosPlaying = false;
        srcPasos.Stop();
    }

    // =====================================================================
    //  EVENTOS UNICOS: GAME OVER / GANAR
    // =====================================================================
    public void PlayGameOver()
    {
        StopAll();
        if (sonGameOver == null) return;
        srcEvento.clip = sonGameOver;
        srcEvento.Play();
    }

    public void PlayGanar()
    {
        StopAll();
        if (sonGanar == null) return;
        srcEvento.clip = sonGanar;
        srcEvento.Play();
    }

    // =====================================================================
    //  TECLADO (dialogo)
    // =====================================================================
    public void StartTeclado()
    {
        if (isTecladoPlaying || sonTeclado == null) return;
        isTecladoPlaying    = true;
        srcTeclado.clip     = sonTeclado;
        srcTeclado.Play();
    }

    public void StopTeclado()
    {
        if (!isTecladoPlaying) return;
        isTecladoPlaying = false;
        srcTeclado.Stop();
    }

    // =====================================================================
    //  PARAR TODO (al ganar/perder)
    // =====================================================================
    void StopAll()
    {
        srcSuspenso.Stop();
        srcPersecucion.Stop();
        srcPasos.Stop();
        srcTeclado.Stop();
        isPersecutionPlaying = false;
        isPasosPlaying       = false;
        isTecladoPlaying     = false;
    }
}
