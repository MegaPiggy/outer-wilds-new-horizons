using NewHorizons.Handlers;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    [RequireComponent(typeof(OWTriggerVolume))]
    public class NotificationVolume : MonoBehaviour
    {
        private NotificationTarget _target = NotificationTarget.All;
        private bool _pin = false;
        private OWTriggerVolume _triggerVolume;
        private NotificationData _entryNotification;
        private NotificationData _exitNotification;

        public void Awake()
        {
            _triggerVolume = this.GetRequiredComponent<OWTriggerVolume>();
            _triggerVolume.OnEntry += OnTriggerVolumeEntry;
            _triggerVolume.OnExit += OnTriggerVolumeExit;
        }

        public void OnDestroy()
        {
            if (_triggerVolume == null) return;
            _triggerVolume.OnEntry -= OnTriggerVolumeEntry;
            _triggerVolume.OnExit -= OnTriggerVolumeExit;
        }

        public void SetPinned(bool pin) => _pin = pin;

        public void SetTarget(External.Modules.VolumesModule.NotificationVolumeInfo.NotificationTarget target) => SetTarget(EnumUtils.Parse<NotificationTarget>(target.ToString(), NotificationTarget.All));

        public void SetTarget(NotificationTarget target) => _target = target;

        public void SetEntryNotification(string displayMessage, float duration = 5)
        {
            _entryNotification = new NotificationData(_target, TranslationHandler.GetTranslation(displayMessage, TranslationHandler.TextType.UI), duration);
        }

        public void SetExitNotification(string displayMessage, float duration = 5)
        {
            _exitNotification = new NotificationData(_target, TranslationHandler.GetTranslation(displayMessage, TranslationHandler.TextType.UI), duration);
        }

        public void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (_target == NotificationTarget.All)
            {
                if (hitObj.CompareTag("PlayerDetector") || hitObj.CompareTag("ShipDetector"))
                {
                    PostEntryNotification();
                }
            }
            else if (_target == NotificationTarget.Player)
            {
                if (hitObj.CompareTag("PlayerDetector"))
                {
                    PostEntryNotification();
                }
            }
            else if (_target == NotificationTarget.Ship)
            {
                if (hitObj.CompareTag("ShipDetector"))
                {
                    PostEntryNotification();
                }
            }
        }

        public void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (_target == NotificationTarget.All)
            {
                if (hitObj.CompareTag("PlayerDetector") || hitObj.CompareTag("ShipDetector"))
                {
                    PostExitNotification();
                }
            }
            else if (_target == NotificationTarget.Player)
            {
                if (hitObj.CompareTag("PlayerDetector"))
                {
                    PostExitNotification();
                }
            }
            else if (_target == NotificationTarget.Ship)
            {
                if (hitObj.CompareTag("ShipDetector"))
                {
                    PostExitNotification();
                }
            }
        }

        public void PostEntryNotification()
        {
            if (_entryNotification == null) return;
            NotificationManager.SharedInstance.PostNotification(_entryNotification, _pin);
        }

        public void PostExitNotification()
        {
            if (_exitNotification == null) return;
            NotificationManager.SharedInstance.PostNotification(_exitNotification, _pin);
        }

        public void UnpinEntryNotification()
        {
            if (_entryNotification == null) return;
            if (NotificationManager.SharedInstance.IsPinnedNotification(_entryNotification))
            {
                NotificationManager.SharedInstance.UnpinNotification(_entryNotification);
            }
        }

        public void UnpinExitNotification()
        {
            if (_exitNotification == null) return;
            if (NotificationManager.SharedInstance.IsPinnedNotification(_exitNotification))
            {
                NotificationManager.SharedInstance.UnpinNotification(_exitNotification);
            }
        }
    }
}
