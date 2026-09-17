using System.Collections;
using UnityEngine;

namespace HillDefence
{
    /// <summary>
    /// Plays an expanded fire-wave effect when a flag base is destroyed.
    /// Spawned by TargetTerrain.DetonationTerrain() for large explosions.
    /// </summary>
    public class FlagExplosionFX : MonoBehaviour
    {
        [Tooltip("Fire/explosion prefab (same as TargetTerrain.detonationPrefab).")]
        public GameObject firePrefab;

        [Tooltip("Large shockwave prefab (same as TargetTerrain.detonationTowerPrefab).")]
        public GameObject shockwavePrefab;

        /// <summary>
        /// Launch the full big-base explosion sequence.
        /// </summary>
        public void Play(Vector3 center, float baseScale, GameObject firePref, GameObject shockPref)
        {
            firePrefab = firePref;
            shockwavePrefab = shockPref;
            transform.position = center;
            StartCoroutine(ExplosionSequence(center, baseScale));
        }

        private IEnumerator ExplosionSequence(Vector3 center, float baseScale)
        {
            int   waveCount   = SceneConfig.FLAG.ExplosionWaveCount;
            float duration    = SceneConfig.FLAG.ExplosionWaveDuration;
            float maxScale    = SceneConfig.FLAG.ExplosionMaxScale * baseScale * 0.12f;
            float maxRadius   = baseScale * 1.8f;
            float interval    = duration / waveCount;
            float shakeMag    = SceneConfig.FLAG.CameraShakeMagnitude;
            float shakeDur    = SceneConfig.FLAG.CameraShakeDuration;

            // 1 — Immediate central shockwave
            if (shockwavePrefab != null)
            {
                float sw = baseScale * 1.4f;
                GameObject shock = Instantiate(shockwavePrefab, center, Quaternion.identity);
                shock.transform.localScale = new Vector3(sw, sw, sw);
                Destroy(shock, SceneConfig.TERRAIN.explosionLife);
            }

            // 2 — Screen shake
            StartCoroutine(CameraShake(shakeMag, shakeDur));

            // 3 — Notify post-FX for bloom pulse
            if (CameraPostFX.instance != null)
                CameraPostFX.instance.TriggerExplosionBloom();

            // 4 — Expanding fire ring
            for (int i = 0; i < waveCount; i++)
            {
                float t = (float)i / (waveCount - 1);           // 0 → 1
                float radius   = Mathf.Lerp(0f, maxRadius, t);
                float scale    = Mathf.Sin(t * Mathf.PI) * maxScale; // grow then shrink
                scale = Mathf.Max(scale, maxScale * 0.15f);

                // Evenly distribute fireballs around the ring
                for (int j = 0; j < 6; j++)
                {
                    float angle = (j / 6f + t * 0.3f) * Mathf.PI * 2f; // slow spin
                    float ox = Mathf.Cos(angle) * radius;
                    float oz = Mathf.Sin(angle) * radius;
                    Vector3 spawnPos = center + new Vector3(ox, 0, oz);

                    // Snap to terrain height
                    if (Terrain.activeTerrain != null)
                        spawnPos.y = Terrain.activeTerrain.SampleHeight(spawnPos);

                    if (firePrefab != null)
                    {
                        GameObject fx = Instantiate(firePrefab, spawnPos, Quaternion.identity);
                        fx.transform.localScale = Vector3.one * scale;
                        // Punch-scale then destroy
                        StartCoroutine(PunchScale(fx.transform, scale, SceneConfig.TERRAIN.explosionLife));
                    }
                }

                yield return new WaitForSeconds(interval);
            }

            Destroy(gameObject, SceneConfig.TERRAIN.explosionLife + 1f);
        }

        private IEnumerator PunchScale(Transform t, float peakScale, float lifetime)
        {
            float elapsed = 0f;
            while (elapsed < lifetime && t != null)
            {
                float s = Mathf.Lerp(peakScale, 0f, elapsed / lifetime);
                if (t != null) t.localScale = Vector3.one * s;
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (t != null) Destroy(t.gameObject);
        }

        private IEnumerator CameraShake(float magnitude, float duration)
        {
            if (Camera.main == null) yield break;
            Transform camT = Camera.main.transform;
            Vector3 originalPos = camT.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);
                camT.localPosition = originalPos + Random.insideUnitSphere * strength;
                elapsed += Time.deltaTime;
                yield return null;
            }
            camT.localPosition = originalPos;
        }
    }
}
