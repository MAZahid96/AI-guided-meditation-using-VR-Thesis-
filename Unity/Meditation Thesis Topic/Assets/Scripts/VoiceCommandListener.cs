using UnityEngine;
using System;
using System.Speech.Recognition;    // Windows only

public class VoiceCommandListener : MonoBehaviour
{
    public event Action<string> onCommandRecognized; // Scene1Controller subscribes
    private SpeechRecognitionEngine recognizer;

    void Start()
    {
        try
        {
            recognizer = new SpeechRecognitionEngine();
            recognizer.LoadGrammar(
                new Grammar(new GrammarBuilder(new Choices("yes", "no", "start")))
            );
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.SpeechRecognized += (_, e) =>
            {
                string cmd = e.Result.Text.ToLower();
                UnityEngine.Debug.Log("Recognized: " + cmd);
                onCommandRecognized?.Invoke(cmd);
            };
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            UnityEngine.Debug.Log("Voice recognizer started.");
        }
        catch (Exception ex) { UnityEngine.Debug.LogError("Speech init failed: " + ex.Message); }
    }

    void OnDestroy() { recognizer?.Dispose(); }
}
