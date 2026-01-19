using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// Handles fade in/out effects for buff visual effects
    /// </summary>
    public class BuffEffectFade : MonoBehaviour
    {
        private List<Renderer> renderers = new List<Renderer>();
        private List<Color[]> originalColors = new List<Color[]>();
        private Vector3 originalScale;
        private Coroutine currentCoroutine;

        private void Awake()
        {
            CacheRenderers();
            originalScale = transform.localScale;
        }

        private void CacheRenderers()
        {
            renderers.Clear();
            originalColors.Clear();

            // Get all renderers including children
            var allRenderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in allRenderers)
            {
                renderers.Add(renderer);

                // Cache original colors for all materials
                var materials = renderer.materials;
                Color[] colors = new Color[materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    colors[i] = materials[i].HasProperty("_Color")
                        ? materials[i].color
                        : Color.white;
                }
                originalColors.Add(colors);
            }
        }

        /// <summary>
        /// Fade in the effect
        /// </summary>
        public void FadeIn(BuffFadeType fadeType, float duration)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(FadeCoroutine(fadeType, duration, true));
        }

        /// <summary>
        /// Fade out the effect, then destroy
        /// </summary>
        public void FadeOut(BuffFadeType fadeType, float duration)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(FadeCoroutine(fadeType, duration, false));
        }

        private IEnumerator FadeCoroutine(BuffFadeType fadeType, float duration, bool fadeIn)
        {
            float elapsed = 0f;

            // Set initial state
            if (fadeIn)
            {
                SetFadeState(fadeType, 0f);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Ease in/out
                t = fadeIn ? EaseOutQuad(t) : EaseInQuad(t);

                float value = fadeIn ? t : (1f - t);
                SetFadeState(fadeType, value);

                yield return null;
            }

            // Set final state
            SetFadeState(fadeType, fadeIn ? 1f : 0f);

            // Destroy if fading out
            if (!fadeIn)
            {
                Destroy(gameObject);
            }

            currentCoroutine = null;
        }

        private void SetFadeState(BuffFadeType fadeType, float t)
        {
            bool useAlpha = fadeType == BuffFadeType.Alpha || fadeType == BuffFadeType.Both;
            bool useScale = fadeType == BuffFadeType.Scale || fadeType == BuffFadeType.Both;

            if (useAlpha)
            {
                SetAlpha(t);
            }

            if (useScale)
            {
                transform.localScale = originalScale * t;
            }
        }

        private void SetAlpha(float alpha)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                var materials = renderer.materials;

                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j].HasProperty("_Color"))
                    {
                        Color color = originalColors[i][j];
                        color.a = alpha * originalColors[i][j].a;
                        materials[j].color = color;
                    }
                }
            }
        }

        /// <summary>
        /// Restore original state
        /// </summary>
        public void RestoreOriginal()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            transform.localScale = originalScale;

            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                var materials = renderer.materials;

                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j].HasProperty("_Color"))
                    {
                        materials[j].color = originalColors[i][j];
                    }
                }
            }
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInQuad(float t) => t * t;
    }
}
