namespace UnityEngine.UI.Extensions;

[ExecuteInEditMode]
public class SoftMask : MonoBehaviour
{
	private Material material_0;

	private Canvas canvas_0;

	public Shader ImageMaskShader;

	[Tooltip("The area that is to be used as the container.")]
	public RectTransform MaskArea;

	private RectTransform rectTransform_0;

	[Tooltip("A Rect Transform that can be used to scale and move the mask - Does not apply to Text UI Components being masked")]
	public RectTransform maskScalingRect;

	[Tooltip("Texture to be used to do the soft alpha")]
	public Texture AlphaMask;

	[Tooltip("At what point to apply the alpha min range 0-1")]
	[Range(0f, 1f)]
	public float CutOff;

	[Tooltip("Implement a hard blend based on the Cutoff")]
	public bool HardBlend;

	[Tooltip("Flip the masks alpha value")]
	public bool FlipAlphaMask;

	[Tooltip("If Mask Scaling Rect is given and this value is true, the area around the mask will not be clipped")]
	public bool DontClipMaskScalingRect;

	[Tooltip("If set to true, this mask is applied to all child Graphic objects belonging to this object.")]
	public bool CascadeToALLChildren;

	private Vector3[] vector3_0;

	private Vector2 vector2_0;

	private Vector2 vector2_1;

	private Vector2 vector2_2 = Vector2.one;

	private Vector2 vector2_3;

	private Vector2 vector2_4;

	private Vector2 vector2_5 = new Vector2(0.5f, 0.5f);

	private bool bool_0;

	private Rect rect_0;

	private Rect rect_1;

	private Vector2 vector2_6;

	private static readonly int int_0 = Shader.PropertyToID("_HardBlend");

	private static readonly int int_1 = Shader.PropertyToID("_Min");

	private static readonly int int_2 = Shader.PropertyToID("_Max");

	private static readonly int int_3 = Shader.PropertyToID("_FlipAlphaMask");

	private static readonly int int_4 = Shader.PropertyToID("_AlphaMask");

	private static readonly int int_5 = Shader.PropertyToID("_AlphaUV");

	private static readonly int int_6 = Shader.PropertyToID("_NoOuterClip");

	private static readonly int int_7 = Shader.PropertyToID("_CutOff");

	public void Start()
	{
		rectTransform_0 = base.transform as RectTransform;
		if (!MaskArea)
		{
			MaskArea = rectTransform_0;
		}
		if (ImageMaskShader == null)
		{
			ImageMaskShader = GClass872.Find("UI Extensions/SoftMask");
		}
		if (GetComponent<Graphic>() != null)
		{
			material_0 = new Material(ImageMaskShader);
			GetComponent<Graphic>().material = material_0;
		}
		if (CascadeToALLChildren)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				method_0(base.transform.GetChild(i));
			}
		}
		bool_0 = material_0 == null;
	}

	public void method_0(Transform t)
	{
		SoftMask softMask = t.gameObject.GetComponent<SoftMask>();
		if (softMask == null)
		{
			softMask = t.gameObject.AddComponent<SoftMask>();
		}
		softMask.MaskArea = MaskArea;
		softMask.AlphaMask = AlphaMask;
		softMask.CutOff = CutOff;
		softMask.HardBlend = HardBlend;
		softMask.FlipAlphaMask = FlipAlphaMask;
		softMask.maskScalingRect = maskScalingRect;
		softMask.DontClipMaskScalingRect = DontClipMaskScalingRect;
		softMask.CascadeToALLChildren = CascadeToALLChildren;
	}

	public void method_1()
	{
		Transform parent = base.transform;
		int num = 100;
		int num2 = 0;
		while (canvas_0 == null && num2 < num)
		{
			canvas_0 = parent.gameObject.GetComponent<Canvas>();
			if (canvas_0 == null)
			{
				parent = parent.parent;
			}
			num2++;
		}
	}

	public void Update()
	{
		method_2();
	}

	public void method_2()
	{
		if (!bool_0)
		{
			rect_0 = MaskArea.rect;
			rect_1 = rectTransform_0.rect;
			if (maskScalingRect != null)
			{
				rect_0 = maskScalingRect.rect;
			}
			if (maskScalingRect != null)
			{
				vector2_6 = rectTransform_0.transform.InverseTransformPoint(maskScalingRect.transform.TransformPoint(maskScalingRect.rect.center));
			}
			else
			{
				vector2_6 = rectTransform_0.transform.InverseTransformPoint(MaskArea.transform.TransformPoint(MaskArea.rect.center));
			}
			vector2_6 += (Vector2)rectTransform_0.transform.InverseTransformPoint(rectTransform_0.transform.position) - rectTransform_0.rect.center;
			vector2_0 = new Vector2(rect_0.width / rect_1.width, rect_0.height / rect_1.height);
			vector2_1 = vector2_6;
			vector2_2 = vector2_1;
			vector2_4 = new Vector2(rect_0.width, rect_0.height) * 0.5f;
			vector2_1 -= vector2_4;
			vector2_2 += vector2_4;
			vector2_1 = new Vector2(vector2_1.x / rect_1.width, vector2_1.y / rect_1.height) + vector2_5;
			vector2_2 = new Vector2(vector2_2.x / rect_1.width, vector2_2.y / rect_1.height) + vector2_5;
			material_0.SetFloat(int_0, HardBlend ? 1 : 0);
			material_0.SetVector(int_1, vector2_1);
			material_0.SetVector(int_2, vector2_2);
			material_0.SetInt(int_3, FlipAlphaMask ? 1 : 0);
			material_0.SetTexture(int_4, AlphaMask);
			material_0.SetVector(int_5, vector2_0);
			material_0.SetInt(int_6, (DontClipMaskScalingRect && maskScalingRect != null) ? 1 : 0);
			material_0.SetFloat(int_7, CutOff);
		}
	}
}
