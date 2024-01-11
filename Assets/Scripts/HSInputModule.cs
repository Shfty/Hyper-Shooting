using System;

namespace UnityEngine.EventSystems
{
	[AddComponentMenu("Event/HS Input Module")]
	public class HSInputModule : PointerInputModule
	{
		private Vector2 m_LastMousePosition;
		private Vector2 m_MousePosition;
		
		InputManager.InputData m_currentInput {
			get {
				return InputManager.instance.CurrentInput;
			}
		}

		public override void UpdateModule()
		{
			m_LastMousePosition = m_MousePosition;
			m_MousePosition = Input.mousePosition;
		}
		
		public override bool IsModuleSupported()
		{
			return true;
		}
		
		public override bool ShouldActivateModule()
		{
			if (!base.ShouldActivateModule())
				return false;
			
			var shouldActivate = m_currentInput.select;
			shouldActivate |= m_currentInput.cancel;
			shouldActivate |= !Mathf.Approximately(m_currentInput.movement.x, 0.0f);
			shouldActivate |= !Mathf.Approximately(m_currentInput.movement.y, 0.0f);
			shouldActivate |= !Mathf.Approximately(m_currentInput.menuMovement.x, 0.0f);
			shouldActivate |= !Mathf.Approximately(m_currentInput.menuMovement.y, 0.0f);
			shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
			shouldActivate |= Input.GetMouseButtonDown(0);
			return shouldActivate;
		}
		
		public override void ActivateModule()
		{
			base.ActivateModule ();
			m_MousePosition = Input.mousePosition;
			m_LastMousePosition = Input.mousePosition;
			
			var toSelect = eventSystem.currentSelectedGameObject;
			if (toSelect == null)
				toSelect = eventSystem.lastSelectedGameObject;
			if (toSelect == null)
				toSelect = eventSystem.firstSelectedGameObject;
			
			eventSystem.SetSelectedGameObject (null, GetBaseEventData ());
			eventSystem.SetSelectedGameObject (toSelect, GetBaseEventData ());
		}
		
		public override void DeactivateModule()
		{
			base.DeactivateModule ();
			ClearSelection();
		}
		
		public override void Process()
		{
			bool usedEvent = SendUpdateEventToSelectedObject ();
			
			if (eventSystem.sendNavigationEvents)
			{
				if (!usedEvent)
					usedEvent |= SendMoveEventToSelectedObject ();
				
				if (!usedEvent)
					SendSubmitEventToSelectedObject ();
			}
			
			ProcessMouseEvent ();
		}

		// Key/Pad Handling
		bool prevSelect = false;
		bool prevCancel = false;
		private bool SendSubmitEventToSelectedObject()
		{
			if (eventSystem.currentSelectedGameObject == null)
				return false;

			var data = GetBaseEventData ();
			if (m_currentInput.select && !prevSelect)
				ExecuteEvents.Execute (eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
			
			if (m_currentInput.cancel && !prevCancel)
				ExecuteEvents.Execute (eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);

			prevSelect = m_currentInput.select;
			prevCancel = m_currentInput.cancel;
			
			return data.used;
		}

		float prevMag = 0.0f;
		private bool SendMoveEventToSelectedObject()
		{
			var mag = m_currentInput.movement.magnitude + m_currentInput.menuMovement.magnitude;

			if (!(mag >= 0.5f && prevMag < 0.5f)) {
				prevMag = mag;
				return false;
			}

			prevMag = mag;

			var axisEventData = GetAxisEventData (m_currentInput.movement.x + m_currentInput.menuMovement.x, m_currentInput.movement.z + m_currentInput.menuMovement.y, 0.0f);
			if (!Mathf.Approximately(axisEventData.moveVector.x, 0f)
			 || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
			{
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
			}
			
			return axisEventData.used;
		}
		
		// Mouse Handling
		private void ProcessMouseEvent()
		{
			var mouseData = GetMousePointerEventData();
			
			var pressed = mouseData.AnyPressesThisFrame();
			var released = mouseData.AnyReleasesThisFrame();
			
			var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;
			
			if (!UseMouse(pressed, released, leftButtonData.buttonData))
				return;
			
			// Process the first mouse button fully
			ProcessMousePress(leftButtonData);
			ProcessMove(leftButtonData.buttonData);
			ProcessDrag(leftButtonData.buttonData);
			
			// Now process right / middle clicks
			ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
			
			if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
			{
				var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
				ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
			}
		}
		
		private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
		{
			if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
				return true;
			
			return false;
		}
		
		private bool SendUpdateEventToSelectedObject()
		{
			if (eventSystem.currentSelectedGameObject == null)
				return false;
			
			var data = GetBaseEventData();
			ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
			return data.used;
		}
		
		private void ProcessMousePress(MouseButtonEventData data)
		{
			var pointerEvent = data.buttonData;
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;
			
			// PointerDown notification
			if (data.PressedThisFrame())
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
				
				DeselectIfSelectionChanged(currentOverGo, pointerEvent);
				
				// search for the control that will receive the press
				// if we can't find a press handler set the press
				// handler to be what would receive a click.
				var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
				
				// didnt find a press handler... search for a click handler
				if (newPressed == null)
					newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
				
				// Debug.Log("Pressed: " + newPressed);
				
				float time = Time.unscaledTime;
				
				if (newPressed == pointerEvent.lastPress)
				{
					var diffTime = time - pointerEvent.clickTime;
					if (diffTime < 0.3f)
						++pointerEvent.clickCount;
					else
						pointerEvent.clickCount = 1;
					
					pointerEvent.clickTime = time;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				
				pointerEvent.pointerPress = newPressed;
				pointerEvent.rawPointerPress = currentOverGo;
				
				pointerEvent.clickTime = time;
				
				// Save the drag handler as well
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
				
				if (pointerEvent.pointerDrag != null)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
			}
			
			// PointerUp notification
			if (data.ReleasedThisFrame())
			{
				// Debug.Log("Executing pressup on: " + pointer.pointerPress);
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
				
				// Debug.Log("KeyCode: " + pointer.eventData.keyCode);
				
				// see if we mouse up on the same element that we clicked on...
				var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
				
				// PointerClick and Drop events
				if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
				{
					ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				else if (pointerEvent.pointerDrag != null)
				{
					ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
				}
				
				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
				
				if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
				
				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;
				
				// redo pointer enter / exit to refresh state
				// so that if we moused over somethign that ignored it before
				// due to having pressed on something else
				// it now gets it.
				if (currentOverGo != pointerEvent.pointerEnter)
				{
					HandlePointerExitAndEnter(pointerEvent, null);
					HandlePointerExitAndEnter(pointerEvent, currentOverGo);
				}
			}
		}
	}
}