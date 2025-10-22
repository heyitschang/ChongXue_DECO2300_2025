using UnityEngine;
using UnityEngine.SceneManagement;

// AppVoiceExperience lives here in your SDK build
using Oculus.Voice;
using Meta.WitAi;
using Meta.WitAi.Events;

public class VoiceCommandManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the AppVoiceExperience from the Hierarchy (auto-finds if left empty).")]
    public AppVoiceExperience voice;

    [Tooltip("Paint360PassthroughMode in the scene (auto-finds if left empty).")]
    public Paint360PassthroughMode passthrough;

    [Header("Phrases that trigger NEW")]
    public string[] newScenePhrases = { "new", "reset scene", "clear scene", "start over", "reset" };

    [Header("Phrases that toggle MODE")]
    public string[] modePhrases = { "mode", "toggle mode", "passthrough", "real world", "toggle background" };

    void Awake()
    {
        if (!voice) voice = FindObjectOfType<AppVoiceExperience>();
        if (!voice)
        {
            Debug.LogError("[VoiceCommandManager] AppVoiceExperience not found in scene!");
            enabled = false;
            return;
        }

        if (!passthrough) passthrough = FindObjectOfType<Paint360PassthroughMode>();

        voice.TranscriptionEvents.OnFullTranscription.AddListener(OnTranscript);
    }

    void OnEnable()
    {
        voice.VoiceEvents.OnStoppedListening.AddListener(OnStopped);
        voice.Activate(); // start continuous listening
    }

    void OnDisable()
    {
        voice.TranscriptionEvents.OnFullTranscription.RemoveListener(OnTranscript);
        voice.VoiceEvents.OnStoppedListening.RemoveListener(OnStopped);
    }

    void OnStopped()
    {
        // keep listening
        if (voice) voice.Activate();
    }

    void OnTranscript(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var t = text.ToLowerInvariant();
        Debug.Log($"[Voice] Heard: {t}");

        // NEW
        foreach (var p in newScenePhrases)
            if (t.Contains(p)) { ReloadScene(); return; }

        // MODE
        foreach (var p in modePhrases)
            if (t.Contains(p)) { ToggleMode(); return; }
    }

    void ReloadScene()
    {
        var s = SceneManager.GetActiveScene();
        Debug.Log($"[Voice] NEW → reloading {s.name}");
        SceneManager.LoadScene(s.buildIndex);
    }

    void ToggleMode()
    {
        if (!passthrough)
        {
            Debug.LogWarning("[Voice] MODE → No Paint360PassthroughMode found in scene.");
            return;
        }
        passthrough.ToggleMode();
        Debug.Log($"[Voice] MODE → {passthrough.Current}");
    }
}
