using UnityEngine;
using ClocknestGames.Library.Editor;
using ClocknestGames.Library.Utils;
using Dreamteck.Splines;
using CSGManager;
using EzySlice;
using Cinemachine;
using System.Collections.Generic;
using DG.Tweening;

namespace ClocknestGames.Game.Core
{
    [System.Serializable]
    public enum SpatulaStatus
    {
        Flipping,
        Scraping,
        Stuck
    }

    public enum SpatulaBehaviour
    {
        None,
        TargetAngle
    }

    [System.Serializable]
    public enum SpatulaCut
    {
        Horizontal,
        Vertical
    }

    public enum SpatulaEventType
    {
        Flipped,
        Stuck,
        Sliced,
        ScrapeStarted,
        ScrapeEnded
    }

    public struct SpatulaEvent
    {
        public SpatulaEventType EventType;

        public SpatulaEvent(SpatulaEventType eventType)
        {
            EventType = eventType;
        }
    }

    public class RigidbodyStorage
    {
        protected Rigidbody _rigidbody;

        protected Vector3 _velocity;
        protected Vector3 _angularVelocity;
        protected Vector3 _position;
        protected Quaternion _rotation;
        protected Vector3 _intertiaTensor;
        protected Quaternion _inertiaTensorRotation;

        public RigidbodyStorage(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
        }

        public virtual void Update()
        {
            _velocity = _rigidbody.velocity;
            _angularVelocity = _rigidbody.angularVelocity;
            _position = _rigidbody.position;
            _rotation = _rigidbody.rotation;
            _intertiaTensor = _rigidbody.inertiaTensor;
            _inertiaTensorRotation = _rigidbody.inertiaTensorRotation;
        }

        public virtual void Restore()
        {
            _rigidbody.velocity = _velocity;
            _rigidbody.angularVelocity = _angularVelocity;
            // _rigidbody.position = _position;
            // _rigidbody.rotation = _rotation;
            // _rigidbody.inertiaTensor = _intertiaTensor;
            // _rigidbody.inertiaTensorRotation = _inertiaTensorRotation;
        }
    }

