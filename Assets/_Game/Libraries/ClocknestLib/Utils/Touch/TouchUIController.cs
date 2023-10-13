// Modified version of CorgiEngine's Touch Control.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using ClocknestGames.Library.Editor;

namespace ClocknestGames.Library.Utils
{
    /// <summary>
    /// The possible directions a swipe can have
    /// </summary>
    public enum SwipeDirections { Up, Down, Left, Right }

    [System.Serializable]
    public class SwipeEvent : UnityEvent<TouchSwipeEvent> { }

    /// <summary>
    /// An event usually triggered when a swipe happens. It contains the swipe "base" direction, and detailed information if needed (angle, length, origin and destination
    /// </summary>
    public struct TouchSwipeEvent
    {
        public SwipeDirections SwipeDirection;
        public float SwipeAngle;
        public float SwipeLength;
        public Vector2 SwipeOrigin;
        public Vector2 SwipeDestination;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchSwipeEvent"/> struct.
        /// </summary>
        /// <param name="direction">Direction.</param>
        /// <param name="angle">Angle.</param>
        /// <param name="length">Length.</param>
        /// <param name="origin">Origin.</param>
        /// <param name="destination">Destination.</param>
        public TouchSwipeEvent(SwipeDirections direction, float angle, float length, Vector2 origin, Vector2 destination)
        {
            SwipeDirection = direction;
            SwipeAngle = angle;
            SwipeLength = length;
            SwipeOrigin = origin;
            SwipeDestination = destination;
        }
    }

    /// <summary>
    /// An event usually triggered when a tap happens.
    /// </summary>
    public struct TouchTapEvent
    {
        public float Length;
        public Vector2 SwipeOrigin;
        public Vector2 SwipeDestination;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchTapEvent"/> struct.
        /// </summary>
        /// <param name="length">Length.</param>
        /// <param name="origin">Origin.</param>
        /// <param name="destination">Destination.</param>
        public TouchTapEvent(float length, Vector2 origin, Vector2 destination)
        {
            Length = length;
            SwipeOrigin = origin;
            SwipeDestination = destination;
        }
    }

    /// <summary>
    /// An event usually triggered when a tap happens.
    /// </summary>
    public struct TouchUpEvent
    {
        public float Length;
        public Vector2 SwipeOrigin;
        public Vector2 SwipeDestination;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchUpEvent"/> struct.
        /// </summary>
        /// <param name="length">Length.</param>
        /// <param name="origin">Origin.</param>
        /// <param name="destination">Destination.</param>
        public TouchUpEvent(float length, Vector2 origin, Vector2 destination)
        {
            Length = length;
            SwipeOrigin = origin;
            SwipeDestination = destination;
        }
    }


    /// <summary>
    /// An event usually triggered when a tap happens.
    /// </summary>
    public struct TouchEvent
    {
        public bool NewState;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchEvent"/> struct.
        /// </summary>
        /// <param name="newState">New state of event.</param>
        public TouchEvent(bool newState)
        {
            NewState = newState;
        }
    }

    /// <summary>
    /// An event usually triggered when a tap happens.
    /// </summary>
    public struct TouchFirstTapEvent
    {
        public Vector2 SwipeOrigin;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchFirstTapEvent"/> struct.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public TouchFirstTapEvent(Vector2 origin)
        {
            SwipeOrigin = origin;
        }
    }

    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// Add a swipe manager to your scene, and it'll trigger MMSwipeEvents everytime a swipe happens. From its inspector you can determine the minimal length of a swipe. Shorter swipes will be ignored
    /// </summary>
    public class TouchUIController : Singleton<TouchUIController>, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        /// the minimal length of a swipe
        public float MinimalSwipeLength = 50f;
        /// the maximum press length of a swipe
        public float MaximumPressLength = 10f;
        /// the maximum length for normalized input swipe
        public float MaximumInputNormalizedLength = 500f;
        /// the minimum length for swipe to be recognized as move
        public float MinimumSwipeLengthToMove = .5f;

        /// The method(s) to call when the zone is swiped
        public SwipeEvent ZoneSwiped;
        /// The method(s) to call while the zone is being pressed
        public UnityEvent ZonePressed;
        /// The method(s) to call when the zone is tapped first time
        public UnityEvent ZoneTapped;

        [Header("Mouse Mode")]
        [Information("If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).", InformationAttribute.InformationType.Info, false)]
        /// If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).
        public bool MouseMode = false;

        public bool IsTouching { get; private set; }
        public Vector2 InputDelta { get; private set; }
        public Vector2 InputAxis { get; private set; }
        public Vector2 InputNormalized { get; private set; }

        protected float _angle;
        protected float _length;
        protected Vector2 _firstTouchPosition;
        protected Vector2 _destination;
        protected Vector2 _deltaSwipe;
        protected Vector2 _previousTouchPosition;
        protected Vector2 _currentTouchPosition;
        protected SwipeDirections _swipeDirection;

        private void Update()
        {
            SetTouchContinuous();
        }

        private void SetTouchDefault()
        {
            InputDelta = Vector2.zero;
            InputAxis = Vector2.zero;

            if (IsTouching)
            {
                _previousTouchPosition = _currentTouchPosition;
                _currentTouchPosition = GetTouchPosition();
            }

            InputDelta = _currentTouchPosition - _previousTouchPosition;

            var inputAxis = Vector2.zero;
            if (InputDelta.x > 0) inputAxis.x = 1;
            else if (InputDelta.x < 0) inputAxis.x = -1;
            if (InputDelta.y > 0) inputAxis.y = 1;
            else if (InputDelta.y < 0) inputAxis.y = -1;

            InputAxis = inputAxis;
            InputNormalized = new Vector2(Mathf.Clamp01(Mathf.Abs(CGMaths.Normalize(InputDelta.x, 0f, MaximumInputNormalizedLength)))
                                        , Mathf.Clamp01(Mathf.Abs(CGMaths.Normalize(InputDelta.y, 0f, MaximumInputNormalizedLength))));
        }

