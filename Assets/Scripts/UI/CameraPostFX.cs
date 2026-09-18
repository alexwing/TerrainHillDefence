using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace HillDefence
{
    /// <summary>
    /// Manages camera post-processing effects (Bloom, Color Grading, Vignette, Ambient Occlusion)
    /// using the Post Processing Stack v2 (com.unity.postprocessing).
    /// </summary>
    [RequireComponent(typeof(PostProcessVolume))]
    public class CameraPostFX : MonoBehaviour
    {
        public static CameraPostFX instance;

        [Header("Runtime profile (auto-created if null)")]
        public PostProcessProfile profile;

        private Bloom            _bloom;
        private ColorGrading     _colorGrading;
        private Vignette         _vignette;
        private AmbientOcclusion _ao;

        private float _baseBloomIntensity = 1.0f;
        private bool _pulsing = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoEnsureInstance()
        {
            if (instance == null && FindFirstObjectByType<CameraPostFX>() == null)
            {
                GameObject host = new GameObject("PostProcessVolume");
                host.AddComponent<PostProcessVolume>();
                host.AddComponent<CameraPostFX>();
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Ensure Main Camera has PostProcessLayer
            EnsurePostProcessLayerOnCamera();

            PostProcessVolume vol = GetComponent<PostProcessVolume>();
            vol.isGlobal = true;
            vol.priority = 10;

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            }
            vol.profile = profile;

            SetupEffects();
        }

        private void EnsurePostProcessLayerOnCamera()
        {
            if (Camera.main != null)
            {
                PostProcessLayer layer = Camera.main.GetComponent<PostProcessLayer>();
                if (layer == null)
                {
                    layer = Camera.main.gameObject.AddComponent<PostProcessLayer>();
                }
                layer.volumeLayer = ~0; // Everything
                layer.volumeTrigger = Camera.main.transform;
                layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                Camera.main.allowHDR = true;
            }
        }

        private void SetupEffects()
        {
            // ── Bloom (subtle, fastMode for performance) ──────────────────
            if (!profile.TryGetSettings(out _bloom))
            {
                _bloom = profile.AddSettings<Bloom>();
            }
            _bloom.enabled.value     = true;
            _bloom.intensity.value   = _baseBloomIntensity;
            _bloom.threshold.value   = 1.0f;
            _bloom.diffusion.value   = 5f;
            _bloom.fastMode.value    = true;

            // ── Color Grading (mild warm tint) ───────────────────────────
            if (!profile.TryGetSettings(out _colorGrading))
            {
                _colorGrading = profile.AddSettings<ColorGrading>();
            }
            _colorGrading.enabled.value       = true;
            _colorGrading.gradingMode.value   = GradingMode.LowDefinitionRange;
            _colorGrading.temperature.value   = 10f;
            _colorGrading.saturation.value    = 10f;
            _colorGrading.contrast.value      = 5f;

            // ── Vignette (very light) ────────────────────────────────────
            if (!profile.TryGetSettings(out _vignette))
            {
                _vignette = profile.AddSettings<Vignette>();
            }
            _vignette.enabled.value   = true;
            _vignette.intensity.value = 0.2f;
            _vignette.smoothness.value = 0.5f;
            _vignette.roundness.value  = 1f;

            // ── Ambient Occlusion DISABLED (too expensive) ───────────────
            // _ao is not created to avoid GPU cost
        }

        /// <summary>
        /// Triggers a short bloom flash to emphasize a big explosion.
        /// </summary>
        public void TriggerExplosionBloom()
        {
            if (!_pulsing && _bloom != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(BloomPulse());
            }
        }

        private IEnumerator BloomPulse()
        {
            _pulsing = true;
            float peak     = SceneConfig.FLAG.BloomPulseIntensity;
            float duration = SceneConfig.FLAG.BloomPulseDuration;
            float elapsed  = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float intensity = t < 0.15f
                    ? Mathf.Lerp(_baseBloomIntensity, peak, t / 0.15f)
                    : Mathf.Lerp(peak, _baseBloomIntensity, (t - 0.15f) / 0.85f);

                if (_bloom != null) _bloom.intensity.value = intensity;
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_bloom != null) _bloom.intensity.value = _baseBloomIntensity;
            _pulsing = false;
        }
    }
}
