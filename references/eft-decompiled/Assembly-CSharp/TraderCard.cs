using System;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Diz.Binding;
using EFT;
using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TraderCard : UIElement, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public enum ETraderCardState
	{
		Locked,
		Available,
		Selected
	}

	[Serializable]
	public class TraderCardState
	{
		public ETraderCardState State;

		public TraderCardStateStyle[] SetStyles;

		public GameObject[] Enable;

		public GameObject[] Disable;
	}

	[Serializable]
	public class TraderCardStateStyle
	{
		public Color Color;

		public Graphic[] Graphic;
	}

	[CompilerGenerated]
	public class Class584
	{
		public Profile.TraderInfo trader;

		public TraderCard traderCard_0;

		public void method_0()
		{
			trader.OnAvailabilityChanged -= traderCard_0.method_0;
		}

		public void method_1()
		{
			trader.OnStandingChanged -= traderCard_0.method_0;
		}

		public void method_2()
		{
			trader.OnLoyaltyChanged -= traderCard_0.method_0;
		}
	}

	[CompilerGenerated]
	public class Class585
	{
		public ETraderCardState state;

		public bool method_0(TraderCardState currentState)
		{
			return currentState.State == state;
		}
	}

	[SerializeField]
	private TraderCardState[] _states;

	[SerializeField]
	private TraderAvatar _traderAvatar;

	[SerializeField]
	private LocalizedText _nickName;

	[SerializeField]
	private TMP_Text _standing;

	[SerializeField]
	private TMP_Text _timeLeft;

	[SerializeField]
	private RankPanel _rankPanel;

	[SerializeField]
	private Image _mouseOver;

	[SerializeField]
	private Image _borderOver;

	[SerializeField]
	private GameObject _questionMark;

	private DateTime dateTime_0;

	private TraderTooltip traderTooltip_0;

	private Profile.TraderInfo traderInfo_0;

	private readonly BindableEvent bindableEvent_0 = new BindableEvent();

	public IBindableEvent OnClick => bindableEvent_0;

	public void Awake()
	{
		HoverTrigger orAddComponent = GClass6.GetOrAddComponent<HoverTrigger>(_questionMark.gameObject);
		orAddComponent.OnHoverStart += delegate
		{
			traderTooltip_0.Show(traderInfo_0);
		};
		orAddComponent.OnHoverEnd += delegate
		{
			traderTooltip_0.Hide();
		};
	}

	public void Show(Profile.TraderInfo trader, AbstractQuestControllerClass questController, TraderTooltip tooltip)
	{
		traderInfo_0 = trader;
		traderTooltip_0 = tooltip;
		_traderAvatar.Show(trader, questController);
		trader.OnStandingChanged += method_0;
		trader.OnLoyaltyChanged += method_0;
		trader.OnAvailabilityChanged += method_0;
		UI.AddDisposable(delegate
		{
			trader.OnAvailabilityChanged -= method_0;
		});
		UI.AddDisposable(delegate
		{
			trader.OnStandingChanged -= method_0;
		});
		UI.AddDisposable(delegate
		{
			trader.OnLoyaltyChanged -= method_0;
		});
		UI.AddDisposable(_rankPanel);
		UI.AddDisposable(_traderAvatar);
		ApplyState(ETraderCardState.Locked);
		method_0();
	}

	public void OnDisable()
	{
		_mouseOver.DOKill();
		_mouseOver.color = GClass808.SetAlpha(_mouseOver.color, 0f);
		_borderOver.DOKill();
		_borderOver.color = GClass808.SetAlpha(_borderOver.color, 0f);
	}

	public void method_0()
	{
		_nickName.LocalizationKey = traderInfo_0.Settings.Nickname;
		_standing.text = traderInfo_0.Standing.ToString("N2");
		_rankPanel.Show(traderInfo_0.LoyaltyLevel, traderInfo_0.MaxLoyaltyLevel);
		SetCardState(traderInfo_0);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		bindableEvent_0.Invoke();
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPointerClick
		this.OnPointerClick(eventData);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (traderInfo_0.Available)
		{
			_mouseOver.DOKill();
			_mouseOver.DOColor(new Color(1f, 1f, 1f, 0.1f), 0.2f);
			_borderOver.DOKill();
			_borderOver.DOColor(new Color(1f, 1f, 1f, 0.6f), 0.2f);
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPointerEnter
		this.OnPointerEnter(eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (traderInfo_0.Available)
		{
			_mouseOver.DOKill();
			_mouseOver.DOColor(new Color(1f, 1f, 1f, 0f), 0.2f);
			_borderOver.DOKill();
			_borderOver.DOColor(new Color(1f, 1f, 1f, 0f), 0.2f);
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPointerExit
		this.OnPointerExit(eventData);
	}

	public void LateUpdate()
	{
		DateTime utcNow = EFTDateTimeClass.UtcNow;
		if (traderInfo_0 != null && (utcNow - dateTime_0).TotalSeconds >= 1.0)
		{
			dateTime_0 = utcNow;
			DateTime unixEpoch = EFTDateTimeClass.UnixEpoch;
			TimeSpan timeLeft = unixEpoch.AddSeconds(traderInfo_0.NextResupply) - utcNow;
			bool flag = timeLeft.Ticks <= 0L;
			GClass1673.SetMonospaceText(_timeLeft, (!flag) ? GClass1674.TraderFormat(timeLeft) : GClass1674.TraderFormat(TimeSpan.Zero));
		}
	}

	public void SetCardState(Profile.TraderInfo selectedTraderInfo)
	{
		if (traderInfo_0.Available)
		{
			ETraderCardState state = ((traderInfo_0 != selectedTraderInfo) ? ETraderCardState.Available : ETraderCardState.Selected);
			ApplyState(state);
		}
	}

	public void ApplyState(ETraderCardState state)
	{
		TraderCardState traderCardState = _states.First((TraderCardState currentState) => currentState.State == state);
		GameObject[] enable = traderCardState.Enable;
		for (int num = 0; num < enable.Length; num++)
		{
			enable[num].SetActive(value: true);
		}
		enable = traderCardState.Disable;
		for (int num = 0; num < enable.Length; num++)
		{
			enable[num].SetActive(value: false);
		}
		TraderCardStateStyle[] setStyles = traderCardState.SetStyles;
		foreach (TraderCardStateStyle traderCardStateStyle in setStyles)
		{
			Graphic[] graphic = traderCardStateStyle.Graphic;
			for (int num2 = 0; num2 < graphic.Length; num2++)
			{
				graphic[num2].color = traderCardStateStyle.Color;
			}
		}
	}

	public void method_1()
	{
		ApplyState(ETraderCardState.Locked);
	}

	public void method_2()
	{
		ApplyState(ETraderCardState.Available);
	}

	public void method_3()
	{
		ApplyState(ETraderCardState.Selected);
	}

	[CompilerGenerated]
	public void method_4(PointerEventData _)
	{
		traderTooltip_0.Show(traderInfo_0);
	}

	[CompilerGenerated]
	public void method_5(PointerEventData _)
	{
		traderTooltip_0.Hide();
	}
}