    [RequireComponent(typeof(Rigidbody))]
    public class Spatula : Singleton<Spatula>, EventListener<TouchFirstTapEvent>
                                            , EventListener<LevelEvent>
                                            , EventListener<StateChangeEvent<SpatulaBehaviour>>
    {
        [Header("General")]
        [SerializeField] protected LayerMask _killerLayerMask;
        [SerializeField] protected LayerMask _slicableLayerMask;
        [SerializeField] protected SpatulaTrigger _spatulaTrigger;
        [SerializeField] protected float _maxAngularVelocity = 50f;

        [Header("Stick")]
        [SerializeField] protected LayerMask _stickLayerMask;
        [SerializeField] protected float _minAngleToStick = -120f;

        [Header("Flip")]
        [SerializeField] protected Vector2YZ _force = new Vector2YZ(30f, 15f);
        [SerializeField] protected Vector2 _torque = new Vector2(10, 10f);
        [SerializeField] protected float _flipBalanceDelay = .5f;
        [SerializeField] protected float _velocityApproach = 10f;
        [SerializeField] public float ArcFastTime = .25f;
        [SerializeField] public float ArcLongTime = 2f;
        [SerializeField] public MinMaxValue ArcLongAngles = new MinMaxValue(120f, 160f);
        /*
        [Header("Flip-Easing")]
        [SerializeField] protected float _velocityApproach = 2f;
        [SerializeField] protected float _minAngVelocity = 5f;
        [SerializeField] protected float _maxAngVelocity = 15f;
        [SerializeField] protected float _angleForMinTorque = 40f;
        [SerializeField] protected float _angleForMaxTorque = 140f;

        [SerializeField] protected AnimationCurve _flipCurve;
        [SerializeField] protected MinMaxValue _flipTorqueRange = new MinMaxValue(1f, 10f);

        [SerializeField] protected float _flipRotationStep = 10f;
        */

        [Header("Scrape")]
        [SerializeField] protected float _minAngleToScrape = 20f;
        [SerializeField] protected float _targetScrapeAngle = 30f;
        [SerializeField] protected float _targetScrapeAngleTime = 1f;
        [SerializeField] protected float _targetScrapeAngleTimeDelay = .5f;
        [SerializeField] protected Vector3 _scrapeTipOffset;
        [SerializeField] protected Vector3 _spiralOffsetFromTip;
        [SerializeField] protected float _scrapeItemGainPerSec = 50f;
        [SerializeField] protected MinMaxValue _scrapeSpeedModifier = new MinMaxValue(20f, 50f);
        [SerializeField] protected float _scrapeSpeedEaseTime = 1f;
        [SerializeField] protected CGEase.Ease _scrapeSpeedEasing = CGEase.Ease.EaseOutQuad;

        [Header("Slice")]
        [SerializeField] protected SpatulaCut _spatulaCut = SpatulaCut.Horizontal;
        [SerializeField, Layer] protected int _spatulaCutPartLayer;
        [SerializeField] protected float _spatulaCutForceForward = 10f;
        [SerializeField] protected float _spatulaCutForceUp = 5f;
        [SerializeField] protected float _sliceVerticalSpeedModifierStopDelay = .5f;
        [SerializeField] protected MinMaxValue _sliceVerticelSpeedModifier = new MinMaxValue(25f, 50f);
        [SerializeField] protected CGEase.Ease _sliceVerticalEasing = CGEase.Ease.EaseOutQuad;
        [SerializeField] protected float _sliceVerticalSpeedEaseTime = 1f;
        [SerializeField] protected CGEase.Ease _sliceVerticalRotationEasing = CGEase.Ease.EaseOutQuad;
        [SerializeField] protected float _sliceVerticalRotationEaseTime = .25f;
        [SerializeField] protected float _sliceVerticalRotation = 120f;
        [SerializeField] protected Transform _spatulaCutterHorizontal;
        [SerializeField] protected Transform _spatulaCutterVertical;

        [Header("References")]
        [SerializeField] protected Transform _spatulaTip;
        [SerializeField] protected List<Transform> _models;
        [SerializeField] protected SpiralController _spiralPrefab;
        [SerializeField] protected GameObject _deathVFX;
        [SerializeField] protected GameObject _metalHitVFX;
        [SerializeField] protected GameObject _stuckHitVFX;
        [SerializeField] protected List<ParticleSystem> _scrapeVFXs;

        [SerializeField] protected StateMachine<SpatulaStatus> _status;
        [SerializeField] protected StateMachine<SpatulaBehaviour> _behaviour;

        [Header("Editor")]
        [SerializeField, ReadOnly] protected SpatulaStatus _statusEditor;           // Editor only
        [SerializeField, ReadOnly] protected SpatulaBehaviour _behaviourEditor;     // Editor only

        public bool IsActive { get; protected set; } = true;
        public Vector3 TipPosition => _spatulaTip.position;
        public SpatulaStatus CurrentStatus => _status.CurrentState;
        public int CurrentSpatulaPoint => Mathf.CeilToInt(_currentSpatulaGainedPoint);
        public float FollowerUpVectorSign => (_currentSurfacePart != null && _currentSurfacePart.RightVectorSign < 0f) ? -1f : 1f;
        public Rigidbody RB => _rigidbody;

        protected GameplayController _gameplay;
        protected Rigidbody _rigidbody;

        // General variables
        protected Surface _currentSurface;
        protected SurfaceScrapePart _currentSurfacePart;
        protected Vector3 _collisionNormal;
        protected Vector3 _offsetOfTipToFollower;
        protected bool _spatulaActionedThisFrame;
        protected float _currentSpatulaGainedPoint;
        protected SplineSample _tmpSplineSample = new SplineSample();

        // Flip variables
        protected bool _canFlip = true;
        protected float _flipTime = -50f;

        // Scrape variables
        protected SpiralController _currentSpiral;
        protected float _scrapeTimer;
        protected float _scrapeRotateTimer;
        protected float _scrapeTimerDelay;
        protected float _currentScrapeSpeedModifier;
        protected Quaternion _scrapeStartRotation;
        protected CGEase.Function _scrapeEasingFunction;

        // Slice variables
        protected RigidbodyStorage _rigidbodyStorage;
        protected bool _isSlicingVertically;
        protected float _sliceSpeedModifierStopTimer;
        protected float _currentSliceVerticalSpeedModifier;
        protected float _currentVerticalSliceTime;
        protected float _verticalSliceMinSpeed;
        protected Quaternion _sliceStartRotation;
        protected CGEase.Function _sliceEasingFunction;
        protected CGEase.Function _sliceRotationEasingFunction;

        protected RaycastHit[] _raycastHits = new RaycastHit[10];

        protected enum TimerEnum
        {
            None,
            Long,
            Short
        }
        protected float _flipBalanceTimer;
        protected float currentTimer = 0f;
        protected TimerEnum timerEnum = TimerEnum.None;


        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody>();
            _status = new StateMachine<SpatulaStatus>(gameObject, false);
            _behaviour = new StateMachine<SpatulaBehaviour>(gameObject, true);

            _rigidbodyStorage = new RigidbodyStorage(_rigidbody);
            _rigidbody.isKinematic = true;
            _rigidbody.maxAngularVelocity = _maxAngularVelocity;

            _sliceEasingFunction = CGEase.GetEasingFunction(_sliceVerticalEasing);
            _scrapeEasingFunction = CGEase.GetEasingFunction(_scrapeSpeedEasing);
            _sliceRotationEasingFunction = CGEase.GetEasingFunction(_sliceVerticalRotationEasing);

            _status.ChangeState(SpatulaStatus.Stuck);
            _behaviour.ChangeState(SpatulaBehaviour.None);
        }

        protected virtual void Start()
        {
            _gameplay = GameplayController.Instance;
        }

