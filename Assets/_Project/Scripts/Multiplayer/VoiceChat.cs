// =============================================================================
// VoiceChat.cs
// =============================================================================
// PURPOSE:
//   Voice chat scaffolding. Drop in Vivox / Dissonance / Photon Voice.
//   This template uses Unity's Microphone API + Opus encoder stub so you can
//   run without a 3rd-party SDK and add a real one later.
//
// ENGINE: Unity 6
// =============================================================================

using System.Collections;
using UnityEngine;

namespace ShadowEmpire.Multiplayer
{
    public class VoiceChat : MonoBehaviour
    {
        public enum State { Idle, Listening, Transmitting }

        [Header("Recording")]
        public int sampleRate = 16000;
        public int frameSize = 400; // 25ms at 16kHz

        public State CurrentState { get; private set; } = State.Idle;

        private AudioClip _micClip;
        private string _micDevice;

        public void StartVoice()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("[Voice] No microphone found.");
                return;
            }
            _micDevice = Microphone.devices[0];
            _micClip = Microphone.Start(_micDevice, true, 1, sampleRate);
            CurrentState = State.Listening;
            StartCoroutine(StreamLoop());
            Debug.Log($"[Voice] Listening on {_micDevice}");
        }

        public void StopVoice()
        {
            if (CurrentState == State.Idle) return;
            Microphone.End(_micDevice);
            StopAllCoroutines();
            CurrentState = State.Idle;
        }

        private IEnumerator StreamLoop()
        {
            var wait = new WaitForSecondsRealtime(frameSize / 1000f);
            while (CurrentState != State.Idle)
            {
                int pos = Microphone.GetPosition(_micDevice) - frameSize;
                if (pos < 0) { yield return wait; continue; }
                var data = new float[frameSize];
                _micClip.GetData(data, pos);
                // TODO: encode (Opus) and send via Netcode RPC / Vivox SDK
                yield return wait;
            }
        }
    }
}