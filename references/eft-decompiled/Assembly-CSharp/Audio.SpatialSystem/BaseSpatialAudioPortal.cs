using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Audio.SpatialSystem;

[Serializable]
public abstract class BaseSpatialAudioPortal : MonoBehaviour, ISpatialPortal
{
	[Serializable]
	public enum PortalType
	{
		Opening,
		Static,
		Window
	}

	[Serializable]
	public enum PortalState : byte
	{
		Open,
		Closed,
		InProgressOpen,
		InProgressClose
	}

	private const byte CONNECTED_ROOMS_COUNT = 2;

	private const float MAX_ALLOWED_POSITION_OFFSET = 15f;

	[Tooltip("The level of portal transmission, 1 - full transmission like open portal, 0 - min transmission, like wall")]
	[Range(0f, 1f)]
	[SerializeField]
	private float _transmission;

	[Tooltip("Represent portal depth, it's affected for outdoor ambient audible")]
	public float Depth = 3f;

	[Tooltip("Optionally give the portal a unique name for more informative debug messages.")]
	public string portalName;

	[Tooltip("this function is used to try to adjust the size of the portal to the surrounding geometry")]
	public bool FitToGeometry;

	[Tooltip("experimental function to try to automatically set rotations for the portal")]
	public bool AutoRotate;

	public PortalType portalType;

	public PortalState state;

	public BoxCollider portalCollider;

	[Range(0f, 1f)]
	public float traversalMaxCost;

	public bool ToOutdoor;

	public AnimationCurve openEnvelope = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve closeEnvelope = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	[Range(0f, 10f)]
	public float openFadeTime = 0.1f;

	[Range(0f, 10f)]
	public float closeFadeTime = 0.1f;

	[SerializeField]
	private short _iD;

	[SerializeField]
	private List<SpatialAudioRoom> _connectedRooms = new List<SpatialAudioRoom>(2);

	private bool _inProgressOpen;

	private bool _inProgressClose;

	private Coroutine _interactionCoroutine;

	private PortalState _prevFinalState;

	public short ID => _iD;

	public float PortalClosureLevel { get; set; }

	public PortalState State
	{
		get
		{
			return state;
		}
		set
		{
			if (Application.isPlaying)
			{
				method_1(value);
			}
			state = value;
		}
	}

	public SpatialAudioRoom FrontRoom
	{
		get
		{
			List<SpatialAudioRoom> connectedRooms = _connectedRooms;
			if (connectedRooms == null || connectedRooms.Count <= 0)
			{
				return null;
			}
			return _connectedRooms[0];
		}
	}

	public SpatialAudioRoom BackRoom
	{
		get
		{
			List<SpatialAudioRoom> connectedRooms = _connectedRooms;
			if (connectedRooms == null || connectedRooms.Count <= 1)
			{
				return null;
			}
			return _connectedRooms[1];
		}
	}

	public void Awake()
	{
		_prevFinalState = state;
		if ((portalType == PortalType.Opening || portalType == PortalType.Window) && state == PortalState.Closed)
		{
			method_3(allowFade: false);
		}
		else
		{
			SetPortalOpen(allowFade: false);
		}
		method_0();
		method_2();
		OnAwake();
		Init().HandleExceptions();
	}

	public virtual void Subscribe()
	{
	}

	public virtual void OnAwake()
	{
		Subscribe();
	}

	public virtual Task Init()
	{
		SyncState();
		return Task.CompletedTask;
	}

	public virtual void SyncState()
	{
	}

	public bool IsPositionIdentical(Vector3 interactiveObjectPosition)
	{
		return Vector3.Distance(interactiveObjectPosition, base.transform.position) < 15f;
	}

	public void method_0()
	{
		portalName = (string.IsNullOrEmpty(portalName) ? base.gameObject.name : portalName);
	}

