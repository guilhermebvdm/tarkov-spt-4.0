using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiFlare;

[Serializable]
public class Flare
{
	[Tooltip("Размер выбранной флары - ширина и высота")]
	[SerializeField]
	[FormerlySerializedAs("Scale")]
	public Vector2 _scale;

	[Tooltip("Прозрачность выбранной флары")]
	[SerializeField]
	[FormerlySerializedAs("Alpha")]
	[Range(0f, 1f)]
	public float _alpha;

	[Tooltip("Цвет выбранной флары")]
	[SerializeField]
	[FormerlySerializedAs("Color")]
	public Color _color;

	[Tooltip("Тип выбранной флары. В зависимости от типа, флары могут иметь специфическую логику отображения или материал. Общие настройки каждого типа можно найти в FlareSceneSettings внутри скриптовой сцены. Более опдробно о типах флар можно узнать из документации")]
	[SerializeField]
	[FormerlySerializedAs("Type")]
	public FlareType _flareType;

	[Tooltip("Расстояние, которое определяет размер и прозрачность флары. Когда расстояние от камеры до флары больше или равно End, ее размер(Scale) будет умножен на MaxScale. Когда расстояние от камеры до флары меньше или равно Start, ее размер(Scale) будет умножен на MinScale. Промежуточные значения будут линейно интерполированные между этими граничными значениями. Аналогично будет вычислена прозрачность.")]
	[SerializeField]
	[FormerlySerializedAs("MinDist")]
	public float _minDist;

	[SerializeField]
	[FormerlySerializedAs("MaxDist")]
	public float _maxDist;

	[Tooltip("Диапазон изменения размера флары в зависимости от знвчения DistanceRange")]
	[SerializeField]
	[FormerlySerializedAs("MinScale")]
	public float _minScale;

	[SerializeField]
	[FormerlySerializedAs("MaxScale")]
	public float _maxScale;

	[Tooltip("Диапазон изменения прозрачности флары в зависимости от знвчения DistanceRange")]
	[SerializeField]
	[FormerlySerializedAs("MinAlpha")]
	public float _minAlpha;

	[SerializeField]
	[FormerlySerializedAs("MaxAlpha")]
	public float _maxAlpha;

	[Tooltip("Текстура флары. Все текстуры собранны в атлас. Значения атласа можно найти в FlareSceneSettings")]
	[SerializeField]
	[FormerlySerializedAs("TextureId")]
	public int _texId;

	public static string ScalePropertyName => "_scale";

	public static string AlphaPropertyName => "_alpha";

	public static string MinDistPropertyName => "_minDist";

	public static string MaxDistPropertyName => "_maxDist";

	public static string MinScalePropertyName => "_minScale";

	public static string MaxScalePropertyName => "_maxScale";

	public static string MinAlphaPropertyName => "_minAlpha";

	public static string MaxAlphaPropertyName => "_maxAlpha";

	public static string ColorPropertyName => "_color";

	public static string TexIdPropertyName => "_texId";

	public static string FlareTypePropertyName => "_flareType";

	public Vector2 Scale => _scale;

	public float Alpha => _alpha;

	public float MinDist => _minDist;

	public float MaxDist => _maxDist;

	public float MinScale => _minScale;

	public float MaxScale => _maxScale;

	public float MinAlpha => _minAlpha;

	public float MaxAlpha => _maxAlpha;

	public Color Color => _color;

	public FlareType FlareType => _flareType;

	public int TexId => _texId;
}
