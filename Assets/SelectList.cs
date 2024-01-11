using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/SelectList", 35)]
	[RequireComponent(typeof(RectTransform))]
	public class SelectList : Selectable, ISubmitHandler, ICanvasElement
	{
		[Serializable]
		public class SelectListChangedEvent : UnityEvent<int> { }

		[Serializable]
		public class SelectListSelectedEvent : UnityEvent { }
		
		[SerializeField]
		private Text m_Text;
		public Text text { get { return m_Text; } set { m_Text = value; UpdateVisuals(); } }

		[Space(6)]

		[SerializeField]
		public string[] m_keys;
		public string[] keys
		{
			get
			{
				return m_keys;
			}
			set
			{
				m_keys = value;
				ValidateIndex();
				UpdateVisuals();
			}
		}

		[Space(6)]

		[SerializeField]
		private int m_value = 0;
		public int value
		{
			get
			{
				return m_value;
			}
			set
			{
				Set(value, false);
			}
		}
		
		[Space(6)]
		
		// Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
		[SerializeField]
		private SelectListChangedEvent m_OnValueChanged = new SelectListChangedEvent();
		public SelectListChangedEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

		[SerializeField]
		private SelectListSelectedEvent m_OnSelected = new SelectListSelectedEvent();
		public SelectListSelectedEvent onSelected { get { return m_OnSelected; } set { m_OnSelected = value; } }
		
		
		protected SelectList()
		{ }

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();

			ValidateIndex ();

			Set(m_value, false);
			// Update rects since other things might affect them even if value didn't change.
			UpdateVisuals();
			
			var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
			if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
				CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
		}
		
		#endif // if UNITY_EDITOR

		void ValidateIndex()
		{
            if (m_keys != null)
            {
                if (m_value > m_keys.Length - 1)
                {
                    m_value = 0;
                }
                else if (m_value < 0)
                {
                    m_value = m_keys.Length - 1;
                }
            }
		}

		public virtual void Rebuild(CanvasUpdate executing)
		{
			#if UNITY_EDITOR
			if (executing == CanvasUpdate.Prelayout)
				onValueChanged.Invoke(value);
			#endif
		}
		
		protected override void OnEnable()
		{
			base.OnEnable();
			Set (m_value, false);
			// Update rects since they need to be initialized correctly.
			UpdateVisuals();
		}
		
		// Set the valueUpdate the visible Image.
		void Set(int input)
		{
			Set(input, true);
		}
		
		void Set(int input, bool sendCallback)
		{
			m_value = input;

			ValidateIndex();
			UpdateVisuals();

			if (sendCallback)
				m_OnValueChanged.Invoke(m_value);
		}

		// Force-update the slider. Useful if you've changed the properties and want it to update visually.
		private void UpdateVisuals()
		{
			if (m_Text != null)
			{
				if( m_keys.Length > 0 )
				{
					m_Text.text = m_keys[m_value];
				}
				else
				{
					m_Text.text = "";
				}
			}
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);

			Camera camera = GameObject.FindObjectOfType<Camera> ();
			float relativePos = RectTransformUtility.WorldToScreenPoint( camera, transform.position ).x - eventData.pressPosition.x;

			if (relativePos < 0.0f) {
				Set ( m_value + 1 );
			}
			else if (relativePos > 0.0f) {
				Set ( m_value - 1 );
			}
			
			ValidateIndex();
			UpdateVisuals ();
		}
		
		public override void OnMove(AxisEventData eventData)
		{
			if (!IsActive() || !IsInteractable())
			{
				base.OnMove(eventData);
				return;
			}

			bool highlight = false;

			switch (eventData.moveDir)
			{
			case MoveDirection.Left:
				Set ( m_value - 1 );
				highlight = true;
				break;
			case MoveDirection.Right:
				Set ( m_value + 1 );
				highlight = true;
				break;
			default:
					base.OnMove(eventData);
				break;
			}

			if (highlight) {
				DoStateTransition (SelectionState.Pressed, false);
				StartCoroutine (OnFinishSubmit ());
			}

			ValidateIndex();
			UpdateVisuals ();
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
			if (!IsActive() || !IsInteractable())
				return;
			
			onSelected.Invoke();
			
			// if we get set disabled during the press
			// don't run the coroutine.
			if (!IsActive() || !IsInteractable())
				return;
			
			DoStateTransition(SelectionState.Pressed, false);
			StartCoroutine(OnFinishSubmit());
		}

		private IEnumerator OnFinishSubmit()
		{
			var fadeTime = colors.fadeDuration;
			var elapsedTime = 0f;
			
			while (elapsedTime < fadeTime)
			{
				elapsedTime += Time.unscaledDeltaTime;
				yield return null;
			}
			
			DoStateTransition(currentSelectionState, false);
		}
		
		public override Selectable FindSelectableOnLeft()
		{
			return null;
		}
		
		public override Selectable FindSelectableOnRight()
		{
			return null;
		}

        public void LayoutComplete()
        {
            throw new NotImplementedException();
        }

        public void GraphicUpdateComplete()
        {
            throw new NotImplementedException();
        }
    }
}