	public void method_1(PortalState newState)
	{
		if (_prevFinalState != newState && (newState == PortalState.Closed || newState == PortalState.Open))
		{
			GlobalEventHandlerClass.CreateEvent<GClass3568>().Invoke(ID, newState, PortalClosureLevel, Depth, traversalMaxCost);
			_prevFinalState = newState;
		}
	}

	public void method_2()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			component.enabled = false;
		}
	}

	public void SetPortalStatus(PortalState requiredState, bool allowFade)
	{
		if (requiredState == PortalState.Open)
		{
			SetPortalOpen(allowFade);
		}
		else
		{
			method_3(allowFade);
		}
	}

	public void method_3(bool allowFade)
	{
		State = PortalState.InProgressClose;
		float duration = (allowFade ? closeFadeTime : 0f);
		method_6();
		_interactionCoroutine = StartCoroutine(method_5(PortalClosureLevel, duration));
	}

	public void SetPortalOpen(bool allowFade)
	{
		State = PortalState.InProgressOpen;
		float duration = (allowFade ? openFadeTime : 0f);
		method_6();
		_interactionCoroutine = StartCoroutine(method_4(PortalClosureLevel, duration));
	}

	public void SetConnectedRoom(SpatialAudioRoom room)
	{
		if ((object)room != null && !_connectedRooms.Contains(room) && _connectedRooms.Count < 2)
		{
			_connectedRooms.Add(room);
		}
	}

	public List<SpatialAudioRoom> GetConnectedRooms()
	{
		return _connectedRooms;
	}

	public IEnumerator method_4(float start, float duration)
	{
		duration = PortalClosureLevel * duration;
		float num = 0f;
		_inProgressOpen = true;
		_inProgressClose = false;
		while (num < duration && !_inProgressClose)
		{
			num += Time.deltaTime;
			float time = Mathf.Clamp01(num / duration);
			float value = openEnvelope.Evaluate(time);
			value = Mathf.Clamp01(value);
			PortalClosureLevel = start - start * value;
			yield return null;
		}
		if (!_inProgressClose)
		{
			PortalClosureLevel = 0f;
		}
		_inProgressOpen = false;
		State = PortalState.Open;
	}

	public IEnumerator method_5(float start, float duration)
	{
		duration = (1f - PortalClosureLevel) * duration;
		float num = 0f;
		_inProgressClose = true;
		_inProgressOpen = false;
		while (num < duration && !_inProgressOpen)
		{
			num += Time.deltaTime;
			float time = Mathf.Clamp01(num / duration);
			float value = closeEnvelope.Evaluate(time);
			value = Mathf.Clamp01(value);
			float num2 = 1f - start;
			PortalClosureLevel = start + num2 * value;
			yield return null;
		}
		if (!_inProgressOpen)
		{
			PortalClosureLevel = Mathf.Lerp(1f, 0.1f, _transmission);
		}
		_inProgressClose = false;
		State = PortalState.Closed;
	}

	public void OnDestroy()
	{
		Unsubscribe();
		method_6();
	}

	public virtual void Unsubscribe()
	{
	}

	public void method_6()
	{
		if (_interactionCoroutine != null)
		{
			StopCoroutine(_interactionCoroutine);
			_interactionCoroutine = null;
		}
	}

	public Vector3 Center()
	{
		return base.transform.position;
	}

	public bool IsOpen()
	{
		return state == PortalState.Open;
	}

	public float GetPortalArea()
	{
		Vector2 portalSize = GetPortalSize();
		return portalSize.x * portalSize.y;
	}

	public Vector2 GetPortalSize()
	{
		Vector3 vector = Vector3.Scale(portalCollider.size, base.transform.localScale);
		return new Vector2(vector.x, vector.y);
	}

	public float2 GetPortalHalfSize()
	{
		Vector2 portalSize = GetPortalSize();
		return new float2(portalSize.x, portalSize.y) * 0.5f;
	}

	public BaseSpatialAudioPortal()
	{
	}
}
