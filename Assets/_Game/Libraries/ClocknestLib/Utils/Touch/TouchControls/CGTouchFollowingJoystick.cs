// Modified version of CorgiEngine's MMControls, credits to MoreMountain.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClocknestGames.Library.Editor;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Library.Control
{
	/// <summary>
	/// Add this component to a UI rectangle and it'll act as a detection zone for a joystick.
	/// Note that this component extends the CGTouchJoystick class so you don't need to add another joystick to it. It's both the detection zone and the stick itself.
	/// </summary>
	public class CGTouchFollowingJoystick : CGTouchJoystick, EventListener<TouchEvent>
	{
		[SerializeField] private float _targetAlpha = .4f;
		[SerializeField] private RectTransform _maxRangeObject;

		private bool _isEnabled = true;
		private float _parentCanvasAlpha;
		private CanvasGroup _parentCanvasGroup;
		private RectTransform _parentRectTransform;
		private float _maxRange;

		protected virtual void Awake()
		{
			_parentCanvasGroup = transform.parent.GetComponent<CanvasGroup>();
			_parentRectTransform = transform.parent.GetComponent<RectTransform>();
			_parentCanvasAlpha = _targetAlpha;
			_parentCanvasGroup.alpha = 0f;
		}

		protected override void Start()
		{
			base.Start();

			if (_maxRangeObject != null)
				_maxRange = Vector3.Distance(transform.position, _maxRangeObject.transform.position);
			else
				_maxRange = MaxRange;
		}

		protected override void Update()
		{
			if (!_isEnabled || !TouchUIController.Instance.IsTouching) return;

			_canvasGroup.alpha = PressedOpacity;

			// if we're in "screen space - camera" render mode
			if (ParentCanvasRenderMode == RenderMode.ScreenSpaceCamera)
			{
				_newTargetPosition = TargetCamera.ScreenToWorldPoint(TouchUIController.Instance.GetTouchPosition());
			}
			// otherwise
			else
			{
				_newTargetPosition = TouchUIController.Instance.GetTouchPosition();
			}

			Vector2 parentPosition = _parentRectTransform.transform.position;
			if (Vector2.Distance(_newTargetPosition, parentPosition) > _maxRange)
				_parentRectTransform.transform.position = _newTargetPosition + (parentPosition - _newTargetPosition).normalized * _maxRange;

			_neutralPosition = _parentRectTransform.transform.position;

			// We clamp the stick's position to let it move only inside its defined max range
			_newTargetPosition = Vector2.ClampMagnitude(_newTargetPosition - _neutralPosition, _maxRange);

			// If we haven't authorized certain axis, we force them to zero
			if (!HorizontalAxisEnabled)
			{
				_newTargetPosition.x = 0;
			}
			if (!VerticalAxisEnabled)
			{
				_newTargetPosition.y = 0;
			}
			// For each axis, we evaluate its lerped value (-1...1)
			_joystickValue.x = EvaluateInputValue(_newTargetPosition.x);
			_joystickValue.y = EvaluateInputValue(_newTargetPosition.y);

			_newJoystickPosition = _neutralPosition + _newTargetPosition;
			_newJoystickPosition.z = _initialZPosition;

			// We move the joystick to its dragged position
			transform.position = _newJoystickPosition;

			base.Update();
		}

		public void SetStatus(bool enabled)
		{
			_isEnabled = enabled;

			if (_isEnabled) Show();
			else Hide();
		}

		private void Show()
		{
			Vector2 touchPosition;
			// if we're in "screen space - camera" render mode
			if (ParentCanvasRenderMode == RenderMode.ScreenSpaceCamera)
			{
				touchPosition = TargetCamera.ScreenToWorldPoint(TouchUIController.Instance.GetTouchPosition());
			}
			// otherwise
			else
			{
				touchPosition = TouchUIController.Instance.GetTouchPosition();
			}

			transform.parent.transform.position = touchPosition;
			transform.localPosition = Vector3.zero;

			if (_isEnabled && TouchUIController.Instance.IsTouching)
				_parentCanvasGroup.alpha = _parentCanvasAlpha;
		}

		private void Hide()
		{
			_parentCanvasGroup.alpha = 0f;

			// we reset the stick's position
			_newJoystickPosition = _neutralPosition;
			_newJoystickPosition.z = _initialZPosition;
			transform.position = _newJoystickPosition;
			_joystickValue.x = 0f;
			_joystickValue.y = 0f;

			// we set its opacity back
			_canvasGroup.alpha = _initialOpacity;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			this.EventStartListening<TouchEvent>();
		}

		protected virtual void OnDisable()
		{
			this.EventStopListening<TouchEvent>();
		}

		public override void OnDrag(PointerEventData eventData)
		{
			base.OnDrag(eventData);
		}

		public void OnCGEvent(TouchEvent currentEvent)
		{
			if (currentEvent.NewState)
				Show();
			else
				Hide();
		}
	}
}
