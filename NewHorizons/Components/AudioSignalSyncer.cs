using System;
using UnityEngine;

namespace NewHorizons.Components
{
    public class AudioSignalSyncer : MonoBehaviour
    {
        public AudioSignal signal;

        private bool _playerInBramble;

        private void Start()
        {
            GlobalMessenger.AddListener("GameUnpaused", OnUnpause);
            GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
            GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", OnEquipSignalscope);
            GlobalMessenger.AddListener("PlayerEnterBrambleDimension", OnPlayerEnterBrambleDimension);
            GlobalMessenger.AddListener("PlayerExitBrambleDimension", OnPlayerExitBrambleDimension);
        }

        private void OnDestroy()
        {
            GlobalMessenger.RemoveListener("GameUnpaused", OnUnpause);
            GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
            GlobalMessenger<Signalscope>.RemoveListener("EquipSignalscope", OnEquipSignalscope);
            GlobalMessenger.RemoveListener("PlayerEnterBrambleDimension", OnPlayerEnterBrambleDimension);
            GlobalMessenger.RemoveListener("PlayerExitBrambleDimension", OnPlayerExitBrambleDimension);
        }

        private void Sync()
        {
            if (signal.IsInsideDarkBramble() == _playerInBramble)
            {
                if (signal.IsOnlyAudibleToScope() && !signal.GetOWAudioSource().isPlaying)
                {
                    signal.GetOWAudioSource().SetLocalVolume(0f);
                    signal.GetOWAudioSource().Play();
                }
                signal.GetOWAudioSource().timeSamples = 0;
            }
        }

        private void OnUnpause()
        {
            bool isPlaying = signal.GetOWAudioSource().isPlaying;
            signal.GetOWAudioSource().Stop();
            if (isPlaying)
            {
                signal.GetOWAudioSource().Play();
                signal.GetOWAudioSource().timeSamples = 0;
            }
        }

        private void OnEndFastForward() => OnUnpause();

        private void OnEquipSignalscope(Signalscope scope) => Sync();

        private void OnPlayerEnterBrambleDimension()
        {
            _playerInBramble = true;
            if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.SignalScope))
            {
                Sync();
            }
        }

        private void OnPlayerExitBrambleDimension()
        {
            _playerInBramble = false;
            if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.SignalScope))
            {
                Sync();
            }
        }
    }
}
