using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace HillDefence
{
    /// <summary>
    /// Manages camera post-processing effects (Bloom, Color Grading, Vignette, etc.)
    /// using the Post Processing Stack v2 (com.unity.postprocessing).
    ///
    /// SETUP (do once in Unity Editor after adding this script):
    ///   1. On the Main Camera, add Component -> Rendering -> Post Process Layer
    ///      Set "Layer" to "Everything" (or a dedicated PP layer).
    ///   2. Create an empty GameObject "PostProcessVolume", add Component ->
    ///      Rendering -> Post Process Volume, tick "Is Global", assign the
    ///      PostProcessProfile that this script creates at runtime (or let this
    ///      script create it automatically by leaving the Profile field empty and
    ///      assigning the Volume reference).
    ///   3. Assign the PostProcessVolume reference to this component in the Inspector.
    /// </summary>
    [RequireComponent(typeof(PostProcessVolume))]
    public class CameraPostFX : MonoBehaviour
    {
        public static CameraPostFX instance;

        [Header("Runtime profile (auto-created if null)")]
        public PostProcessProfile profile;

        // Cached effect settings
        private Bloom          _bloom;
        private ColorGrading   _colorGrading;
        private Vignette       _vignette;
        private AmbientOcclusion _ao;

        // Baseline bloom intensity
        private float _baseBloomIntensity = 0.8f;
        private bool _pulsing = false;

        void Awake()
        {
            if (instance == null) instance = this;
            else { Destroy(gameObject); return; }

            PostProcessVolume vol = GetComponent<PostProcessVolume>();
            vol.isGlobal = true;
            vol.priority = 10;

            // Create or reuse profile
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PostProcessProfile>();
                vol.profile = profile;
            }
            else
            {
                vol.profile = profile;
            }

            SetupEffects();
        }

        private void SetupEffects()
        {
            // ── Bloom ────────────────────────────────────────────────────
            _bloom = profile.AddSettings<Bloom>();
            _bloom.enabled.value     = true;
            _bloom.intensity.value   = _baseBloomIntensity;
            _bloom.threshold.value   = 0.9f;
            _bloom.diffusion.value   = 6f;
            _bloom.fastMode.value    = false;

            // ── Color Grading (warm, slightly cinematic) ─────────────────
            _colorGrading = profile.AddSettings<ColorGrading>();
            _colorGrading.enabled.value       = true;
            _colorGrading.gradingMode.value   = GradingMode.HighDefinitionRange;
            _colorGrading.temperature.value   = 12f;   // warm orange tint
            _colorGrading.saturation.value    = 15f;   // more vivid
            _colorGrading.contrast.value      = 8f;
            _colorGrading.gamma.value         = new Vector4(1f, 0.97f, 0.92f, 0f); // slight warm gamma

            // ── Vignette ────────────────────────────────────────────────
            _vignette = profile.AddSettings<Vignette>();
            _vignette.enabled.value   = true;
            _vignette.intensity.value = 0.28f;
            _vignette.smoothness.value = 0.65f;
            _vignette.roundness.value  = 0.9f;

            // ── Ambient Occlusion ────────────────────────────────────────
            _ao = profile.AddSettings<AmbientOcclusion>();
            _ao.enabled.value   = true;
            _ao.intensity.value = 0.7f;
            _ao.radius.value    = 0.4f;
        }

        /// <summary>
        /// Triggers a short bloom flash to emphasize a big explosion.
        /// Called by FlagExplosionFX.
        /// </summary>
        public void TriggerExplosionBloom()
        {
            if (!_pulsing && _bloom != null)
                StartCoroutine(BloomPulse());
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
                // Quick rise then slow fall
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
