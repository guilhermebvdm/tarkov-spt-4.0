using UnityEngine;

namespace Audio.Effects;

public abstract class AudioEQFilterBase : MonoBehaviour, GInterface84, GInterface69
{
	[SerializeField]
	protected float lerpSpeed = 5f;

	[SerializeField]
	protected float shelfFilterQ = 0.707f;

	protected float _targetHpf;

	protected float _targetLpf;

	protected float _targetHpfQ;

	protected float _targetLpfQ;

	protected float _currentHpf;

	protected float _currentLpf;

	protected float _currentHpfQ;

	protected float _currentLpfQ;

	protected bool _isInitialized;

	protected bool _highPassEnabled = true;

	protected bool _lowPassEnabled = true;

	public void Init()
	{
		InitializeComponents();
		_targetHpfQ = (_currentHpfQ = shelfFilterQ);
		_targetLpfQ = (_currentLpfQ = shelfFilterQ);
		method_0();
		_isInitialized = true;
	}

	public void SetTargetLowPassSettings(float targetFrequency, float targetQ, bool applyImmediately = false)
	{
		if (base.enabled && _isInitialized)
		{
			_targetLpf = targetFrequency;
			_targetLpfQ = targetQ;
			if (applyImmediately)
			{
				_currentLpf = _targetLpf;
				_currentLpfQ = _targetLpfQ;
				ApplyLowPass(_currentLpf, _currentLpfQ);
			}
		}
	}

	public void SetTargetHighPassSettings(float targetFrequency, float targetQ, bool applyImmediately = false)
	{
		if (base.enabled && _isInitialized)
		{
			_targetHpf = targetFrequency;
			_targetHpfQ = targetQ;
			if (applyImmediately)
			{
				_currentHpf = _targetHpf;
				_currentHpfQ = _targetHpfQ;
				ApplyHighPass(_currentHpf, _currentHpfQ);
			}
		}
	}

	public void method_0()
	{
		if (_highPassEnabled)
		{
			ApplyHighPass(_currentHpf, _currentHpfQ);
		}
		if (_lowPassEnabled)
		{
			ApplyLowPass(_currentLpf, _currentLpfQ);
		}
	}

	public void ResetValues()
	{
		float targetHpf = 20f;
		_currentHpf = 20f;
		_targetHpf = targetHpf;
		_targetHpfQ = (_currentHpfQ = shelfFilterQ);
		targetHpf = 22000f;
		_currentLpf = 22000f;
		_targetLpf = targetHpf;
		_targetLpfQ = (_currentLpfQ = shelfFilterQ);
		method_0();
	}

	public void SetActiveHighPass(bool active)
	{
		_highPassEnabled = active;
		OnHighPassEnabled(active);
	}

	public void SetActiveLowPass(bool active)
	{
		_lowPassEnabled = active;
		OnLowPassEnabled(active);
	}

	public void SetActive(bool active)
	{
		OnSetActive(active);
	}

	public void ManualUpdate()
	{
		if (base.enabled && _isInitialized)
		{
			float deltaTime = Time.deltaTime;
			bool flag = false;
			if (!Mathf.Approximately(_currentHpf, _targetHpf))
			{
				_currentHpf = Mathf.Lerp(_currentHpf, _targetHpf, deltaTime * lerpSpeed);
				flag = true;
			}
			if (!Mathf.Approximately(_currentHpfQ, _targetHpfQ))
			{
				_currentHpfQ = Mathf.Lerp(_currentHpfQ, _targetHpfQ, deltaTime * lerpSpeed);
				flag = true;
			}
			if (!Mathf.Approximately(_currentLpf, _targetLpf))
			{
				_currentLpf = Mathf.Lerp(_currentLpf, _targetLpf, deltaTime * lerpSpeed);
				flag = true;
			}
			if (!Mathf.Approximately(_currentLpfQ, _targetLpfQ))
			{
				_currentLpfQ = Mathf.Lerp(_currentLpfQ, _targetLpfQ, deltaTime * lerpSpeed);
				flag = true;
			}
			if (flag)
			{
				method_0();
			}
			OnUpdate();
		}
	}

	public virtual void OnUpdate()
	{
	}

	public abstract void InitializeComponents();

	public abstract void ApplyHighPass(float frequency, float q);

	public abstract void ApplyLowPass(float frequency, float q);

	public abstract void OnHighPassEnabled(bool targetState);

	public abstract void OnLowPassEnabled(bool targetState);

	public abstract void OnSetActive(bool active);

	public AudioEQFilterBase()
	{
	}
}
