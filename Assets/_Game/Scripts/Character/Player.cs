using UnityEngine;
using UnityEngine.Events;
using ClocknestGames.Library.Utils;
using ClocknestGames.Library.Editor;
using System.Collections.Generic;
using Dreamteck.Splines;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClocknestGames.Game.Core
{
    [System.Flags]
    public enum PlayerStatus : int
    {
        Dead = 1 << 0,
        Idle = 1 << 1,
        Running = 1 << 2,
        Falling = 1 << 3,
        Dancing = 1 << 4
    }

    public enum TouchStatus : byte
    {
        Touching = 0,
        NotTouching = 1,
        JustTouching = 2,
        JustNotTouching = 3
    }

    public enum GroundStatus : byte
    {
        Grounded = 0,
        NotGrounded = 1,
        JustGrounded = 2,
        JustNotGrounded = 3
    }

    public class Player : Character, EventListener<LevelEvent>
    {
        [Header("Character")]
        public Rigidbody Rigidbody;
        public CapsuleCollider Collider;
        public BoxCollider BoundingCollider;
        public Transform RoadFollower;

        [Header("Movement")]
        public Vector3 RunSpeed = new Vector3(8f, -9.81f, 5f);
        public Vector3 FallSpeed = new Vector3(8f, -9.81f, 5f);
        public float WalkSpeedOnLevelEnd = 5f;
        public float MoveSpeedSideDamping = 1f;
        public float MaxSwipeLengthForMove = 1000f;
        public float StopDampingOnLevelCompleted = 1f;
        public bool LookAtMoveDirection = true;

        [Header("Road")]
        public float RoadWidth = 8f;
        public float RoadInset = 1f;

        [Header("Ground Check")]
        public LayerMask GroundLayerMask;
        public float GroundCheckAngle = 30f;
        public float GroundCheckStepInterval = .2f;
        public float GroundCheckRayLength = .2f;
        public float GroundCheckRadiusScaler = .9f;

        [Header("Animation")]
        public Animator Animator;
        public float AnimationCrossFadeTime = .1f;
        public string AnimatorStateNameDead = "Standing React Death Forward";
        public string AnimatorStateNameDeadTripping = "Tripping";
        public string AnimatorStateNameRunning = "Catwalk Walking";
        public string AnimatorStateNameRailingIdle = "Railing Idle";
        public string AnimatorStateNameRailing = "Railing";
        public string AnimatorStateNameFalling = "Falling Idle";
        public string AnimatorStateNameSliding = "Falling Idle";
        public List<string> AnimatorStateNamesDancing = new List<string> { "Dancing" };
        public List<string> AnimatorStateNamesIdle = new List<string> { "Idle" };

        public UnityEvent OnLevelFinishEnteredEvent;

        public StateMachine<PlayerStatus> Status;
        public StateMachine<TouchStatus> TouchState;
        public StateMachine<GroundStatus> GroundState;

        [ReadOnly, SerializeField] private PlayerStatus _status;        // For only inspector
        [ReadOnly, SerializeField] private TouchStatus _touchState;     // For only inspector
        [ReadOnly, SerializeField] private GroundStatus _groundState;     // For only inspector

        protected const float MinVelocityToIdle = .2f;

        public float CurrentSpeed { get; protected set; }
        public Vector3 CurrentSpeedModifier { get; protected set; }
        public Vector3 CurrentVelocity { get; protected set; }
        public bool IsLevelFinishEntered { get; protected set; }
        public bool IsLevelStarted { get; protected set; }

        protected GameplayController _gameplay;
        protected TouchUIController _touch;

        protected Vector3 _groundPosition;
        protected Collider _groundCollider;
        protected Vector2 _previousTouchPosition;
        protected float _targetPositionX;
        protected Vector3 _previousPosition;
        protected float _lastDeltaTime;

        protected float _lastRoadRotation;
        protected Vector3 _lastRoadPosition;

        RaycastHit[] _boundingColliderHits = new RaycastHit[50];
        protected SplineSample _tmpSplineSample = new SplineSample();
        protected float _colliderDefaultHeight;
        protected Vector3 _colliderDefaultCenter;

        protected virtual void Start()
        {
            _gameplay = GameplayController.Instance;
            _touch = TouchUIController.Instance;

            _colliderDefaultHeight = Collider.height;
            _colliderDefaultCenter = Collider.center;

            Status = new StateMachine<PlayerStatus>(gameObject, true);
            TouchState = new StateMachine<TouchStatus>(gameObject, true);
            GroundState = new StateMachine<GroundStatus>(gameObject, true);

            TouchState.ChangeState(TouchStatus.NotTouching);
            GroundState.ChangeState(GroundStatus.Grounded);

            CurrentSpeedModifier = GetSpeedModifier();

            _previousPosition = transform.position;
            _lastDeltaTime = Time.deltaTime;

            _lastRoadPosition = transform.position;
            _lastRoadRotation = transform.eulerAngles.y;

            BoundingCollider.transform.SetParent(_gameplay.transform);
            RoadFollower.transform.SetParent(_gameplay.transform);

            transform.SetParent(RoadFollower.transform);

            ChangeStatus(PlayerStatus.Idle, crossFade: false);
        }

        protected virtual void Update()
        {
            // Set touch state.
            var isTouching = _touch.IsTouching;
            switch (TouchState.CurrentState)
            {
                case TouchStatus.NotTouching: { if (isTouching) TouchState.ChangeState(TouchStatus.JustTouching); } break;
                case TouchStatus.Touching: { if (!isTouching) TouchState.ChangeState(TouchStatus.JustNotTouching); } break;
                case TouchStatus.JustNotTouching: TouchState.ChangeState(TouchStatus.NotTouching); break;
                case TouchStatus.JustTouching: TouchState.ChangeState(TouchStatus.Touching); break;
            }

            // Set grounded state.
            var isGrounded = GroundCheck();
            switch (GroundState.CurrentState)
            {
                case GroundStatus.NotGrounded: { if (isGrounded) GroundState.ChangeState(GroundStatus.JustGrounded); } break;
                case GroundStatus.Grounded: { if (!isGrounded) GroundState.ChangeState(GroundStatus.JustNotGrounded); } break;
                case GroundStatus.JustNotGrounded: GroundState.ChangeState(GroundStatus.NotGrounded); break;
                case GroundStatus.JustGrounded: GroundState.ChangeState(GroundStatus.Grounded); break;
            }

            // Set target position x, which player will move to on x axis.
            Vector2 currentTouchPosition = _touch.GetTouchPosition();
            if (TouchState.CurrentState == TouchStatus.JustTouching)
                _previousTouchPosition = currentTouchPosition;

            if (Status.CurrentState == PlayerStatus.Dead)
                return;

            // if (!IsLevelStarted) return;

            if (!IsLevelStarted)
            {
                CurrentSpeedModifier = new Vector3(0f, GetSpeedModifier().y, 0f);
            }
            // If level completed, stop slowly.
            else if (LevelManager.Instance.IsLevelCompleted)
            {
                Vector3 newSpeedModifier = CurrentSpeedModifier;
                newSpeedModifier.z += Cinemachine.Utility.Damper.Damp(0f - newSpeedModifier.z, StopDampingOnLevelCompleted, Time.deltaTime);
                CurrentSpeedModifier = newSpeedModifier;
            }
            else
            {
                CurrentSpeedModifier = GetSpeedModifier();

                /*
                if ((IsRailing() || IsSliding()) && CurrentRoad.Holder.Follower.follow)
                {
                    CurrentRoad.Holder.Follower.followSpeed = CurrentSpeedModifier.z;
                }
                */

                if (IsLevelFinishEntered)
                {
                    CurrentSpeedModifier = new Vector3(0f, 0f, WalkSpeedOnLevelEnd);
                    _gameplay.ShowConfetties();
                    _gameplay.LevelSuccess(1);
                }
            }

            if (!LevelManager.Instance.IsLevelCompleted)
            {
                // If player is touching, update target position x related to swipe.
                if (TouchState.CurrentState == TouchStatus.Touching | TouchState.CurrentState == TouchStatus.JustTouching)
                {
                    if (Status.CurrentState == PlayerStatus.Running)
                    {
                        Vector2 touchDelta = currentTouchPosition - _previousTouchPosition;
                        float currentSwipeLength = Mathf.Abs(touchDelta.x).Clamp(0f, MaxSwipeLengthForMove);
                        float speedX = CGMaths.Remap(currentSwipeLength, 0f, MaxSwipeLengthForMove, 0f, CurrentSpeedModifier.x) * Mathf.Sign(touchDelta.x);

                        _targetPositionX += speedX;

                        float roadLaneWidth = (RoadWidth - 2 * RoadInset) * .5f;
                        _targetPositionX = _targetPositionX.Clamp(-roadLaneWidth, roadLaneWidth);
                    }
                }
            }

            // _previousTouchPosition = currentTouchPosition;

            Vector3 newPos = RoadFollower.transform.position + Vector3.up * CurrentSpeedModifier.y * Time.deltaTime;
            Quaternion newRot = RoadFollower.transform.rotation;

            bool canMoveSides = false;

            if (GroundState.CurrentState == GroundStatus.Grounded || GroundState.CurrentState == GroundStatus.JustGrounded)
                newPos.y = _groundPosition.y + (Collider.height * Collider.transform.lossyScale.y * .5f) - Collider.center.y;

            if (Status.CurrentState == PlayerStatus.Dead)
            {
                newPos.x = RoadFollower.transform.position.x;
                newPos.z = RoadFollower.transform.position.z;
            }
            else
            {
                newPos += Quaternion.Euler(0f, _lastRoadRotation, 0f) * Vector3.forward * CurrentSpeedModifier.z * Time.deltaTime;

                Vector3 lookPosition = newPos;
                lookPosition.y = RoadFollower.transform.position.y;
                if (Vector3.Distance(lookPosition, RoadFollower.transform.position) > 0.01f)
                {
                    newRot = Quaternion.LookRotation(lookPosition - RoadFollower.transform.position);
                }

                canMoveSides = true;
            }

            RoadFollower.transform.position = newPos;
            RoadFollower.transform.rotation = newRot;

            Vector3 playerPos = transform.localPosition;
            Quaternion playerRot = transform.localRotation;

            if (canMoveSides && !IsLevelFinishEntered)
            {
                Vector3 targetPosition = newPos + newRot * Vector3.right * _targetPositionX;
                targetPosition.y = newPos.y;

                float delta = Cinemachine.Utility.Damper.Damp(_targetPositionX - playerPos.x, MoveSpeedSideDamping, Time.deltaTime);
                playerPos.x += delta;
            }

            // If enabled, look to moving rotation.
            if (LookAtMoveDirection && Status.CurrentState != PlayerStatus.Falling)
            {
                Vector3 lookPosition = playerPos;
                lookPosition.y = transform.localPosition.y;
                if (Vector3.Distance(lookPosition, transform.localPosition) > 0.01f)
                {
                    playerRot = Quaternion.LookRotation(lookPosition - transform.localPosition);
                }
            }

            transform.localPosition = playerPos;
            transform.localRotation = playerRot;

            CurrentSpeed = GetCharacterSpeed();
            CurrentVelocity = GetCharacterVelocity();

            if ((Status.CurrentState == PlayerStatus.Idle || Status.CurrentState == PlayerStatus.Running) && GroundState.CurrentState == GroundStatus.JustNotGrounded)
            {
                ChangeStatus(PlayerStatus.Falling);
            }
            else if (Status.CurrentState == PlayerStatus.Falling && GroundState.CurrentState == GroundStatus.JustGrounded)
                ChangeStatus(PlayerStatus.Running);
            // If character is stopping, and speed is below move speed, change status to idle
            else if (Status.CurrentState == PlayerStatus.Running && !_gameplay.IsLevelFinishEntered && CurrentSpeed <= MinVelocityToIdle)
                ChangeStatus(PlayerStatus.Idle);

            if (Status.CurrentState == PlayerStatus.Idle && CurrentSpeed > MinVelocityToIdle)
                ChangeStatus(PlayerStatus.Running, crossFade: true, randomAnimationTime: true);

            if (Status.CurrentState == PlayerStatus.Running && CurrentSpeed <= MinVelocityToIdle)
                ChangeStatus(PlayerStatus.Idle, crossFade: true, randomAnimationTime: true);

            if (LevelManager.Instance.IsLevelCompleted && LevelManager.Instance.IsLevelSuccess && Status.CurrentState == PlayerStatus.Idle)
            {
                // transform.rotation = Quaternion.LookRotation(Vector3.forward);
                ChangeStatus(PlayerStatus.Dancing, crossFade: false);
            }

            /*
            if (Status.CurrentState != PlayerStatus.Railing && Status.CurrentState != PlayerStatus.RailingIdle && CurrentRoad != null)
            {
                ChangeStatus(PlayerStatus.Railing, crossFade: false);
            }
            */
        }

        protected virtual void LateUpdate()
        {
            _status = Status.CurrentState;
            _touchState = TouchState.CurrentState;
            _groundState = GroundState.CurrentState;

            _previousPosition = transform.position;
            _lastDeltaTime = Time.deltaTime;
        }

        protected Vector3 GetSpeedModifier()
        {
            if (Status.CurrentState == PlayerStatus.Falling)
                return FallSpeed;

            return RunSpeed;
        }

        public float GetCharacterSpeed()
        {
            return (transform.position - _previousPosition).magnitude / _lastDeltaTime;
        }

        public Vector3 GetCharacterVelocity()
        {
            return (transform.position - _previousPosition) / _lastDeltaTime;
        }

        public bool GroundCheck()
        {
            bool isGrounded = false;

            Vector3 center = transform.position + Collider.center;
            float radius = Collider.radius * Collider.transform.lossyScale.x * GroundCheckRadiusScaler;
            float height = Collider.height * Collider.transform.lossyScale.y;
            float width = radius * 2f;

            int angleCount = Mathf.CeilToInt(360f / GroundCheckAngle);
            float angleStep = 360f / angleCount;

            int intervalCount = Mathf.CeilToInt(width / GroundCheckStepInterval);

            RaycastHit hitInfo;
            for (int angleIndex = 0; angleIndex < angleCount + 1; angleIndex++)
            {
                Vector3 startPosition = center + Quaternion.AngleAxis(angleIndex * angleStep, transform.up) * (transform.forward * radius);
                Vector3 endPosition = center + Quaternion.AngleAxis(angleIndex * angleStep, transform.up) * (-transform.forward * radius);

                for (int intervalIndex = 0; intervalIndex < intervalCount + 1; intervalIndex++)
                {
                    Vector3 rayPosition = Vector3.Lerp(startPosition, endPosition, intervalIndex / (float)intervalCount);
                    Debug.DrawLine(rayPosition, rayPosition - transform.up * (height * .5f + GroundCheckRayLength), Color.red);
                    if (Physics.Raycast(rayPosition, -transform.up, out hitInfo, height * .5f + GroundCheckRayLength, GroundLayerMask))
                    {
                        _groundPosition = hitInfo.point;
                        _groundCollider = hitInfo.collider;
                        isGrounded = true;
                    }
                }
            }

            return isGrounded;
        }

        public virtual void Die(bool randomAnimationTime = false, bool crossFade = false, float startTime = 0f, bool animationEnabled = true, bool tripped = false)
        {
            if (Status.CurrentState == PlayerStatus.Dead) return;

            string animationName = null;
            if (tripped)
            {
                animationName = AnimatorStateNameDeadTripping;
            }
            else if (Status.CurrentState == PlayerStatus.Falling)
            {
                Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                Vector3 speed = CurrentVelocity;
                Rigidbody.velocity = speed;
                animationEnabled = false;
            }

            ChangeStatus(PlayerStatus.Dead, randomAnimationTime: randomAnimationTime, crossFade: crossFade, startTime: startTime, animationEnabled: animationEnabled, animationName: animationName);

            HapticManager.Instance.HapticOnCharacterKilled();

            Rigidbody.useGravity = true;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            GameplayController.Instance.LevelFailed();
        }

        #region Animation Operations
        protected void ChangeStatus(PlayerStatus newStatus, bool randomAnimationTime = false, bool crossFade = true, float startTime = 0f, bool animationEnabled = true, bool force = false, string animationName = null)
        {
            if (!force && (Status.CurrentState == newStatus || Status.CurrentState == PlayerStatus.Dead))
                return;

            Status.ChangeState(newStatus);

            switch (newStatus)
            {
                case PlayerStatus.Dead: ChangeStateToDead(randomAnimationTime, crossFade, startTime, animationEnabled, animationName); break;
                case PlayerStatus.Idle: ChangeStateToIdle(randomAnimationTime, crossFade, startTime, animationEnabled, animationName); break;
                case PlayerStatus.Running: ChangeStateToRunning(randomAnimationTime, crossFade, startTime, animationEnabled, animationName); break;
                case PlayerStatus.Falling: ChangeStateToFalling(randomAnimationTime, crossFade, startTime, animationEnabled, animationName); break;
                case PlayerStatus.Dancing: ChangeStateToDancing(randomAnimationTime, crossFade, startTime, animationEnabled, animationName); break;
            }

            OnStatusChanged();
        }

        protected virtual void OnStatusChanged()
        {

        }

        private void ChangeStateToDead(bool randomTime, bool crossFade, float startTime, bool animationEnabled, string animationName)
        {
            if (animationEnabled)
            {
                string stateName = animationName ?? AnimatorStateNameDead;
                PlayAnimation(stateName, randomTime, crossFade, startTime);
            }
        }

        private void ChangeStateToIdle(bool randomTime, bool crossFade, float startTime, bool animationEnabled, string animationName)
        {
            if (animationEnabled && (!string.IsNullOrEmpty(animationName) || AnimatorStateNamesIdle.Count > 0))
            {
                string stateName = AnimatorStateNamesIdle[Random.Range(0, AnimatorStateNamesIdle.Count)];
                PlayAnimation(stateName, randomTime, crossFade, startTime);
            }
        }

        private void ChangeStateToRunning(bool randomTime, bool crossFade, float startTime, bool animationEnabled, string animationName)
        {
            if (animationEnabled)
            {
                string stateName = animationName ?? AnimatorStateNameRunning;
                PlayAnimation(stateName, randomTime, crossFade, startTime);
            }
        }

        private void ChangeStateToRailingIdle(bool randomTime, bool crossFade, float startTime, bool animationEnabled, string animationName)
        {
            if (animationEnabled)
            {
                string stateName = animationName ?? AnimatorStateNameRailingIdle;
                PlayAnimation(stateName, randomTime, crossFade, startTime);
            }
        }

        private void ChangeStateToRailing(bool randomTime, bool crossFade, float startTime, bool animationEnabled, string animationName)
        {
            if (animationEnabled)
            {
                string stateName = animationName ?? AnimatorStateNameRailing;
                PlayAnimation(stateName, randomTime, crossFade, startTime);
            }
        }

        private void ChangeStateToFalling(bool randomTime, bool crossFade, float startTime, bool animationEnabled, string animationName)
        {
            if (animationEnabled)
            {
                string stateName = animationName ?? AnimatorStateNameFalling;
                PlayAnimation(stateName, randomTime, crossFade, startTime);
            }
        }

        private void ChangeStateToDancing(bool randomTime, bool crossFade, float startTime, bool animationEnabled, string animationName)
        {
            if (animationEnabled && (!string.IsNullOrEmpty(animationName) || AnimatorStateNamesDancing.Count > 0))
            {
                string stateName = animationName ?? AnimatorStateNamesDancing[Random.Range(0, AnimatorStateNamesDancing.Count)];
                PlayAnimation(stateName, randomTime, crossFade, startTime);
            }
        }

        private void ChangeStateToSliding(bool randomTime, bool crossFade, float startTime, bool animationEnabled, string animationName)
        {
            if (animationEnabled)
            {
                string stateName = animationName ?? AnimatorStateNameSliding;
                PlayAnimation(stateName, randomTime, crossFade, startTime);
            }
        }

        private void PlayAnimation(string stateName, bool randomTime, bool crossFade, float startTime)
        {
            PlayAnimation(Animator, stateName, randomTime, crossFade, startTime);
        }

        private void PlayAnimation(Animator animator, string stateName, bool randomTime, bool crossFade, float startTime)
        {
            if (string.IsNullOrEmpty(stateName))
                return;

            if (animator == null)
                return;

            if (crossFade)
            {
                if (randomTime) animator.CrossFade(stateName, AnimationCrossFadeTime, -1, Random.value);
                else animator.CrossFade(stateName, AnimationCrossFadeTime, -1, 0f);
            }
            else
            {
                if (randomTime) animator.Play(stateName, -1, Random.value);
                else animator.Play(stateName, -1, startTime);
            }
        }
        #endregion

        public new static Player GetFromCollider(Collider collider)
        {
            var player = collider?.GetComponent<Player>();
            if (player != null)
                return player;

            var playerBody = collider?.GetComponent<CharacterBody>();
            if (playerBody != null)
                return playerBody.Owner as Player;

            return null;
        }

        public virtual void OnLevelFinishEntered()
        {
            if (Status.CurrentState == PlayerStatus.Dead) return;

            if (IsLevelFinishEntered) return;

            IsLevelFinishEntered = true;

            OnLevelFinishEnteredEvent?.Invoke();
        }

        private void OnEnable()
        {
            this.EventStartListening<LevelEvent>();
        }

        private void OnDisable()
        {
            this.EventStopListening<LevelEvent>();
        }

        public void OnCGEvent(LevelEvent currentEvent)
        {
            if (currentEvent.EventType == LevelEventType.Started)
            {
                ChangeStatus(PlayerStatus.Running, crossFade: false, force: true);

                _previousPosition = transform.position - transform.forward * CurrentSpeedModifier.z;

                IsLevelStarted = true;
            }
        }
    }
}