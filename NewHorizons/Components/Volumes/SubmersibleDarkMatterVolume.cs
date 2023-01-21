using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class SubmersibleDarkMatterVolume : DarkMatterVolume
    {
        public bool _activeWhenSubmerged;

        [Space]
        public EffectVolume[] _effectVolumes = new EffectVolume[0];

        public OWRenderer[] _renderers = new OWRenderer[0];

        [Header("Sensors")]
        public DynamicFluidDetector _fluidDetector;

        private bool _active = true;

        private bool _submerged;

        private void FixedUpdate()
        {
            _submerged = _fluidDetector.InFluidType(FluidVolume.Type.WATER);
            UpdateActivation();
        }

        private void UpdateActivation()
        {
            bool flag = _submerged == _activeWhenSubmerged;
            if (_active != flag)
            {
                _active = flag;

                foreach (EffectVolume effectVolume in _effectVolumes)
                    effectVolume.SetVolumeActivation(flag);

                foreach (OWRenderer renderer in _renderers)
                    renderer.SetActivation(flag);
            }
        }
    }
}
