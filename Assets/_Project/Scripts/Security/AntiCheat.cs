// =============================================================================
// AntiCheat.cs
// =============================================================================
// PURPOSE:
//   Lightweight server-side validation hooks. Real games do all of this on
//   the dedicated server; this client-side guard is a first line of defense.
//
// ENGINE: Unity 6
// =============================================================================

using UnityEngine;

namespace ShadowEmpire.Security
{
    public class AntiCheat : MonoBehaviour
    {
        public static AntiCheat Instance { get; private set; }

        [Header("Limits")]
        public float maxSpeed = 60f;
        public float maxDamagePerSecond = 500f;
        public float maxCurrencyPerMinute = 100000f;

        private float _damageWindow;
        private float _currencyWindow;
        private float _lastWindowReset;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Time.time - _lastWindowReset > 60f)
            {
                _damageWindow = 0; _currencyWindow = 0;
                _lastWindowReset = Time.time;
            }
        }

        public bool ValidateDamage(float amount)
        {
            _damageWindow += amount;
            if (_damageWindow > maxDamagePerSecond * 60f)
            {
                ReportViolation($"damage overflow: {_damageWindow}");
                return false;
            }
            return true;
        }

        public bool ValidateCurrencyGain(float amount)
        {
            _currencyWindow += amount;
            if (_currencyWindow > maxCurrencyPerMinute)
            {
                ReportViolation($"currency overflow: {_currencyWindow}");
                return false;
            }
            return true;
        }

        public bool ValidateSpeed(Vector3 delta, float dt)
        {
            if (dt <= 0) return true;
            float speed = delta.magnitude / dt;
            if (speed > maxSpeed)
            {
                ReportViolation($"speed hack: {speed}");
                return false;
            }
            return true;
        }

        private void ReportViolation(string reason)
        {
            Debug.LogWarning($"[AntiCheat] VIOLATION: {reason}");
            // TODO: server-side telemetry
        }
    }
}