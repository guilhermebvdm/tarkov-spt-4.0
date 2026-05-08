using System;
using UnityEngine;

namespace MirzaBeig.Shaders.ImageEffects;

[Serializable]
[ExecuteInEditMode]
public class IEBase : MonoBehaviour
{
	private Material _material;

	private Camera _camera;

	public Material material
	{
		get
		{
			if (!_material)
			{
				_material = new Material(shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
			return _material;
		}
	}

	public Shader shader { get; set; }

	public Camera camera
	{
		get
		{
			if (!_camera)
			{
				_camera = GetComponent<Camera>();
			}
			return _camera;
		}
	}

	public void Awake()
	{
	}

	public void Start()
	{
	}

	public void Update()
	{
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
	}

	public void blit(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);
	}

	public void OnDisable()
	{
		if ((bool)_material)
		{
			UnityEngine.Object.DestroyImmediate(_material);
		}
	}
}