        protected virtual void FixedUpdate()
        {
            if (_status.CurrentState == SpatulaStatus.Flipping)
            {
                // FIRST
                /*
                float angle = transform.eulerAngles.x;
                float targetAngVelocity = _rigidbody.angularVelocity.x;
                float angleMinAddition = _angleForMaxTorque + 15f;
                if (targetAngVelocity >= 0f)
                {
                    if (angle >= angleMinAddition && angle <= _angleForMaxTorque)
                    {
                        float percentage = CGMaths.Remap(angle, angleMinAddition, _angleForMaxTorque, 0f, 1f);
                        float percentageEased = CGEase.EaseInQuad(0f, 1f, percentage);
                        float angVelocity = CGMaths.Remap(percentageEased, 0f, 1f, angleMinAddition, _maxAngVelocity);
                        targetAngVelocity = angVelocity;
                    }
                    else
                    {
                        float minAngle = _angleForMinTorque + 360f;
                        if (angle < _angleForMinTorque)
                            angle += 360f;

                        float percentage = CGMaths.Remap(angle, _angleForMaxTorque, minAngle, 0f, 1f);
                        float percentageEased = CGEase.EaseInQuad(0f, 1f, percentage);
                        float angVelocity = CGMaths.Remap(percentageEased, 0f, 1f, _maxAngVelocity, _minAngVelocity);
                        targetAngVelocity = angVelocity;
                    }
                }

                Vector3 targetAngVel = _rigidbody.angularVelocity;
                targetAngVel.x = targetAngVelocity;
                targetAngVel = Vector3.MoveTowards(_rigidbody.angularVelocity, targetAngVel, _velocityApproach * Time.deltaTime);
                _rigidbody.angularVelocity = targetAngVel;
                */

                // SECOND
                /*
                float currentAngle = transform.eulerAngles.x;
                if (currentAngle < _flipAngle)
                    currentAngle += 360f;

                float anglePercent = CGMaths.Remap(currentAngle, _flipAngle, _flipAngle + 360f, 0f, 1f);
                float remapedTorque = CGMaths.Remap(_flipCurve.Evaluate(anglePercent), 0f, 1f, _flipTorqueRange.Min, _flipTorqueRange.Max);
                float targetAngVelocity = remapedTorque;

                Vector3 targetAngVel = _rigidbody.angularVelocity;
                targetAngVel.x = targetAngVelocity;
                targetAngVel = Vector3.MoveTowards(_rigidbody.angularVelocity, targetAngVel, _velocityApproach * Time.deltaTime);
                _rigidbody.angularVelocity = targetAngVel;
                */

                if (Time.time - _flipTime >= .5f)
                {
                    _rigidbody.detectCollisions = true;
                }

                if (_isSlicingVertically)
                {
                    _sliceSpeedModifierStopTimer -= Time.fixedDeltaTime;
                    if (_sliceSpeedModifierStopTimer <= 0f)
                    {
                        // Stop slicing vertically.
                        StopVerticalSlicing();
                    }
                }

                if (_isSlicingVertically)
                {
                    _currentVerticalSliceTime += Time.fixedDeltaTime;

                    float percent = (_currentVerticalSliceTime / _sliceVerticalSpeedEaseTime).Clamp(0f, 1f);
                    _currentSliceVerticalSpeedModifier = _sliceEasingFunction(_verticalSliceMinSpeed, _sliceVerticelSpeedModifier.Max, percent);
                    _currentSliceVerticalSpeedModifier = _currentSliceVerticalSpeedModifier.ClampMax(_sliceVerticelSpeedModifier.Max);

                    // Debug.Log("SPEED: " + _currentSliceVerticalSpeedModifier.ToString("F1") + ", Percent: " + percent.ToString("F1") + ", Time: " + _currentVerticalSliceTime.ToString("F1"));

                    var hitCount = Physics.RaycastNonAlloc(_spatulaTip.position
                                                            , Vector3.down
                                                            , _raycastHits
                                                            , (_currentSliceVerticalSpeedModifier + .1f) * Time.fixedDeltaTime
                                                            , _slicableLayerMask);

                    CGDebug.DebugDrawArrow(_spatulaTip.position, Vector3.down * (_currentSliceVerticalSpeedModifier + 1f) * Time.fixedDeltaTime, Color.red);

                    if (hitCount > 0)
                    {
                        for (int index = 0; index < hitCount; index ++)
                        {
                            var hit = _raycastHits[index];
                            var slicable = Slicable.GetFromCollider(hit.collider);
                            if (slicable != null)
                                Slice(slicable);
                        }
                    }

                    var rotationPercent = (_currentVerticalSliceTime / _sliceVerticalRotationEaseTime).Clamp(0f, 1f);
                    var targetRotation = Quaternion.LookRotation(Quaternion.Euler(_sliceVerticalRotation, 0f, 0f) * Vector3.forward, Vector3.forward);

                    _rigidbody.transform.rotation = Quaternion.Lerp(_sliceStartRotation, targetRotation, rotationPercent);
                    _rigidbody.velocity = Vector3.down * _currentSliceVerticalSpeedModifier;
                    // _rigidbody.transform.position += Vector3.down * (_currentSliceVerticalSpeedModifier) * Time.fixedDeltaTime;
                }
                else
                {
                    bool isBalanceEnabled = true;

                    if (_flipBalanceTimer > 0f)
                    {
                        _flipBalanceTimer -= Time.fixedDeltaTime;
                        if (_flipBalanceTimer > 0f)
                            isBalanceEnabled = false;
                    }

                    // THIRD
                    // eulerAngles.x gives wrong rotation (gimbal problem?), so we are using direction to find rotation on X.
                    // float currentAngle = _rigidbody.rotation.eulerAngles.x;
                    if (isBalanceEnabled)
                    {
                        float currentAngle = Vector3.SignedAngle(Vector3.up, transform.up, Vector3.right);
                        currentAngle = CGMaths.PositiveAngle(currentAngle);

                        float targetAngVelocity = _rigidbody.angularVelocity.x;
                        if (currentAngle >= ArcLongAngles.Min - .1f && currentAngle <= ArcLongAngles.Max)
                        {
                            if (timerEnum != TimerEnum.Long)
                            {
                                timerEnum = TimerEnum.Long;
                                // Debug.Log("T Short: " + currentTimer.ToString("F1"));
                                currentTimer = 0f;
                            }

                            currentTimer += Time.fixedDeltaTime;

                            float percentage = CGMaths.Remap(currentAngle, ArcLongAngles.Min, ArcLongAngles.Max, 0f, 1f);
                            float currentTime = (percentage * ArcLongTime + Time.fixedDeltaTime).Clamp(0f, ArcLongTime);
                            percentage = CGMaths.Remap(currentTime, 0f, ArcLongTime, 0f, 1f);
                            // float percentageEased = CGEase.EaseInQuad(0f, 1f, percentage);
                            float targetAngle = CGMaths.Remap(percentage, 0f, 1f, ArcLongAngles.Min, ArcLongAngles.Max);
                            targetAngVelocity = (targetAngle - currentAngle);
                            if (targetAngVelocity == 0f)
                                targetAngVelocity = 1f;
                        }
                        else
                        {
                            if (timerEnum != TimerEnum.Short)
                            {
                                timerEnum = TimerEnum.Short;
                                // Debug.Log("T Long: " + currentTimer.ToString("F1"));
                                currentTimer = 0f;
                            }

                            currentTimer += Time.fixedDeltaTime;

                            float minAngle = ArcLongAngles.Min + 360f;
                            if (currentAngle < ArcLongAngles.Min)
                                currentAngle += 360f;

                            float percentage = CGMaths.Remap(currentAngle, ArcLongAngles.Max, minAngle, 0f, 1f);
                            float currentTime = (percentage * ArcFastTime + Time.fixedDeltaTime).Clamp(0f, ArcFastTime);
                            percentage = CGMaths.Remap(currentTime, 0f, ArcFastTime, 0f, 1f);
                            // float percentageEased = CGEase.EaseInQuad(0f, 1f, percentage);
                            float targetAngle = CGMaths.Remap(percentage, 0f, 1f, ArcLongAngles.Max, minAngle);
                            targetAngVelocity = (targetAngle - currentAngle);

                            // Debug.Log("O VELO: " + _rigidbody.angularVelocity.x.ToString("F1") +  "N VELO: " + targetAngVelocity.ToString("F1") + ", TA : " + targetAngle.ToString("F1") + ", CA: " + currentAngle.ToString("F1") + ", T: " + currentTimer.ToString("F2"));

                            if (targetAngVelocity == 0f)
                                targetAngVelocity = 1f;
                        }

                        Vector3 targetAngVel = _rigidbody.angularVelocity;
                        targetAngVel.x = (targetAngVelocity * Mathf.Deg2Rad) / Time.fixedDeltaTime;
                        targetAngVel = Vector3.MoveTowards(_rigidbody.angularVelocity, targetAngVel, _velocityApproach * Time.fixedDeltaTime);
                        _rigidbody.angularVelocity = targetAngVel;
                    }
                }
            }

            _rigidbodyStorage.Update();
        }