        private void SetTouchContinuous()
        {
            if (IsTouching)
            {
                var touchPosition = GetTouchPosition();
                var currentDelta = touchPosition - _previousTouchPosition;
                if ((InputAxis.x > 0f && currentDelta.x < 0f) || (InputAxis.x < 0f && currentDelta.x > 0f))
                    _firstTouchPosition = touchPosition;

                InputDelta = touchPosition - _firstTouchPosition;

                var inputAxis = Vector2.zero;

                if (InputDelta.x > 0) inputAxis.x = 1;
                else if (InputDelta.x < 0) inputAxis.x = -1;

                if (InputDelta.y > 0) inputAxis.y = 1;
                else if (InputDelta.y < 0) inputAxis.y = -1;

                InputAxis = inputAxis;

                _previousTouchPosition = touchPosition;
            }

            InputNormalized = new Vector2(Mathf.Clamp01(Mathf.Abs(CGMaths.Normalize(InputDelta.x, 0f, MaximumInputNormalizedLength)))
                            , Mathf.Clamp01(Mathf.Abs(CGMaths.Normalize(InputDelta.y, 0f, MaximumInputNormalizedLength))));
        }

        public bool HasMovedHorizontally()
        {
            return Mathf.Abs(InputDelta.x) > MinimumSwipeLengthToMove;
        }

        protected virtual void Swipe()
        {
            TouchSwipeEvent swipeEvent = new TouchSwipeEvent(_swipeDirection, _angle, _length, _firstTouchPosition, _destination);
            EventManager.TriggerEvent(swipeEvent);
            if (ZoneSwiped != null)
            {
                ZoneSwiped.Invoke(swipeEvent);
            }
        }

        protected virtual void Press()
        {
            TouchTapEvent tapEvent = new TouchTapEvent(_length, _firstTouchPosition, _destination);
            EventManager.TriggerEvent(tapEvent);
            if (ZonePressed != null)
            {
                ZonePressed.Invoke();
            }
        }

        protected virtual void FirstTouch()
        {
            TouchFirstTapEvent tapEvent = new TouchFirstTapEvent(_firstTouchPosition);
            EventManager.TriggerEvent(tapEvent);
            if (ZoneTapped != null)
            {
                ZoneTapped.Invoke();
            }
        }

        /// <summary>
        /// Triggers the bound pointer down action
        /// </summary>
        public virtual void OnPointerDown(PointerEventData data)
        {
            IsTouching = true;
            EventManager.TriggerEvent(new TouchEvent(IsTouching));

            _firstTouchPosition = GetTouchPosition();
            _currentTouchPosition = _firstTouchPosition;
            _previousTouchPosition = _firstTouchPosition;

            FirstTouch();
        }

        /// <summary>
        /// Triggers the bound pointer up action
        /// </summary>
        public virtual void OnPointerUp(PointerEventData data)
        {
            IsTouching = false;
            EventManager.TriggerEvent(new TouchEvent(IsTouching));

            _currentTouchPosition = Vector2.zero;
            _previousTouchPosition = Vector2.zero;

            _destination = GetTouchPosition();
            _deltaSwipe = _destination - _firstTouchPosition;
            _length = _deltaSwipe.magnitude;

            // if the swipe has been long enough
            if (_length > MinimalSwipeLength)
            {
                _angle = CGMaths.AngleBetween(_deltaSwipe, Vector2.right);
                _swipeDirection = AngleToSwipeDirection(_angle);
                Swipe();
            }

            // if it's just a press
            if (_deltaSwipe.magnitude < MaximumPressLength)
            {
                Press();
            }

            // Trigger touch up event
            EventManager.TriggerEvent(new TouchUpEvent(_length, _firstTouchPosition, _destination));
        }

        /// <summary>
        /// Triggers the bound pointer enter action when touch enters zone
        /// </summary>
        public void OnPointerEnter(PointerEventData data)
        {
            if (!MouseMode)
            {
                OnPointerDown(data);
            }
        }

        /// <summary>
        /// Triggers the bound pointer exit action when touch is out of zone
        /// </summary>
        public void OnPointerExit(PointerEventData data)
        {
            if (!MouseMode)
            {
                OnPointerUp(data);
            }
        }

        /// <summary>
        /// Determines a MMPossibleSwipeDirection out of an angle in degrees. 
        /// </summary>
        /// <returns>The to swipe direction.</returns>
        /// <param name="angle">Angle in degrees.</param>
        protected virtual SwipeDirections AngleToSwipeDirection(float angle)
        {
            if ((angle < 45) || (angle >= 315))
            {
                return SwipeDirections.Right;
            }
            if ((angle >= 45) && (angle < 135))
            {
                return SwipeDirections.Up;
            }
            if ((angle >= 135) && (angle < 225))
            {
                return SwipeDirections.Left;
            }
            if ((angle >= 225) && (angle < 315))
            {
                return SwipeDirections.Down;
            }
            return SwipeDirections.Right;
        }

        public Vector2 GetTouchPosition()
        {
            return Input.mousePosition;
        }
    }
}