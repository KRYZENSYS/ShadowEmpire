// =============================================================================
// DayNightCycle.cs
// =============================================================================
// PURPOSE:
//   Real-time day/night cycle. Drives sun angle, skybox blend, exposure,
//   fog density, and ambient light. Plays well with HDRP volumes.
//
// ENGINE: Unity 6, HDRP
// =============================================================================

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ShadowEmpire.World
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Cycle")]
        public float dayLengthMinutes = 24f;
        public float startHour = 12f;
        public Light sun;
        public Light moon;
        public Volume hdrpVolume;

        [Header("Sky")]
        public Gradient ambientSky;
        public Gradient ambientEquator;
        public Gradient ambientGround;
        public Gradient fogColor;
        public AnimationCurve fogDensity;

        private float _hour;
        private float _secondsPerHour;

        private void Awake()
        {
            _hour = startHour;
            _secondsPerHour = (dayLengthMinutes * 60f) / 24f;
        }

        private void Update()
        {
            _hour = (_hour + Time.deltaTime / _secondsPerHour) % 24f;
            UpdateSunMoon();
            UpdateEnvironment();
        }

        private void UpdateSunMoon()
        {
            float angle = (_hour / 24f) * 360f - 90f;
            float rad = angle * Mathf.Deg2Rad;
            // Sun
            sun.transform.localRotation = Quaternion.Euler(angle, 30f, 0f);
            sun.color = sun.isActiveAndEnabled ? Color.Lerp(Color.black, Color.white, Mathf.Clamp01(Mathf.Cos(rad) * 0.5f + 0.5f)) : Color.white;
            // Moon (opposite side)
            moon.transform.localRotation = Quaternion.Euler(angle + 180f, 30f, 0f);
        }

        private void UpdateEnvironment()
        {
            float t = _hour / 24f;
            RenderSettings.ambientSkyColor    = ambientSky.Evaluate(t);
            RenderSettings.ambientEquatorColor = ambientEquator.Evaluate(t);
            RenderSettings.ambientGroundColor = ambientGround.Evaluate(t);
            RenderSettings.fogColor = fogColor.Evaluate(t);
            RenderSettings.fogDensity = fogDensity.Evaluate(t);

            // HDRP exposure
            if (hdrpVolume != null && hdrpVolume.profile.TryGet<Exposure>(out var ex))
                ex.fixedExposure.value = Mathf.Lerp(8f, 14f, Mathf.Cos(t * 360f * Mathf.Deg2Rad) * 0.5f + 0.5f);
        }

        public float GetHour() => _hour;
        public bool IsNight() => _hour < 6f || _hour > 19f;
    }
}