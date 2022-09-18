using UnityEngine;
using NewHorizons.Components.SizeControllers;

namespace NewHorizons.Components.Stars
{
    [RequireComponent(typeof(OWAudioSource))]
    public class StarSurfaceAudioController : SectoredMonoBehaviour
    {
        [SerializeField]
        private StarEvolutionController _starEvolutionController;
        private OWAudioSource _audioSource;
        private float _fade;
        public float _size;

        public void Start()
        {
            _audioSource = this.GetRequiredComponent<OWAudioSource>();
            _audioSource.SetLocalVolume(0);
            enabled = false;
        }

        public override void OnSectorOccupantsUpdated()
        {
            bool wasEnabled = enabled;
            enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
            if (enabled && !wasEnabled) _audioSource.Play();
            else if (!enabled && wasEnabled)
            {
                _fade = 0;
                _audioSource.Stop();
            }
        }

        public void Update()
        {
            _fade = Mathf.MoveTowards(_fade, 1, Time.deltaTime * 0.2f);
            float value = Mathf.Max(0.0f, Vector3.Distance(Locator.GetPlayerCamera().transform.position, transform.position) - (_starEvolutionController != null ? _starEvolutionController.CurrentScale : _size));
            float num = Mathf.InverseLerp(1600f, 100f, value);
            _audioSource.SetLocalVolume(num * num * _fade);
        }

        public void SetStarEvolutionController(StarEvolutionController controller) => _starEvolutionController = controller;
    }
}