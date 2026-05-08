using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarButtons : MonoBehaviour
{
	[CompilerGenerated]
	public class Class586
	{
		public Scrollbar scrollbar;

		public void method_0()
		{
			scrollbar.value += 0.1f;
		}

		public void method_1()
		{
			scrollbar.value -= 0.1f;
		}
	}

	[SerializeField]
	private Button _positiveButton;

	[SerializeField]
	private Button _negativeButton;

	public void Start()
	{
		Scrollbar scrollbar = GetComponent<Scrollbar>();
		_positiveButton.onClick.AddListener(delegate
		{
			scrollbar.value += 0.1f;
		});
		_negativeButton.onClick.AddListener(delegate
		{
			scrollbar.value -= 0.1f;
		});
	}
}
