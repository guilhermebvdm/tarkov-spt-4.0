using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using UnityEngine;

namespace MOAR.Components;

/// <summary>
/// Renders spawn point information for bot zones
///
/// Credits: DrakiaXYZ for the overlay code
/// </summary>
public class BotZoneRenderer : MonoBehaviour
{
    private List<BotZone> _botZones = [];
    private List<SpawnPointInfo> _spawnPointInfos = [];

    private StringBuilder _sb = new();
    private GUIStyle guiStyle;
    private float _screenScale = 1.0f;

    private static Player _player => Singleton<GameWorld>.Instance.MainPlayer;

    public void RefreshZones()
    {
        // This method is just lol, but works for this use case.
        _botZones = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<BotZone>().ToList();

        foreach (var zone in _botZones)
        {
            IterateSpawnPoints(zone);
        }
    }

    private void Awake()
    {
        // If DLSS or FSR are enabled, set a screen scale value
        if (CameraClass.Instance.SSAA.isActiveAndEnabled)
        {
            _screenScale =
                (float)CameraClass.Instance.SSAA.GetOutputWidth()
                / (float)CameraClass.Instance.SSAA.GetInputWidth();
            // Plugin.Log.LogDebug($"DLSS or FSR is enabled, scale screen offsets by {_screenScale}");
        }

        RefreshZones();
    }

    private void OnGUI()
    {
        if (guiStyle is null)
        {
            CreateGuiStyle();
        }

        foreach (var spawnPoint in _spawnPointInfos)
        {
            var pos = spawnPoint.Position;
            var dist = Mathf.RoundToInt(
                (spawnPoint.Position - _player.Transform.position).magnitude
            );

            if (spawnPoint.GUIContent.text.Length <= 0 || !(dist < 200f))
                continue;

            var screenPos = Camera.main!.WorldToScreenPoint(pos + (Vector3.up * 1.5f));

            // Skip points behind the camera.
            if (screenPos.z <= 0)
                continue;

            var guiSize = guiStyle.CalcSize(spawnPoint.GUIContent);
            spawnPoint.GUIRect.x = (screenPos.x * _screenScale) - (guiSize.x / 2);
            spawnPoint.GUIRect.y = Screen.height - ((screenPos.y * _screenScale) + guiSize.y);
            spawnPoint.GUIRect.size = guiSize;

            GUI.Box(spawnPoint.GUIRect, spawnPoint.GUIContent, guiStyle);
        }
    }

    private void OnDestroy()
    {
        foreach (var obj in _spawnPointInfos.ToArray())
        {
            _spawnPointInfos.Remove(obj);
        }
    }

    private void IterateSpawnPoints(BotZone zone)
    {
        var zoneColor = CreateRandomZoneColor();

        foreach (var spawnPoint in zone.SpawnPoints)
        {
            CreateSpawnPointInfo(spawnPoint, zone, zoneColor);
        }
    }

    private void CreateSpawnPointInfo(ISpawnPoint spawnPoint, BotZone zone, Color color)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = spawnPoint.Position;
        sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        var sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.material.color = color;

        var infoText = GetPointInfoText(spawnPoint, zone);

        var pointInfo = new SpawnPointInfo()
        {
            Position = spawnPoint.Position,
            ParentZone = zone,
            Sphere = sphere,
            GUIContent = new GUIContent() { text = infoText },
            GUIRect = new Rect(),
        };

        _spawnPointInfos.Add(pointInfo);
    }

    private string GetPointInfoText(ISpawnPoint spawnPoint, BotZone zone)
    {
        // Make sure we clear the string builder before trying to build a new point.
        _sb.Clear();

        // AppendLabeledValue("BotZone Id", zone.Id.ToString(), Color.gray, Color.green);
        AppendLabeledValue("BotZone Name", zone.ShortName, Color.gray, Color.green);
        AppendLabeledValue("Position", spawnPoint.Position.ToString(), Color.gray, Color.green);
        AppendLabeledValue("Side Mask", spawnPoint.Sides.ToString(), Color.gray, Color.green);
        // AppendLabeledValue("IsSniper", spawnPoint.IsSnipeZone.ToString(), Color.gray, Color.green);

        return _sb.ToString();
    }

    private void CreateGuiStyle()
    {
        guiStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 14, // TODO: Add config for font size
            margin = new RectOffset(3, 3, 3, 3),
            richText = true,
        };
    }

    private void AppendLabeledValue(
        string label,
        string data,
        Color labelColor,
        Color dataColor,
        bool labelEnabled = true
    )
    {
        var labelColorString = GetColorString(labelColor);
        var dataColorString = GetColorString(dataColor);

        AppendLabeledValue(label, data, labelColorString, dataColorString, labelEnabled);
    }

    private void AppendLabeledValue(
        string label,
        string data,
        string labelColor,
        string dataColor,
        bool labelEnabled = true
    )
    {
        if (labelEnabled)
        {
            _sb.AppendFormat("<color={0}>{1}:</color>", labelColor, label);
        }

        _sb.AppendFormat(" <color={0}>{1}</color>\n", dataColor, data);
    }

    private static string GetColorString(Color color)
    {
        if (color == Color.black)
            return "black";
        if (color == Color.white)
            return "white";
        if (color == Color.yellow)
            return "yellow";
        if (color == Color.red)
            return "red";
        if (color == Color.green)
            return "green";
        if (color == Color.blue)
            return "blue";
        if (color == Color.cyan)
            return "cyan";
        if (color == Color.magenta)
            return "magenta";
        if (color == Color.gray)
            return "gray";
        if (color == Color.clear)
            return "clear";
        return "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    private static Color CreateRandomZoneColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    private class SpawnPointInfo
    {
        public Vector3 Position;
        public BotZone ParentZone;
        public GameObject Sphere;
        public GUIContent GUIContent;
        public Rect GUIRect;
    }
}
