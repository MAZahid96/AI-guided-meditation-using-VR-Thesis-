#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class EditorTTS : MonoBehaviour
{
    void Awake() => DontDestroyOnLoad(gameObject);

    public void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        // Escape quotes for VBScript
        string safe = text.Replace("\"", "\"\"");

        // Build command line
        string vbscript  = $"CreateObject(\"SAPI.SpVoice\").Speak(\"{safe}\")";
        string cmdArgs   = "/c mshta \"javascript:var sh=new ActiveXObject('WScript.Shell');" +
                           $" sh.Run('wscript /e:vbscript \"{vbscript}\"');close();\"";

        var psi = new ProcessStartInfo("cmd.exe", cmdArgs)
        {
            CreateNoWindow  = true,
            UseShellExecute = false
        };

        try { Process.Start(psi); }
        catch (System.Exception ex) { Debug.LogError("EditorTTS ERROR: " + ex.Message); }
    }
}
#endif
