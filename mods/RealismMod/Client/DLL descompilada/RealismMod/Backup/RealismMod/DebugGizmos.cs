// Decompiled with JetBrains decompiler
// Type: RealismMod.DebugGizmos
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class DebugGizmos
{
  public static bool DrawGizmos = PluginConfig.EnableGeneralLogging.Value;

  public class SingleObjects
  {
    public static GameObject Sphere(
      Vector3 position,
      float size,
      Color color,
      bool temporary = false,
      float expiretime = 1f)
    {
      if (!DebugGizmos.DrawGizmos)
        return (GameObject) null;
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 0);
      primitive.GetComponent<Renderer>().material.color = color;
      primitive.GetComponent<Collider>().enabled = false;
      primitive.transform.position = position;
      primitive.transform.localScale = new Vector3(size, size, size);
      if (temporary)
        DebugGizmos.TempCoroutine.DestroyAfterDelay(primitive, expiretime);
      return primitive;
    }

    public static GameObject Line(
      Vector3 startPoint,
      Vector3 endPoint,
      Color color,
      float lineWidth = 0.1f,
      bool temporary = false,
      float expiretime = 1f,
      bool taperLine = false)
    {
      if (!DebugGizmos.DrawGizmos)
        return (GameObject) null;
      GameObject gameObject = new GameObject();
      LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
      ((Renderer) lineRenderer).material.color = color;
      lineRenderer.startWidth = lineWidth;
      lineRenderer.endWidth = taperLine ? lineWidth / 4f : lineWidth;
      lineRenderer.SetPosition(0, startPoint);
      lineRenderer.SetPosition(1, endPoint);
      if (temporary)
        DebugGizmos.TempCoroutine.DestroyAfterDelay(gameObject, expiretime);
      return gameObject;
    }

    public static GameObject Ray(
      Vector3 startPoint,
      Vector3 direction,
      Color color,
      float length = 0.35f,
      float lineWidth = 0.1f,
      bool temporary = false,
      float expiretime = 1f,
      bool taperLine = false)
    {
      if (!DebugGizmos.DrawGizmos)
        return (GameObject) null;
      GameObject gameObject = new GameObject();
      LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
      ((Renderer) lineRenderer).material.color = color;
      lineRenderer.startWidth = lineWidth;
      lineRenderer.endWidth = taperLine ? lineWidth / 4f : lineWidth;
      lineRenderer.SetPosition(0, startPoint);
      lineRenderer.SetPosition(1, Vector3.op_Addition(startPoint, Vector3.op_Multiply(((Vector3) ref direction).normalized, length)));
      if (temporary)
        DebugGizmos.TempCoroutine.DestroyAfterDelay(gameObject, expiretime);
      return gameObject;
    }

    private static Color GetColor(string colliderName)
    {
      float num = PluginConfig.test2.Value;
      if (colliderName.Contains("_chest") || colliderName.Contains("_back") || colliderName.Contains("_side") || colliderName.ToLower() == "left" || colliderName.ToLower() == "right" || colliderName.ToLower() == "top")
        return Color.magenta;
      if (colliderName.Contains("SpineTopChest") || colliderName.Contains("SAPI_back"))
        return new Color(1f, 0.0f, 0.0f, num);
      if (colliderName.Contains("SpineLowerChest"))
        return new Color(0.0f, 1f, 0.0f, num);
      if (colliderName.Contains("PelvisBack"))
        return new Color(0.0f, 0.0f, 1f, num);
      if (colliderName.Contains("SideChestDown"))
        return new Color(0.5f, 1f, 0.0f, num);
      if (colliderName.Contains("SideChestUp"))
        return new Color(0.0f, 0.5f, 1f, num);
      if (colliderName.Contains("HumanSpine2"))
        return new Color(1f, 0.0f, 0.5f, num);
      if (colliderName.Contains("HumanSpine3"))
        return new Color(1f, 1f, 1f, num);
      if (colliderName.Contains("HumanPelvis"))
        return new Color(0.5f, 1f, 0.5f, num);
      if (colliderName.ToLower().Contains("leg"))
        return new Color(0.0f, 0.0f, 1f, num);
      if (colliderName.ToLower().Contains("arm"))
        return new Color(1f, 0.0f, 0.0f, num);
      if (colliderName.ToLower().Contains("eye"))
        return new Color(0.0f, 1f, 0.0f, num);
      if (colliderName.ToLower().Contains("jaw"))
        return new Color(0.0f, 1f, 0.0f, num);
      if (colliderName.ToLower().Contains("ear"))
        return new Color(1f, 1f, 1f, num);
      return colliderName.ToLower().Contains("head") ? new Color(1f, 0.0f, 0.0f, num) : new Color(0.0f, 0.0f, 1f, num);
    }

    public static void VisualizeSphereCollider(SphereCollider sphereCollider, string colliderName)
    {
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 0);
      Object.Destroy((Object) primitive.GetComponent<Collider>());
      Transform transform = ((Component) sphereCollider).transform;
      primitive.transform.position = transform.TransformPoint(sphereCollider.center);
      float num = sphereCollider.radius / 0.5f;
      Vector3 vector3;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3).\u002Ector(num, num, num);
      primitive.transform.localScale = Vector3.op_Multiply(Vector3.Scale(transform.localScale, vector3), PluginConfig.test1.Value);
      primitive.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"))
      {
        color = DebugGizmos.SingleObjects.GetColor(colliderName)
      };
      primitive.transform.SetParent(transform, true);
    }

    public static void VisualizeBoxCollider(BoxCollider boxCollider, string colliderName)
    {
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
      Object.Destroy((Object) primitive.GetComponent<Collider>());
      Transform transform = ((Component) boxCollider).transform;
      primitive.transform.position = transform.TransformPoint(boxCollider.center);
      primitive.transform.localScale = Vector3.op_Multiply(Vector3.Scale(transform.localScale, boxCollider.size), PluginConfig.test1.Value);
      primitive.transform.rotation = transform.rotation;
      primitive.GetComponent<Renderer>().material = new Material(Shader.Find("Transparent/Diffuse"))
      {
        color = DebugGizmos.SingleObjects.GetColor(colliderName)
      };
      primitive.transform.SetParent(transform, true);
    }

    public static void VisualizeCapsuleCollider(
      CapsuleCollider capsuleCollider,
      string colliderName)
    {
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 1);
      Object.Destroy((Object) primitive.GetComponent<Collider>());
      Transform transform = ((Component) capsuleCollider).transform;
      primitive.transform.position = transform.TransformPoint(capsuleCollider.center);
      float num1 = 2f;
      float num2 = 0.5f;
      float num3 = (capsuleCollider.height - 2f * capsuleCollider.radius) / num1;
      float num4 = capsuleCollider.radius / num2;
      Vector3 one = Vector3.one;
      Quaternion quaternion = Quaternion.identity;
      switch (capsuleCollider.direction)
      {
        case 0:
          // ISSUE: explicit constructor call
          ((Vector3) ref one).\u002Ector(num3, num4, num4);
          quaternion = Quaternion.Euler(0.0f, 0.0f, 90f);
          break;
        case 1:
          // ISSUE: explicit constructor call
          ((Vector3) ref one).\u002Ector(num4, num3, num4);
          break;
        case 2:
          // ISSUE: explicit constructor call
          ((Vector3) ref one).\u002Ector(num4, num4, num3);
          quaternion = Quaternion.Euler(90f, 0.0f, 0.0f);
          break;
      }
      primitive.transform.rotation = Quaternion.op_Multiply(transform.rotation, quaternion);
      primitive.transform.localScale = Vector3.op_Multiply(Vector3.Scale(transform.localScale, one), PluginConfig.test2.Value);
      primitive.GetComponent<Renderer>().material = new Material(Shader.Find("Transparent/Diffuse"))
      {
        color = DebugGizmos.SingleObjects.GetColor(colliderName)
      };
      primitive.transform.SetParent(transform, true);
    }
  }

  public class DrawLists
  {
    private static ManualLogSource Logger;
    private Color ColorA;
    private Color ColorB;
    private GameObject[] DebugObjects;

    public DrawLists(Color colorA, Color colorB, string LogName = "", bool randomColor = false)
    {
      LogName += "[Drawer]";
      if (randomColor)
      {
        this.ColorA = new Color(Random.value, Random.value, Random.value);
        this.ColorB = new Color(Random.value, Random.value, Random.value);
      }
      else
      {
        this.ColorA = colorA;
        this.ColorB = colorB;
      }
      DebugGizmos.DrawLists.Logger = Logger.CreateLogSource(LogName);
    }

    public void Draw(List<Vector3> list, bool destroy, float size = 0.1f, bool rays = false, float rayLength = 0.35f)
    {
      if (!DebugGizmos.DrawGizmos)
        this.DestroyDebug();
      else if (destroy)
      {
        this.DestroyDebug();
      }
      else
      {
        if (list.Count <= 0 || this.DebugObjects != null)
          return;
        DebugGizmos.DrawLists.Logger.LogWarning((object) $"Drawing {list.Count} Vector3s");
        this.DebugObjects = this.Create(list, size, rays, rayLength);
      }
    }

    public void Draw(Vector3[] array, bool destroy, float size = 0.1f, bool rays = false, float rayLength = 0.35f)
    {
      if (!DebugGizmos.DrawGizmos)
        this.DestroyDebug();
      else if (destroy)
      {
        this.DestroyDebug();
      }
      else
      {
        if (array.Length == 0 || this.DebugObjects != null)
          return;
        DebugGizmos.DrawLists.Logger.LogWarning((object) $"Drawing {array.Length} Vector3s");
        this.DebugObjects = this.Create(array, size, rays, rayLength);
      }
    }

    private GameObject[] Create(List<Vector3> list, float size = 0.1f, bool rays = false, float rayLength = 0.35f)
    {
      List<GameObject> gameObjectList = new List<GameObject>();
      foreach (Vector3 vector3 in list)
      {
        if (rays)
        {
          size *= Random.Range(0.5f, 1.5f);
          rayLength *= Random.Range(0.5f, 1.5f);
          GameObject gameObject = DebugGizmos.SingleObjects.Ray(vector3, Vector3.up, this.ColorA, rayLength, size);
          gameObjectList.Add(gameObject);
        }
        else
        {
          GameObject gameObject = DebugGizmos.SingleObjects.Sphere(vector3, size, this.ColorA);
          gameObjectList.Add(gameObject);
        }
      }
      return gameObjectList.ToArray();
    }

    private GameObject[] Create(Vector3[] array, float size = 0.1f, bool rays = false, float rayLength = 0.35f)
    {
      List<GameObject> gameObjectList = new List<GameObject>();
      foreach (Vector3 vector3 in array)
      {
        if (rays)
        {
          size *= Random.Range(0.5f, 1.5f);
          rayLength *= Random.Range(0.5f, 1.5f);
          GameObject gameObject = DebugGizmos.SingleObjects.Ray(vector3, Vector3.up, this.ColorA, rayLength, size);
          gameObjectList.Add(gameObject);
        }
        else
        {
          GameObject gameObject = DebugGizmos.SingleObjects.Sphere(vector3, size, this.ColorA);
          gameObjectList.Add(gameObject);
        }
      }
      return gameObjectList.ToArray();
    }

    private void DestroyDebug()
    {
      if (this.DebugObjects == null)
        return;
      foreach (Object debugObject in this.DebugObjects)
        Object.Destroy(debugObject);
      this.DebugObjects = (GameObject[]) null;
    }
  }

  public class Components
  {
    public static GameObject FollowLine(
      GameObject startObject,
      GameObject endObject,
      float lineWidth,
      Color color)
    {
      GameObject gameObject = new GameObject();
      LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
      ((Renderer) lineRenderer).material.color = color;
      lineRenderer.startWidth = lineWidth;
      lineRenderer.endWidth = lineWidth;
      lineRenderer.SetPosition(0, startObject.transform.position);
      lineRenderer.SetPosition(1, endObject.transform.position);
      DebugGizmos.Components.FollowLineScript followLineScript = gameObject.AddComponent<DebugGizmos.Components.FollowLineScript>();
      followLineScript.startObject = startObject;
      followLineScript.endObject = endObject;
      followLineScript.lineRenderer = lineRenderer;
      return gameObject;
    }

    public class FollowLineScript : MonoBehaviour
    {
      public GameObject startObject;
      public GameObject endObject;
      public LineRenderer lineRenderer;
      public float yOffset = 1f;

      private void Update()
      {
        this.lineRenderer.SetPosition(0, Vector3.op_Addition(this.startObject.transform.position, new Vector3(0.0f, this.yOffset, 0.0f)));
        this.lineRenderer.SetPosition(1, Vector3.op_Addition(this.endObject.transform.position, new Vector3(0.0f, this.yOffset, 0.0f)));
      }

      public void SetColor(Color color) => ((Renderer) this.lineRenderer).material.color = color;
    }
  }

  internal class TempCoroutine : MonoBehaviour
  {
    public static void DestroyAfterDelay(GameObject obj, float delay)
    {
      if (!Object.op_Inequality((Object) obj, (Object) null))
        return;
      new GameObject("TempCoroutineRunner").AddComponent<DebugGizmos.TempCoroutine.TempCoroutineRunner>().StartCoroutine(DebugGizmos.TempCoroutine.RunDestroyAfterDelay(obj, delay));
    }

    private static IEnumerator RunDestroyAfterDelay(GameObject obj, float delay)
    {
      yield return (object) new WaitForSeconds(delay);
      if (Object.op_Inequality((Object) obj, (Object) null))
        Object.Destroy((Object) obj);
      DebugGizmos.TempCoroutine.TempCoroutineRunner runner = obj?.GetComponentInParent<DebugGizmos.TempCoroutine.TempCoroutineRunner>();
      if (Object.op_Inequality((Object) runner, (Object) null))
        Object.Destroy((Object) ((Component) runner).gameObject);
    }

    internal class TempCoroutineRunner : MonoBehaviour
    {
    }
  }
}
