using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Vintage")]
public class CC_Vintage : CC_LookupFilter
{
	public enum Filter
	{
		None,
		K506,
		Zabid,
		Cognac,
		Edwards,
		Cheese,
		LateGoose,
		Bread,
		Montreal,
		Feather,
		Jason,
		Fahrenheit,
		Owl,
		Chillwave,
		Albert,
		Bayswater,
		Atlanta,
		Felicity,
		Stefano,
		Boost,
		Emilia,
		Doze,
		Clifden,
		Blender,
		Tokyo,
		Walk,
		Olive,
		Hotshot
	}

	public Filter filter;

	protected Filter m_CurrentFilter;

	public override void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		if (filter != m_CurrentFilter)
		{
			m_CurrentFilter = filter;
			if (filter == Filter.None)
			{
				lookupTexture = null;
			}
			else
			{
				lookupTexture = GClass861.Load<Texture2D>("Instagram/" + filter);
			}
		}
		base.OnRenderImage(source, destination);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
