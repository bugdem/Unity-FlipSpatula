using UnityEngine;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Library.Control
{
    public enum TouchSwipeDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public class TouchController : Singleton<TouchController>
    {
        public float DragPercentageForSwipe = 15f;

        private Vector3 _firstTouchPos;   //First touch position
        private Vector3 _lastTouchPos;   //Last touch position
        private float _dragDistance;  //minimum distance for a swipe to be registered

        public delegate void TouchSwipeEvent(TouchSwipeDirection direction);
        public delegate void TapEvent();

        public event TouchSwipeEvent OnSwipe;
        public event TapEvent OnTap;

        // Start is called before the first frame update
        void Start()
        {
            _dragDistance = Screen.height * DragPercentageForSwipe / 100; //dragDistance is 15% height of the screen
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount == 1) // user is touching the screen with a single touch
            {
                Touch touch = Input.GetTouch(0); // get the touch
                if (touch.phase == TouchPhase.Began) //check for the first touch
                {
                    _firstTouchPos = touch.position;
                    _lastTouchPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
                {
                    _lastTouchPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
                {
                    _lastTouchPos = touch.position;  //last touch position. Ommitted if you use list

                    //Check if drag distance is greater than 20% of the screen height
                    if (Mathf.Abs(_lastTouchPos.x - _firstTouchPos.x) > _dragDistance || Mathf.Abs(_lastTouchPos.y - _firstTouchPos.y) > _dragDistance)
                    {
                        //It's a drag
                        //check if the drag is vertical or horizontal
                        var dragX = _lastTouchPos.x - _firstTouchPos.x;
                        var dragY = _lastTouchPos.y - _firstTouchPos.y;
                        if (Mathf.Abs(dragX) > Mathf.Abs(dragY))
                        {   //If the horizontal movement is greater than the vertical movement...
                            if ((_lastTouchPos.x > _firstTouchPos.x))  //If the movement was to the right)
                            {   //Right swipe
                                Debug.Log("Right Swipe");
                                OnSwipe?.Invoke(TouchSwipeDirection.Right);
                            }
                            else
                            {   //Left swipe
                                Debug.Log("Left Swipe");
                                OnSwipe?.Invoke(TouchSwipeDirection.Left);
                            }
                        }
                        else
                        {   //the vertical movement is greater than the horizontal movement
                            if (_lastTouchPos.y > _firstTouchPos.y)  //If the movement was up
                            {   //Up swipe
                                Debug.Log("Up Swipe");
                                OnSwipe?.Invoke(TouchSwipeDirection.Up);
                            }
                            else
                            {   //Down swipe
                                Debug.Log("Down Swipe");
                                OnSwipe?.Invoke(TouchSwipeDirection.Down);
                            }
                        }
                    }
                    else
                    {   //It's a tap as the drag distance is less than 20% of the screen height
                        Debug.Log("Tap");
                        OnTap?.Invoke();
                    }
                }
            }
        }
    }
}