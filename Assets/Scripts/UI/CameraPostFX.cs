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

        private float _baseBloomIntensity = 0.8f;
        private bool _pulsing = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoEnsureInstance()
        {
            if (instance == null && FindObjectOfType<CameraPostFX>() == null)
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
            }
        }

        private void SetupEffects()
        {
            // ── Bloom ────────────────────────────────────────────────────
            if (!profile.TryGetSettings(out _bloom))
            {
                _bloom = profile.AddSettings<Bloom>();
            }
            _bloom.enabled.value     = true;
            _bloom.intensity.value   = _baseBloomIntensity;
            _bloom.threshold.value   = 0.9f;
            _bloom.diffusion.value   = 6f;
            _bloom.fastMode.value    = false;

            // ── Color Grading (warm, slightly cinematic) ─────────────────
            if (!profile.TryGetSettings(out _colorGrading))
            {
                _colorGrading = profile.AddSettings<ColorGrading>();
            }
            _colorGrading.enabled.value       = true;
            _colorGrading.gradingMode.value   = GradingMode.HighDefinitionRange;
            _colorGrading.temperature.value   = 12f;   // warm orange tint
            _colorGrading.saturation.value    = 15f;   // vivid colors
            _colorGrading.contrast.value      = 8f;
            _colorGrading.gamma.value         = new Vector4(1f, 0.97f, 0.92f, 0f);

            // ── Vignette ────────────────────────────────────────────────
            if (!profile.TryGetSettings(out _vignette))
            {
                _vignette = profile.AddSettings<Vignette>();
            }
            _vignette.enabled.value   = true;
            _vignette.intensity.value = 0.28f;
            _vignette.smoothness.value = 0.65f;
            _vignette.roundness.value  = 0.9f;

            // ── Ambient Occlusion ────────────────────────────────────────
            if (!profile.TryGetSettings(out _ao))
            {
                _ao = profile.AddSettings<AmbientOcclusion>();
            }
            _ao.enabled.value   = true;
            _ao.intensity.value = 0.7f;
            _ao.radius.value    = 0.4f;
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