        protected virtual void Update()
        {
            if (_status.CurrentState == SpatulaStatus.Scraping)
            {
                if (_gameplay.Follower.result.percent >= 1f)
                {
                    OnFollowerReachedEnd(1f);
                    return;
                }

                _scrapeTimer += Time.deltaTime;
                _currentSpatulaGainedPoint += Time.deltaTime * _scrapeItemGainPerSec;

                float percent = (_scrapeTimer / _scrapeSpeedEaseTime).Clamp(0f, 1f);
                _currentScrapeSpeedModifier = _scrapeEasingFunction(_scrapeSpeedModifier.Min, _scrapeSpeedModifier.Max, percent);
                _gameplay.Follower.followSpeed = _currentScrapeSpeedModifier;

                bool isRotating = true;
                // Delay before start rotating.
                if (_scrapeTimerDelay < _targetScrapeAngleTimeDelay)
                {
                    _scrapeTimerDelay += Time.deltaTime;
                    if (_scrapeTimerDelay >= _targetScrapeAngleTimeDelay)
                        _scrapeStartRotation = _rigidbody.transform.rotation;
                    else
                        isRotating = false;
                }

                Vector3 forwardVector = _gameplay.Follower.transform.forward;
                Vector3 upwardVector = _gameplay.Follower.transform.up;
                Vector3 rightVector = _gameplay.Follower.transform.right;
                float targetScrapeAngle = -_targetScrapeAngle;
                if (_currentSurfacePart.RightVectorSign < 0f)
                {
                    upwardVector *= -1f;
                    targetScrapeAngle *= -1f;
                }

                Debug.DrawRay(_gameplay.Follower.transform.position, forwardVector * 5f, Color.red);
                Debug.DrawRay(_gameplay.Follower.transform.position, upwardVector * 5f, Color.yellow);
                Debug.DrawRay(_gameplay.Follower.transform.position, rightVector * 5f, Color.green);

                if (isRotating)
                {
                    // Rotate with spline follower rotation to target rotation with a slight time.
                    _scrapeRotateTimer += Time.deltaTime;
                    _scrapeRotateTimer = _scrapeRotateTimer.Clamp(0f, _targetScrapeAngleTime);
                    var timeClamped = CGMaths.Remap(_scrapeRotateTimer, 0f, _targetScrapeAngleTime, 0f, 1f);
                    // var EasingFunction = CGEase.GetEasingFunctionDerivative(_scrapeTargetAngleEasing);
                    // timeClamped = EasingFunction(0f, 1f, timeClamped);
                    timeClamped = CGEase.EaseOutCubic(0f, 1f, timeClamped);
                    var fromRotation = _scrapeStartRotation;

                    // var targetRotation = Quaternion.LookRotation(forwardVector, upwardVector);
                    // targetRotation *= Quaternion.AngleAxis(_targetScrapeAngle, rightVector);

                    /*
                    Vector3 targetForwards = Quaternion.AngleAxis(_targetScrapeAngle, rightVector) * forwardVector;
                    Vector3 targetUpwards = Quaternion.AngleAxis(_targetScrapeAngle, rightVector) * upwardVector;
                    targetUpwards *= -1f;
                    var targetRotation = Quaternion.LookRotation(targetForwards, targetUpwards);
                    */

                    var targetRotation = _gameplay.Follower.transform.rotation * Quaternion.AngleAxis(targetScrapeAngle, _gameplay.Follower.transform.right) * Quaternion.AngleAxis(90f, _gameplay.Follower.transform.right);

                    // var targetRotation = Quaternion.LookRotation(Quaternion.Euler(_targetScrapeAngle, 0f, 0f) * forwardVector, Vector3.forward);

                    _rigidbody.transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, timeClamped);
                }

                //Vector3 tipNewPosition = _gameplay.Follower.transform.position - _offsetOfTipToFollower;
                //Vector3 tipOffset = _spatulaTip.transform.position - tipNewPosition;
                //_rigidbody.transform.position = _rigidbody.transform.position - tipOffset;

                // Position spatula with follower position and spatula tip.
                Vector3 tipOffsetFromSpatula = _rigidbody.transform.position - _spatulaTip.transform.position;
                var tipNewPosition = _gameplay.Follower.transform.position;
                tipNewPosition += _gameplay.Follower.transform.right * _scrapeTipOffset.x
                                + upwardVector * _scrapeTipOffset.y
                                + _gameplay.Follower.transform.forward * _scrapeTipOffset.z;
                _rigidbody.transform.position = tipNewPosition + tipOffsetFromSpatula;
            }
        }

        protected virtual void LateUpdate()
        {
            // _canFlip = GameplayController.Instance.LevelCeiling.position.y > _rigidbody.position.y;
            _canFlip = GameplayController.Instance.CanFlip(_rigidbody.transform.position);

            _spatulaActionedThisFrame = false;

            // Set values for editor.
            _statusEditor = _status.CurrentState;
            _behaviourEditor = _behaviour.CurrentState;
        }

        protected virtual void Flip()
        {
            HapticManager.Instance.HapticOnFlip();

            if (_status.CurrentState == SpatulaStatus.Scraping)
            {
                float scaler = 1f;
                if (_currentSurfacePart != null && _currentSurfacePart.RightVectorSign < 0f)
                {
                    scaler *= -1f;
                }
                transform.position += _gameplay.Follower.transform.up * scaler;
            }
            else if (_status.CurrentState == SpatulaStatus.Stuck)
                transform.position += _collisionNormal * 1f; 

            // If scraping, stop it.
            StopScraping();
            StopVerticalSlicing();

            // Vector3 force = Quaternion.Euler(_forceAngle, 90f, 0f) * Vector3.forward * _force;
            Vector3 torque = new Vector3(_torque.x, _torque.y, 0f);
            Vector3 force = new Vector3(0f, _force.y, _force.z);

            _rigidbody.isKinematic = true;
            _rigidbody.isKinematic = false;
            //if (_status.CurrentState == SpatulaStatus.Scraping || _status.CurrentState == SpatulaStatus.Stuck)
            //    _rigidbody.detectCollisions = false;

            _rigidbody.AddForce(force, ForceMode.Impulse);
            _rigidbody.AddTorque(torque, ForceMode.Impulse);

            if (_status.CurrentState != SpatulaStatus.Flipping)
                _flipTime = Time.time;

            _flipBalanceTimer = _flipBalanceDelay;

            EventManager.TriggerEvent(new SpatulaEvent(SpatulaEventType.Flipped));

            _behaviour.ChangeState(SpatulaBehaviour.None);
            _status.ChangeState(SpatulaStatus.Flipping);

            // Debug.Break();
        }

        protected virtual void Stick(Surface surface, GameObject surfaceGo)
        {
            _rigidbody.isKinematic = true;
            _currentSurface = surface;

            // Check if spatula stuck to finish point.
            var levelFinishPoint = surfaceGo.GetComponent<LevelFinishPoint>() ?? surfaceGo.GetComponent<LevelFinishPointZone>()?.Owner;
            if (levelFinishPoint != null)
            {
                levelFinishPoint.ShowConfetties();
                GameplayController.Instance.LevelSuccess(levelFinishPoint.Scale);
            }

            _spatulaActionedThisFrame = true;

            // Create particle with surface material color.
            var particle = Instantiate(_stuckHitVFX, GameplayController.Instance.LevelContainer);
            particle.transform.position = _spatulaTip.transform.position;

            var renderer = surfaceGo.GetComponent<Renderer>();
            if (renderer != null)
            {
                ParticleSystem ps = particle.GetComponent<ParticleSystem>();
                var col = ps.colorOverLifetime;
                var colorKeys = col.color.gradient.colorKeys;
                var alphaKeys = col.color.gradient.alphaKeys;
                var rendererMatColor = renderer.material.GetBaseColor();
                colorKeys[colorKeys.Length - 1].color = rendererMatColor;
                colorKeys[colorKeys.Length - 2].color = rendererMatColor;
                Gradient grad = new Gradient();
                grad.SetKeys(colorKeys, alphaKeys);
                col.color = grad;
            }

            HapticManager.Instance.HapticOnStuck();

            EventManager.TriggerEvent(new SpatulaEvent(SpatulaEventType.Stuck));

            _behaviour.ChangeState(SpatulaBehaviour.None);
            _status.ChangeState(SpatulaStatus.Stuck);
        }

        protected virtual void Scrape(SurfaceScrapePart surfacePart)
        {
            var surface = surfacePart.Owner;

            _rigidbody.isKinematic = true;
            _currentSurface = surface;
            _currentSurfacePart = surfacePart;

            _spatulaActionedThisFrame = true;

            _currentSpatulaGainedPoint = 0;
            _currentScrapeSpeedModifier = _scrapeSpeedModifier.Min;

            HapticManager.Instance.HapticOnScrapeStart(surface.Type);

            // Start following surface spine.
            _gameplay.Follower.spline = surface.Spline;
            _gameplay.Follower.RebuildImmediate();
            _gameplay.Follower.onEndReached += OnFollowerReachedEnd;

            Vector3 pos = GetPositionOnSpline(_spatulaTip.position);
            _gameplay.Follower.startPosition = _tmpSplineSample.percent;
            _gameplay.Follower.SetPercent(_tmpSplineSample.percent);
            _gameplay.Follower.followSpeed = _currentScrapeSpeedModifier;
            _gameplay.Follower.follow = true;

            // Start spinnig spiral.
            _currentSpiral = Instantiate(_spiralPrefab, _gameplay.LevelContainer);
            _currentSpiral.Spin(_spatulaTip, _spiralOffsetFromTip, surface.SpiralMainMaterial, surface.SpiralSurfaceMaterial, surface.SpiralWidth);

            _scrapeTimer = 0f;
            _scrapeRotateTimer = 0f;
            _scrapeTimerDelay = 0f;
            _scrapeStartRotation = _rigidbody.transform.rotation;

            _offsetOfTipToFollower = _gameplay.Follower.transform.position - _spatulaTip.position;

            surface.ScrapeStarted(_tmpSplineSample.percent, surfacePart);

            foreach (var particle in _scrapeVFXs)
            {
                var particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
                particleRenderer.material.SetColor("_BaseColor", surface.ParticleMainColor);
                particleRenderer.material.SetColor("_EmissionColor", surface.ParticleEmissionColor);
                particle.Play(false);
            }

            EventManager.TriggerEvent(new SpatulaEvent(SpatulaEventType.ScrapeStarted));

            // Change status to scraping.
            _status.ChangeState(SpatulaStatus.Scraping);
        }

        protected virtual bool Slice(Slicable slicable)
        {
            var slicableObject = slicable.gameObject;
            var slicerObject = _spatulaCut == SpatulaCut.Horizontal ? _spatulaCutterHorizontal : _spatulaCutterVertical;
            var slicedObjects = slicableObject.SliceInstantiate(slicerObject.transform.position,
                                    slicerObject.transform.up,
                                    slicable.SliceMaterial);

            if (slicedObjects != null && slicedObjects.Length > 0)
            {
                slicable.OnSliced(slicerObject.transform.position);
                slicableObject.SetActive(false);

                _spatulaActionedThisFrame = true;

                HapticManager.Instance.HapticOnSliced();

                // add rigidbodies and colliders
                foreach (GameObject shatteredObject in slicedObjects)
                {
                    shatteredObject.transform.SetParent(GameplayController.Instance.LevelContainer);
                    shatteredObject.transform.position = slicableObject.transform.position;
                    shatteredObject.transform.localScale = slicableObject.transform.lossyScale;
                    shatteredObject.transform.rotation = slicableObject.transform.rotation;
                    shatteredObject.SetLayer(_spatulaCutPartLayer, true);

                    var shatteredMeshFilter = shatteredObject.GetComponent<MeshFilter>();
                    var shatteredCollider = shatteredObject.AddComponent<MeshCollider>();
                    var shatteredRigidbody = shatteredObject.AddComponent<Rigidbody>();

                    shatteredCollider.convex = true;

                    Vector3 shatteredPosition = shatteredObject.transform.TransformPoint(shatteredMeshFilter.mesh.bounds.center);
                    Vector3 forcePosition = _spatulaTip.position;
                    Vector3 force = (shatteredPosition - forcePosition).normalized * _spatulaCutForceForward;
                    force += Vector3.up * _spatulaCutForceUp;

                    CGDebug.DrawCube(shatteredPosition, Color.red, Vector3.one * .5f);

                    //Vector3 cross = CGMaths.IsLeft(shatteredPosition, slicerObject.forward, slicerObject.transform.position);
                    //float sign = _spatulaCut == SpatulaCut.Horizontal ? -cross.x : cross.y;
                    //shatteredObject.name = sign.ToString();

                    shatteredRigidbody.AddForce(force, ForceMode.VelocityChange);
                    // shatteredRigidbody.AddForceAtPosition(force, forcePosition, ForceMode.VelocityChange);

                    Destroy(shatteredObject, 10f);
                }

                // _rigidbodyStorage.Restore();
                _sliceSpeedModifierStopTimer = _sliceVerticalSpeedModifierStopDelay;

                EventManager.TriggerEvent(new SpatulaEvent(SpatulaEventType.Sliced));

                return true;
            }

            return false;
        }

        protected virtual void StartVerticalSlicing()
        {
            if (!_isSlicingVertically)
            {
                _isSlicingVertically = true;
                _sliceSpeedModifierStopTimer = _sliceVerticalSpeedModifierStopDelay;
                _sliceStartRotation = _rigidbody.transform.rotation;
                _verticalSliceMinSpeed = Mathf.Max(Mathf.Abs(_spatulaTrigger.CurrentVelocity.y), _sliceVerticelSpeedModifier.Min);
                _currentVerticalSliceTime = 0f;
                _rigidbody.isKinematic = false;
            }
        }

        protected virtual void StopVerticalSlicing()
        {
            if (_isSlicingVertically)
            {
                _isSlicingVertically = false;
                _rigidbody.isKinematic = false;
                _rigidbody.velocity = Vector3.down * (_currentSliceVerticalSpeedModifier);
                _flipBalanceTimer = _flipBalanceDelay;
                _spatulaTrigger.ResetVelocity();
            }
        }

        protected virtual void StopScraping()
        {
            if (_status.CurrentState == SpatulaStatus.Scraping)
            {
                foreach (var particle in _scrapeVFXs)
                {
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }

                (_currentSurface as SurfaceScrape)?.ScrapeStopped();

                if (_gameplay.Follower.spline != null)
                {
                    _gameplay.Follower.follow = false;
                    _gameplay.Follower.onEndReached -= OnFollowerReachedEnd;
                    _gameplay.Follower.spline = null;
                }

                LevelManager.Instance.PickItem(new PickableItemEvent(new Item { Quantity = (uint)CurrentSpatulaPoint, Type = ItemType.Gold }));
                HapticManager.Instance.StopHapticScrape();
                EventManager.TriggerEvent(new SpatulaEvent(SpatulaEventType.ScrapeEnded));

                _currentSpiral.Release();
                _currentSpiral = null;
            }
        }

        public virtual void Die()
        {
            IsActive = false;

            var particle = Instantiate(_deathVFX, GameplayController.Instance.LevelContainer);
            particle.transform.position = _rigidbody.transform.position;

            _rigidbody.isKinematic = true;

            foreach (var model in _models)
                model.gameObject.SetActive(false);

            GameplayController.Instance.LevelFailed();
        }

        /*
        protected virtual void RemoveSurface()
        {
            var removingObject = _currentSurface.gameObject;
            var removerObject = _meshRemover.gameObject;
            CSGHelper.Substract(removingObject, removerObject);
        }
        */

        public Vector3 GetPositionOnSpline(Vector3 position)
        {
            Vector3 newPosition = position;
            _gameplay.Follower.Project(newPosition, _tmpSplineSample);
            newPosition = _tmpSplineSample.position;

            return newPosition;
        }

        public Vector3 GetFollowerPosition()
        {
            Vector3 roadOffset = _scrapeTipOffset;
            Vector3 newPosition = _gameplay.Follower.transform.position;
            // newPosition.y = newPosition.y - (Collider.height * Collider.transform.lossyScale.y * .5f) - Collider.center.y;
            newPosition -= _gameplay.Follower.transform.right * roadOffset.x
                            - _gameplay.Follower.transform.up * roadOffset.y
                            - _gameplay.Follower.transform.forward * roadOffset.z;

            _gameplay.Follower.Project(newPosition, _tmpSplineSample);
            newPosition = _tmpSplineSample.position;

            return newPosition;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {

        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (_killerLayerMask.Contains(collision.collider.gameObject.layer)
                || collision.collider.gameObject.GetComponent<Killer>() != null)
            {
                Die();
                return;
            }

            if (!_spatulaActionedThisFrame)
            {
                var particle = Instantiate(_metalHitVFX, GameplayController.Instance.LevelContainer);
                particle.transform.position = collision.contacts[0].point;
            }

            _flipBalanceTimer = _flipBalanceDelay;
        }

        public virtual void OnExternalTriggerEnter(Collider collider)
        {

        }

        public virtual void OnExternalCollisionEnter(Collision collision)
        {
            // Debug.Log("COLLISION!: " + collision.collider.gameObject.name + ", " + collision.contactCount);

            var contact = collision.contacts[0];

            CGDebug.DrawCube(collision.contacts[0].point, Color.yellow, Vector3.one * 1f);
            CGDebug.DrawBox(collision.contacts[0].point, Vector3.one * .5f, Quaternion.LookRotation(collision.contacts[0].normal), Color.yellow);
            CGDebug.DebugDrawArrow(collision.contacts[0].point, collision.contacts[0].normal * 3f, Color.red);

            // Debug.Log("Collision FOrce: " + collision.impulse.magnitude.ToString("F2") + ", " + collision.relativeVelocity.magnitude.ToString("F2"));

            var other = collision.collider;

            if (!IsActive) return;

            if (_killerLayerMask.Contains(other.gameObject.layer) 
                || other.gameObject.GetComponent<Killer>() != null)
            {
                Die();
                return;
            }

            if (_status.CurrentState != SpatulaStatus.Flipping)
                return;

            var slicable = Slicable.GetFromCollider(other);
            if (slicable != null)
            {
                var isSliced = Slice(slicable);

                if (isSliced && contact.normal.y > 0f)
                {
                    StartVerticalSlicing();
                }

                return;
            }

            if (other.gameObject.layer == _spatulaCutPartLayer)
                return;

            if (Time.time - _flipTime >= .75f)
            {
                var scrapeSurfacePart = SurfaceScrapePart.GetFromCollider(other);
                var scrapeSurface = scrapeSurfacePart?.Owner;

                var collideAngle = Vector3.SignedAngle(-_rigidbody.transform.up, contact.normal, _rigidbody.transform.right * (scrapeSurfacePart != null ? scrapeSurfacePart.RightVectorSign : 1f));
                // var scrapeSurface = Surface.GetFromCollider<SurfaceScrape>(other);

                // If surface can be scrape, scrape it.
                if (scrapeSurface != null && collideAngle >= _minAngleToScrape/* && collideAngle <= 70f*/)
                {
                    _collisionNormal = contact.normal;

                    Scrape(scrapeSurfacePart);
                    return;
                }

                if (_stickLayerMask.Contains(other.gameObject.layer) && collideAngle >= _minAngleToStick)
                {
                    // If collided object is not a surface or surface and it is permitted to stick, stick to surface.
                    var surface = Surface.GetFromCollider(other);
                    if (surface == null || surface.IsStickable)
                    {
                        _collisionNormal = contact.normal;

                        // Sometimes spatula get stuck too deep in surface, to prevent that, we align it with collision point.
                        Vector3 tipOffsetFromSpatula = _rigidbody.transform.position - _spatulaTip.transform.position;
                        var tipNewPosition = contact.point;
                        tipNewPosition.x = _rigidbody.transform.position.x;
                        _rigidbody.transform.position = tipNewPosition + tipOffsetFromSpatula;

                        Stick(surface, other.gameObject);
                        return;
                    }
                }
            }
        }

        private void OnFollowerReachedEnd(double obj)
        {
            (_currentSurface as SurfaceScrape)?.OnReachedRoadEnd();

            Flip();
        }

        public void OnCGEvent(TouchFirstTapEvent currentEvent)
        {
            if (GameplayController.Instance.IsLevelStarted
                && _canFlip
                && IsActive)
                Flip();
        }

        public void OnCGEvent(StateChangeEvent<SpatulaBehaviour> currentEvent)
        {
            Debug.Log("OK!");
        }

        public void OnCGEvent(LevelEvent currentEvent)
        {
            switch (currentEvent.EventType)
            {
                case LevelEventType.PreCompleted:
                case LevelEventType.PreFailed:
                    {
                        IsActive = false;

                        StopScraping();
                    }
                    break;
            }
        }

        protected virtual void OnEnable()
        {
            this.EventStartListening<TouchFirstTapEvent>();
            this.EventStartListening<LevelEvent>();
            this.EventStartListening<StateChangeEvent<SpatulaBehaviour>>();
        }

        protected virtual void OnDisable()
        {
            this.EventStopListening<TouchFirstTapEvent>();
            this.EventStopListening<LevelEvent>();
            this.EventStopListening<StateChangeEvent<SpatulaBehaviour>>();
        }

#if UNITY_EDITOR
        /**
         * This is for Visual debugging purposes in the editor 
         */
        public void OnDrawGizmos()
        {
            var planeTransform = _spatulaCut == SpatulaCut.Horizontal ? _spatulaCutterHorizontal : _spatulaCutterVertical;
            if (planeTransform == null) return;

            EzySlice.Plane cuttingPlane = new EzySlice.Plane();

            // the plane will be set to the same coordinates as the object that this
            // script is attached to
            // NOTE -> Debug Gizmo drawing only works if we pass the transform
            cuttingPlane.Compute(planeTransform);

            // draw gizmos for the plane
            // NOTE -> Debug Gizmo drawing is ONLY available in editor mode. Do NOT try
            // to run this in the final build or you'll get crashes (most likey)
            cuttingPlane.OnDebugDraw(Color.red);
        }
#endif
    }
}