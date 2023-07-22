using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class NHShuttleController : NomaiShuttleController
    {
        public NomaiInterfaceSlot _neutralSlot;
        public Sector _exteriorSector;
        public Sector _interiorSector;

        public new void Awake()
        {
            Locator.RegisterNomaiShuttle(this);
            _warpEffect = GetComponentInChildren<SingularityWarpEffect>();
            _triggerVolume.OnEntry += OnEntry;
            _triggerVolume.OnExit += OnExit;
            _beamResetVolume.OnExit += OnExitBeamReset;
            _launchSlot.OnSlotActivated += OnLaunchSlotActivated;
            _launchSlot.OnSlotDeactivated += OnLaunchSlotDeactivated;
            _retrieveSlot.OnSlotActivated += OnRetrieveSlotActivated;
            _retrieveSlot.OnSlotDeactivated += OnRetrieveSlotDeactivated;
            _landSlot.OnSlotActivated += OnLandSlotActivated;
            _landSlot.OnSlotDeactivated += OnLandSlotDeactivated;
            _impactSensor.OnImpact += OnImpact;
            _landingBeamRoot.SetActive(value: false);
            if (_id == ShuttleID.BrittleHollowShuttle)
            {
                _exteriorRendererObj.SetActive(value: false);
                GlobalMessenger.AddListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
                GlobalMessenger.AddListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
            }
            _neutralSlot.OnSlotActivated += OnNeutralSlotActivated;
            _neutralSlot.OnSlotDeactivated += OnNeutralSlotDeactivated;
        }

        public new void Start()
        {
            _cannon = Locator.GetGravityCannon(_id);
            if (_cannon == null)
            {
                NHLogger.LogError("Failed to locate gravity cannon " + _id);
            }
            _shuttleBody.Suspend();
            _forceApplier = _detectorObj.GetComponent<ForceApplier>();
            enabled = false;
        }

        private void OnNeutralSlotActivated(NomaiInterfaceSlot slot)
        {
            NHLogger.LogVerbose("Neutral Slot Activated");
        }

        private void OnNeutralSlotDeactivated(NomaiInterfaceSlot slot)
        {
            NHLogger.LogVerbose("Neutral Slot Deactivated");
        }

        private new void OnDestroy()
        {
            _triggerVolume.OnEntry -= OnEntry;
            _triggerVolume.OnExit -= OnExit;
            _beamResetVolume.OnExit += OnExitBeamReset;
            _launchSlot.OnSlotActivated -= OnLaunchSlotActivated;
            _launchSlot.OnSlotDeactivated -= OnLaunchSlotDeactivated;
            _retrieveSlot.OnSlotActivated -= OnRetrieveSlotActivated;
            _retrieveSlot.OnSlotDeactivated -= OnRetrieveSlotDeactivated;
            _landSlot.OnSlotActivated -= OnLandSlotActivated;
            _landSlot.OnSlotDeactivated -= OnLandSlotDeactivated;
            _impactSensor.OnImpact -= OnImpact;
            Locator.UnregisterNomaiShuttle(this);
            if (_id == ShuttleID.BrittleHollowShuttle)
            {
                GlobalMessenger.RemoveListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
                GlobalMessenger.RemoveListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
            }
            _neutralSlot.OnSlotActivated -= OnNeutralSlotActivated;
            _neutralSlot.OnSlotDeactivated -= OnNeutralSlotDeactivated;
        }

        public new void Retrieve()
        {
            if (!_isRetrieving && _cannon != null && _cannon.AllowShuttleRetrieval(transform.position))
            {
                if (_id == ShuttleID.BrittleHollowShuttle && _shuttleBody.IsSuspended())
                {
                    _exteriorRendererObj.SetActive(value: true);
                    GlobalMessenger.RemoveListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
                    GlobalMessenger.RemoveListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
                }
                _cannon.SetGravityActivation(activate: false);
                if (_isPlayerInside)
                {
                    _retrievingWithPlayer = true;
                    _warpEffect.singularityController.OnCreation += StartReposition;
                    _warpEffect.singularityController.Create();
                    _cannon.PlayRecallEffect(_retrievalLength, playerInsideShuttle: true);
                }
                else
                {
                    _retrievingWithPlayer = false;
                    _warpEffect.OnWarpComplete += StartReposition;
                    _warpEffect.WarpObjectOut(_retrievalLength);
                    _cannon.PlayRecallEffect(_retrievalLength, playerInsideShuttle: false);
                }
                _orb.AddLock();
                _isRetrieving = true;
                _allowLanding = false;
            }
        }

        public new void OnImmobilizationComplete()
        {
            _forceApplier.SetApplyFluids(applyFluids: true);
            _forceApplier.SetApplyForces(applyForces: true);
        }

        private new void StartReposition()
        {
            if (_shuttleBody.IsSuspended())
            {
                UnsuspendShuttle();
            }
            _framesToReposition = 2;
            base.enabled = true;
            if (_retrievingWithPlayer)
            {
                _warpEffect.singularityController.OnCreation -= StartReposition;
                _warpEffect.singularityController.CollapseImmediate();
                _cannon.PlayEndOfRecallEffect();
            }
            else
            {
                _warpEffect.OnWarpComplete -= StartReposition;
            }
        }

        private new void CompleteReposition()
        {
            if (_cannon != null)
            {
                _forceApplier.SetApplyFluids(applyFluids: false);
                _forceApplier.SetApplyForces(applyForces: false);
                _cannon.MoveShuttleToSocket(_shuttleBody);
                if (_isLanding)
                {
                    StopLanding();
                }
            }
            for (int i = 0; i < _exteriorLegColliders.Length; i++)
            {
                _exteriorLegColliders[i].SetActivation(active: true);
            }
            _orb.RemoveLock();
            if (_tractorBeam != null && !_isPlayerInside)
            {
                _tractorBeam.SetActivation(active: true);
            }
            _isRetrieving = false;
        }

        private new void UnsuspendShuttle()
        {
            _shuttleBody.Unsuspend(restoreCachedVelocity: false);
            _shuttleBody.transform.parent = null;
            _shuttleBody.transform.position = base.transform.position;
            _shuttleBody.transform.rotation = base.transform.rotation;
            base.transform.parent = _shuttleBody.transform;
            base.transform.localPosition = Vector3.zero;
            base.transform.localRotation = Quaternion.identity;
            EffectVolume[] componentsInChildren = GetComponentsInChildren<EffectVolume>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].ResetAttachedBody();
            }
            _orb.SetParentBody(_shuttleBody);
            _orb.GetComponent<ConstantForceDetector>().AddConstantVolume(_forceVolume, inheritForceAcceleration: true, clearOtherFields: true);
            if (_isPlayerInside)
            {
                DynamicForceDetector component = Locator.GetPlayerDetector().GetComponent<DynamicForceDetector>();
                component.RemoveVolume(_forceVolume);
                component.AddVolume(_forceVolume);
            }
            OWCollider[] componentsInChildren2 = _exteriorColliderRoot.GetComponentsInChildren<OWCollider>();
            for (int j = 0; j < componentsInChildren2.Length; j++)
            {
                _exteriorCollisionGroup.RemoveCollider(componentsInChildren2[j]);
            }
            for (int k = 0; k < _exteriorLegColliders.Length; k++)
            {
                _exteriorLegColliders[k].SetActivation(active: false);
            }
        }

        private new void AttemptLanding()
        {
            PlanetoidRuleset[] array = GameObject.FindObjectsOfType<PlanetoidRuleset>();
            PlanetoidRuleset planetoidRuleset = null;
            float num = float.PositiveInfinity;
            for (int i = 0; i < array.Length; i++)
            {
                float num2 = Vector3.Distance(array[i].transform.position, base.transform.position) - array[i].GetShuttleLandingRadius();
                if (num2 < num)
                {
                    num = num2;
                    planetoidRuleset = array[i];
                }
            }
            if (planetoidRuleset != null && num < 1000f)
            {
                _targetPlanetoid = planetoidRuleset;
                _isLanding = true;
                _landSlot.SetAttractive(attractive: true);
                _landingBeamRoot.SetActive(value: true);
                base.enabled = true;
                NHLogger.LogVerbose("LANDING SEQUENCE: Distance to " + _targetPlanetoid.name + ": " + num);
            }
        }

        private new void StopLanding()
        {
            _isLanding = false;
            _targetPlanetoid = null;
            _landingBeamRoot.SetActive(value: false);
        }

        private new void FixedUpdate()
        {
            if (_isLanding)
            {
                Vector3 toDirection = base.transform.position - _targetPlanetoid.transform.position;
                float rt = toDirection.magnitude - _targetPlanetoid.GetShuttleLandingRadius();
                if (_targetPlanetoid == null || rt > 2000f)
                {
                    StopLanding();
                    return;
                }
                Vector3 vector = OWPhysics.FromToAngularVelocity(base.transform.up, toDirection);
                _shuttleBody.SetAngularVelocity(Vector3.zero);
                _shuttleBody.AddAngularVelocityChange(vector * 0.01f);
                OWRigidbody attachedOWRigidbody = _targetPlanetoid.GetAttachedOWRigidbody();
                float approachSpeed = Mathf.Lerp(10f, 1f, Mathf.InverseLerp(100f, 0f, rt));
                Vector3 deltaVelocity = attachedOWRigidbody.GetVelocity() - toDirection.normalized * approachSpeed - _shuttleBody.GetVelocity();
                NHLogger.LogVerbose("approach speed: " + approachSpeed + "   delta velocity: " + deltaVelocity.magnitude);
                Vector3 velocityChange = deltaVelocity * Time.deltaTime * 1f;
                _shuttleBody.AddVelocityChange(velocityChange);
                if (_isPlayerInside) Locator.GetPlayerBody().AddVelocityChange(velocityChange);
            }
            if (_framesToReposition > 0)
            {
                _framesToReposition--;
                if (_framesToReposition == 0) CompleteReposition();
            }
            if (!_isLanding && _framesToReposition == 0)
            {
                base.enabled = false;
            }
        }

        private new void OnImpact(ImpactData impact)
        {
            if (impact.otherBody.GetMass() > 100f && _isLanding)
            {
                NHLogger.LogVerbose("Shuttle impact with " + impact.otherCollider.transform.GetPath());
                StopLanding();
            }
        }

        private new void OnEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                _isPlayerInside = true;
                GlobalMessenger.FireEvent("EnterShuttle");
                _tractorBeam.SetActivation(active: false);
            }
        }

        private new void OnExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                _isPlayerInside = false;
                GlobalMessenger.FireEvent("ExitShuttle");
            }
        }

        private new void OnExitBeamReset(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector") && !_isPlayerInside && !_tractorBeam.IsActive())
            {
                _tractorBeam.SetActivation(active: true);
            }
        }

        private new void OnLaunchSlotActivated(NomaiInterfaceSlot slot)
        {
            NHLogger.LogVerbose("Launch Slot Activated");
            if (_cannon != null) _cannon.SetGravityActivation(activate: true);
            else NHLogger.LogError("Cannot launch. No gravity cannon found.");
            _allowLanding = true;
        }

        private new void OnLaunchSlotDeactivated(NomaiInterfaceSlot slot)
        {
            NHLogger.LogVerbose("Launch Slot Deactivated");
            if (_cannon != null) _cannon.SetGravityActivation(activate: false);
        }

        private new void OnRetrieveSlotActivated(NomaiInterfaceSlot slot)
        {
            NHLogger.LogVerbose("Retrieve Slot Activated");
            Retrieve();
        }

        private void OnRetrieveSlotDeactivated(NomaiInterfaceSlot slot)
        {
            NHLogger.LogVerbose("Retrieve Slot Deactivated");
        }

        private new void OnLandSlotActivated(NomaiInterfaceSlot slot)
        {
            NHLogger.LogVerbose("Land Slot Activated");
            if (_allowLanding && !_shuttleBody.IsSuspended())
            {
                AttemptLanding();
            }
        }

        private new void OnLandSlotDeactivated(NomaiInterfaceSlot slot)
        {
            NHLogger.LogWarning("Land Slot Deactivated");
            if (_isLanding)
            {
                StopLanding();
                _landSlot.SetAttractive(attractive: false);
            }
        }

        private new void OnPlayerEnterQuantumMoon()
        {
            _exteriorRendererObj.SetActive(value: true);
        }

        private new void OnPlayerExitQuantumMoon()
        {
            _exteriorRendererObj.SetActive(value: false);
        }
    }
}
