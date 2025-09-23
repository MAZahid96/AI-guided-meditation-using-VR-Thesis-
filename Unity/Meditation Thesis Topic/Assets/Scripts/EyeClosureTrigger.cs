  //  using UnityEngine;
  //  using UnityEngine.SceneManagement;
    //using ViveSR.anipal.Eye;
 //   using static Valve.VR.SteamVR_TrackedObject;

//    public class EyeClosureTrigger : MonoBehaviour
/* 
{
        [Header("Detection")]
        [Tooltip("Opennes threshold below which a eye is considered cöosed. Typical 0.25-0.45")]
        [Range(0f, 1f)] public float closedThreshold = 0.35f;

        [Tooltip("How long the eyes must be closed before triggering.")]
        [Range(0.05f, 2f)] public float holdDuration = 0.35f;

        [Tooltip("Exponential smoothing for openness (higher = smoother = more delay)")]
        [Range(0f, 0.99f)] public float smoothing = 0.4f;

        [Header("Action")]
        [Tooltip("exact name of scene to load (must be in build settings)")]
        public string nextScene = "meditationScene";


        [Tooltip("trigger only once (recommended)")]
        public bool triggerOnce = true;

        [Header ("Debug")]

        public bool logDebug = false;
        public bool simulateWithKeyboard = true;   // Hold SPACE to simulate eyes closed

        private float _leftEMA = 1f;
        private float _rightEMA = 1f;
        private float _closedTimer = 0f;
        private bool _fired = false;

        void Awake()
        {
            // Optional: ensure SRanipal framework exists
            var framework = FindObjectOfType<SRanipal_Eye_Framework>();
            if (framework == null)
            {
                Debug.LogWarning("[EyelidSceneAdvance] SRanipal_Eye_Framework not found in scene. Add it for real data.");
            }
            else
            {
                framework.StartFramework(); // safe if already started
            }
        }

        void Update()
        {
            if (_fired && triggerOnce) return;

            // 1) Read openness from SRanipal (0..1). If unavailable, keep last.
            float leftOpen = _leftEMA;
            float rightOpen = _rightEMA;

            bool gotLeft = SRanipal_Eye_API.GetEyeOpenness(EyeIndex.LEFT, out leftOpen) == ViveSR.Error.WORK;
            bool gotRight = SRanipal_Eye_API.GetEyeOpenness(EyeIndex.RIGHT, out rightOpen) == ViveSR.Error.WORK;

            // 2) Optional simulation (for testing without headset or to force)
            if (simulateWithKeyboard && Input.GetKey(KeyCode.Space))
            {
                gotLeft = gotRight = true;
                leftOpen = rightOpen = 0.0f; // fully closed
            }

            // 3) Exponential smoothing
            if (gotLeft) _leftEMA = Mathf.Lerp(leftOpen, _leftEMA, smoothing);
            if (gotRight) _rightEMA = Mathf.Lerp(rightOpen, _rightEMA, smoothing);

            bool leftClosed = _leftEMA < closedThreshold;
            bool rightClosed = _rightEMA < closedThreshold;
            bool bothClosed = leftClosed && rightClosed;

            // 4) Dwell / hold timer
            if (bothClosed)
                _closedTimer += Time.deltaTime;
            else
                _closedTimer = 0f;

            if (logDebug && Time.frameCount % 10 == 0)
            {
                Debug.Log($"[Eyelid] L:{_leftEMA:0.00} R:{_rightEMA:0.00} closed(L/R):{leftClosed}/{rightClosed} timer:{_closedTimer:0.00}");
            }

            // 5) Fire action
            if (_closedTimer >= holdDuration)
            {
                Trigger();
            }
        }

        private void Trigger()
        {
            if (_fired && triggerOnce) return;
            _fired = true;

            if (string.IsNullOrEmpty(nextScene))
            {
                Debug.LogError("[EyelidSceneAdvance] Next scene name is empty. Set it in the inspector.");
                return;
            }

            // Optional small delay to avoid loading mid‑blink noise
            StartCoroutine(LoadNextAtEndOfFrame());
        }

        System.Collections.IEnumerator LoadNextAtEndOfFrame()
        {
            if (logDebug) Debug.Log($"[EyelidSceneAdvance] Loading scene: {nextScene}");
            yield return new WaitForEndOfFrame();
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
    }

    //[Tooltip("Opennes threshold below which a eye is considered closed. Typical 0.25-0.45")]

    //[Header("Detection")]
      //  void Update()
        //{
          //  if (Input.GetKeyDown(KeyCode.E))
            //    SceneManager.LoadScene(nextSceneName);
        //}
    //}
*/