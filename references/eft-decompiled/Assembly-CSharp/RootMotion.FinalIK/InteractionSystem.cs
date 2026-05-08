using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RootMotion.FinalIK;

[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction System")]
public class InteractionSystem : MonoBehaviour
{
	public delegate void GDelegate49(FullBodyBipedEffector effectorType, InteractionObject interactionObject);

	public delegate void GDelegate50(FullBodyBipedEffector effectorType, InteractionObject interactionObject, InteractionObject.InteractionEvent interactionEvent);

	[Tooltip("If not empty, only the targets with the specified tag will be used by this Interaction System.")]
	public string targetTag = "";

	[Tooltip("The fade in time of the interaction.")]
	public float fadeInTime = 0.3f;

	[Tooltip("The master speed for all interactions.")]
	public float speed = 1f;

	[Tooltip("If > 0, lerps all the FBBIK channels used by the Interaction System back to their default or initial values when not in interaction.")]
	public float resetToDefaultsSpeed = 1f;

	[Header("Triggering")]
	[Tooltip("The collider that registers OnTriggerEnter and OnTriggerExit events with InteractionTriggers.")]
	[FormerlySerializedAs("collider")]
	public Collider characterCollider;

	[Tooltip("Will be used by Interaction Triggers that need the camera's position. Assign the first person view character camera.")]
	[FormerlySerializedAs("camera")]
	public Transform FPSCamera;

	[Tooltip("The layers that will be raycasted from the camera (along camera.forward). All InteractionTrigger look at target colliders should be included.")]
	public LayerMask camRaycastLayers;

	[Tooltip("Max distance of raycasting from the camera.")]
	public float camRaycastDistance = 1f;

	private List<InteractionTrigger> inContact = new List<InteractionTrigger>();

	private List<int> bestRangeIndexes = new List<int>();

	public GDelegate49 OnInteractionStart;

	public GDelegate49 OnInteractionPause;

	public GDelegate49 OnInteractionPickUp;

	public GDelegate49 OnInteractionResume;

	public GDelegate49 OnInteractionStop;

	public GDelegate50 OnInteractionEvent;

	public RaycastHit raycastHit;

	[Space(10f)]
	[Tooltip("Reference to the FBBIK component.")]
	[SerializeField]
	private FullBodyBipedIK fullBody;

	[Tooltip("Handles looking at the interactions.")]
	public InteractionLookAt lookAt = new InteractionLookAt();

	private InteractionEffector[] interactionEffectors = new InteractionEffector[9]
	{
		new InteractionEffector(FullBodyBipedEffector.Body),
		new InteractionEffector(FullBodyBipedEffector.LeftFoot),
		new InteractionEffector(FullBodyBipedEffector.LeftHand),
		new InteractionEffector(FullBodyBipedEffector.LeftShoulder),
		new InteractionEffector(FullBodyBipedEffector.LeftThigh),
		new InteractionEffector(FullBodyBipedEffector.RightFoot),
		new InteractionEffector(FullBodyBipedEffector.RightHand),
		new InteractionEffector(FullBodyBipedEffector.RightShoulder),
		new InteractionEffector(FullBodyBipedEffector.RightThigh)
	};

	private bool initiated;

	private Collider lastCollider;

	private Collider c;

	public bool inInteraction
	{
		get
		{
			if (!method_15(log: true))
			{
				return false;
			}
			int num = 0;
			while (true)
			{
				if (num < interactionEffectors.Length)
				{
					if (interactionEffectors[num].inInteraction && !interactionEffectors[num].isPaused)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
	}

	public FullBodyBipedIK ik => fullBody;

	public List<InteractionTrigger> triggersInRange { get; set; }

	[ContextMenu("TUTORIAL VIDEO (PART 1: BASICS)")]
	public void method_0()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=r5jiZnsDH3M");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 2: PICKING UP...)")]
	public void method_1()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=eP9-zycoHLk");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 3: ANIMATION)")]
	public void method_2()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
	public void method_3()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
	}

	[ContextMenu("Support Group")]
	public void method_4()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	public void method_5()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public bool IsInInteraction(FullBodyBipedEffector effectorType)
	{
		if (!method_15(log: true))
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		if (interactionEffectors[num].inInteraction)
		{
			return !interactionEffectors[num].isPaused;
		}
		return false;
	}

	public bool IsPaused(FullBodyBipedEffector effectorType)
	{
		if (!method_15(log: true))
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		if (interactionEffectors[num].inInteraction)
		{
			return interactionEffectors[num].isPaused;
		}
		return false;
	}

	public bool IsPaused()
	{
		if (!method_15(log: true))
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].inInteraction && interactionEffectors[num].isPaused)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public bool IsInSync()
	{
		if (!method_15(log: true))
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (!interactionEffectors[i].isPaused)
			{
				continue;
			}
			for (int j = 0; j < interactionEffectors.Length; j++)
			{
				if (j != i && interactionEffectors[j].inInteraction && !interactionEffectors[j].isPaused)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool StartInteraction(FullBodyBipedEffector effectorType, InteractionObject interactionObject, bool interrupt)
	{
		if (!method_15(log: true))
		{
			return false;
		}
		if (interactionObject == null)
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return interactionEffectors[num].Start(interactionObject, targetTag, fadeInTime, interrupt);
	}

	public bool PauseInteraction(FullBodyBipedEffector effectorType)
	{
		if (!method_15(log: true))
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return interactionEffectors[num].Pause();
	}

	public bool ResumeInteraction(FullBodyBipedEffector effectorType)
	{
		if (!method_15(log: true))
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return interactionEffectors[num].Resume();
	}

	public bool StopInteraction(FullBodyBipedEffector effectorType)
	{
		if (!method_15(log: true))
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return interactionEffectors[num].Stop();
	}

	public void PauseAll()
	{
		if (method_15(log: true))
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].Pause();
			}
		}
	}

	public void ResumeAll()
	{
		if (method_15(log: true))
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].Resume();
			}
		}
	}

	public void StopAll()
	{
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			interactionEffectors[i].Stop();
		}
	}

	public InteractionObject GetInteractionObject(FullBodyBipedEffector effectorType)
	{
		if (!method_15(log: true))
		{
			return null;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return interactionEffectors[num].interactionObject;
	}

	public float GetProgress(FullBodyBipedEffector effectorType)
	{
		if (!method_15(log: true))
		{
			return 0f;
		}
		int num = 0;
		while (true)
		{
			if (num < interactionEffectors.Length)
			{
				if (interactionEffectors[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return 0f;
		}
		return interactionEffectors[num].progress;
	}

	public float GetMinActiveProgress()
	{
		if (!method_15(log: true))
		{
			return 0f;
		}
		float num = 1f;
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].inInteraction)
			{
				float progress = interactionEffectors[i].progress;
				if (progress > 0f && progress < num)
				{
					num = progress;
				}
			}
		}
		return num;
	}

	public bool TriggerInteraction(int index, bool interrupt)
	{
		if (!method_15(log: true))
		{
			return false;
		}
		if (!method_16(index))
		{
			return false;
		}
		bool result = true;
		InteractionTrigger.Range range = triggersInRange[index].ranges[bestRangeIndexes[index]];
		for (int i = 0; i < range.interactions.Length; i++)
		{
			for (int j = 0; j < range.interactions[i].effectors.Length; j++)
			{
				if (!StartInteraction(range.interactions[i].effectors[j], range.interactions[i].interactionObject, interrupt))
				{
					result = false;
				}
			}
		}
		return result;
	}

	public bool TriggerInteraction(int index, bool interrupt, out InteractionObject interactionObject)
	{
		interactionObject = null;
		if (!method_15(log: true))
		{
			return false;
		}
		if (!method_16(index))
		{
			return false;
		}
		bool result = true;
		InteractionTrigger.Range range = triggersInRange[index].ranges[bestRangeIndexes[index]];
		for (int i = 0; i < range.interactions.Length; i++)
		{
			for (int j = 0; j < range.interactions[i].effectors.Length; j++)
			{
				interactionObject = range.interactions[i].interactionObject;
				if (!StartInteraction(range.interactions[i].effectors[j], interactionObject, interrupt))
				{
					result = false;
				}
			}
		}
		return result;
	}

	public bool TriggerInteraction(int index, bool interrupt, out InteractionTarget interactionTarget)
	{
		interactionTarget = null;
		if (!method_15(log: true))
		{
			return false;
		}
		if (!method_16(index))
		{
			return false;
		}
		bool result = true;
		InteractionTrigger.Range range = triggersInRange[index].ranges[bestRangeIndexes[index]];
		for (int i = 0; i < range.interactions.Length; i++)
		{
			for (int j = 0; j < range.interactions[i].effectors.Length; j++)
			{
				InteractionObject interactionObject = range.interactions[i].interactionObject;
				Transform target = interactionObject.GetTarget(range.interactions[i].effectors[j], base.tag);
				if (target != null)
				{
					interactionTarget = target.GetComponent<InteractionTarget>();
				}
				if (!StartInteraction(range.interactions[i].effectors[j], interactionObject, interrupt))
				{
					result = false;
				}
			}
		}
		return result;
	}

	public InteractionTrigger.Range GetClosestInteractionRange()
	{
		if (!method_15(log: true))
		{
			return null;
		}
		int closestTriggerIndex = GetClosestTriggerIndex();
		if (closestTriggerIndex >= 0 && closestTriggerIndex < triggersInRange.Count)
		{
			return triggersInRange[closestTriggerIndex].ranges[bestRangeIndexes[closestTriggerIndex]];
		}
		return null;
	}

	public InteractionObject GetClosestInteractionObjectInRange()
	{
		InteractionTrigger.Range closestInteractionRange = GetClosestInteractionRange();
		if (closestInteractionRange == null)
		{
			return null;
		}
		return closestInteractionRange.interactions[0].interactionObject;
	}

	public InteractionTarget GetClosestInteractionTargetInRange()
	{
		InteractionTrigger.Range closestInteractionRange = GetClosestInteractionRange();
		if (closestInteractionRange == null)
		{
			return null;
		}
		return closestInteractionRange.interactions[0].interactionObject.GetTarget(closestInteractionRange.interactions[0].effectors[0], this);
	}

	public InteractionObject[] GetClosestInteractionObjectsInRange()
	{
		InteractionTrigger.Range closestInteractionRange = GetClosestInteractionRange();
		if (closestInteractionRange == null)
		{
			return new InteractionObject[0];
		}
		InteractionObject[] array = new InteractionObject[closestInteractionRange.interactions.Length];
		for (int i = 0; i < closestInteractionRange.interactions.Length; i++)
		{
			array[i] = closestInteractionRange.interactions[i].interactionObject;
		}
		return array;
	}

	public InteractionTarget[] GetClosestInteractionTargetsInRange()
	{
		InteractionTrigger.Range closestInteractionRange = GetClosestInteractionRange();
		if (closestInteractionRange == null)
		{
			return new InteractionTarget[0];
		}
		List<InteractionTarget> list = new List<InteractionTarget>();
		InteractionTrigger.Range.Interaction[] interactions = closestInteractionRange.interactions;
		foreach (InteractionTrigger.Range.Interaction interaction in interactions)
		{
			FullBodyBipedEffector[] effectors = interaction.effectors;
			foreach (FullBodyBipedEffector effectorType in effectors)
			{
				list.Add(interaction.interactionObject.GetTarget(effectorType, this));
			}
		}
		return list.ToArray();
	}

	public bool TriggerEffectorsReady(int index)
	{
		if (!method_15(log: true))
		{
			return false;
		}
		if (!method_16(index))
		{
			return false;
		}
		for (int i = 0; i < triggersInRange[index].ranges.Length; i++)
		{
			InteractionTrigger.Range range = triggersInRange[index].ranges[i];
			for (int j = 0; j < range.interactions.Length; j++)
			{
				for (int k = 0; k < range.interactions[j].effectors.Length; k++)
				{
					if (IsInInteraction(range.interactions[j].effectors[k]))
					{
						return false;
					}
				}
			}
			for (int l = 0; l < range.interactions.Length; l++)
			{
				for (int m = 0; m < range.interactions[l].effectors.Length; m++)
				{
					if (!IsPaused(range.interactions[l].effectors[m]))
					{
						continue;
					}
					for (int n = 0; n < range.interactions[l].effectors.Length; n++)
					{
						if (n != m && !IsPaused(range.interactions[l].effectors[n]))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	public InteractionTrigger.Range GetTriggerRange(int index)
	{
		if (!method_15(log: true))
		{
			return null;
		}
		if (index >= 0 && index < bestRangeIndexes.Count)
		{
			return triggersInRange[index].ranges[bestRangeIndexes[index]];
		}
		GClass1465.Log("Index out of range.", base.transform);
		return null;
	}

	public int GetClosestTriggerIndex()
	{
		if (!method_15(log: true))
		{
			return -1;
		}
		if (triggersInRange.Count == 0)
		{
			return -1;
		}
		if (triggersInRange.Count == 1)
		{
			return 0;
		}
		int result = -1;
		float num = float.PositiveInfinity;
		for (int i = 0; i < triggersInRange.Count; i++)
		{
			if (triggersInRange[i] != null)
			{
				float num2 = Vector3.SqrMagnitude(triggersInRange[i].transform.position - base.transform.position);
				if (num2 < num)
				{
					result = i;
					num = num2;
				}
			}
		}
		return result;
	}

	public virtual void Start()
	{
		if (fullBody == null)
		{
			fullBody = GetComponent<FullBodyBipedIK>();
		}
		if (fullBody == null)
		{
			GClass1465.Log("InteractionSystem can not find a FullBodyBipedIK component", base.transform);
			return;
		}
		IKSolverFullBodyBiped solver = fullBody.solver;
		solver.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Combine(solver.OnPreUpdate, new IKSolver.GDelegate47(method_13));
		IKSolverFullBodyBiped solver2 = fullBody.solver;
		solver2.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Combine(solver2.OnPostUpdate, new IKSolver.GDelegate47(method_14));
		OnInteractionStart = (GDelegate49)Delegate.Combine(OnInteractionStart, new GDelegate49(method_9));
		OnInteractionPause = (GDelegate49)Delegate.Combine(OnInteractionPause, new GDelegate49(method_6));
		OnInteractionResume = (GDelegate49)Delegate.Combine(OnInteractionResume, new GDelegate49(method_7));
		OnInteractionStop = (GDelegate49)Delegate.Combine(OnInteractionStop, new GDelegate49(method_8));
		InteractionEffector[] array = interactionEffectors;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initiate(this);
		}
		triggersInRange = new List<InteractionTrigger>();
		c = GetComponent<Collider>();
		method_12();
		initiated = true;
	}

	public void method_6(FullBodyBipedEffector effector, InteractionObject interactionObject)
	{
		lookAt.isPaused = true;
	}

	public void method_7(FullBodyBipedEffector effector, InteractionObject interactionObject)
	{
		lookAt.isPaused = false;
	}

	public void method_8(FullBodyBipedEffector effector, InteractionObject interactionObject)
	{
		lookAt.isPaused = false;
	}

	public void method_9(FullBodyBipedEffector effector, InteractionObject interactionObject)
	{
		lookAt.Look(interactionObject.lookAtTarget, Time.time + interactionObject.length * 0.5f);
	}

	public void OnTriggerEnter(Collider c)
	{
		if (!(fullBody == null))
		{
			InteractionTrigger component = c.GetComponent<InteractionTrigger>();
			if (!inContact.Contains(component))
			{
				inContact.Add(component);
			}
		}
	}

	public void OnTriggerExit(Collider c)
	{
		if (!(fullBody == null))
		{
			InteractionTrigger component = c.GetComponent<InteractionTrigger>();
			inContact.Remove(component);
		}
	}

	public bool method_10(int index, out int bestRangeIndex)
	{
		bestRangeIndex = -1;
		if (!method_15(log: true))
		{
			return false;
		}
		if (index >= 0 && index < inContact.Count)
		{
			if (inContact[index] == null)
			{
				GClass1465.Log("The InteractionTrigger in the list 'inContact' has been destroyed", base.transform);
				return false;
			}
			bestRangeIndex = inContact[index].GetBestRangeIndex(base.transform, FPSCamera, raycastHit);
			if (bestRangeIndex == -1)
			{
				return false;
			}
			return true;
		}
		GClass1465.Log("Index out of range.", base.transform);
		return false;
	}

	public void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			if (fullBody == null)
			{
				fullBody = GetComponent<FullBodyBipedIK>();
			}
			if (characterCollider == null)
			{
				characterCollider = GetComponent<Collider>();
			}
		}
	}

	public void Update()
	{
		if (fullBody == null)
		{
			return;
		}
		method_12();
		method_11();
		triggersInRange.Clear();
		bestRangeIndexes.Clear();
		for (int i = 0; i < inContact.Count; i++)
		{
			int bestRangeIndex = -1;
			if (inContact[i] != null && inContact[i].gameObject.activeInHierarchy && inContact[i].enabled && method_10(i, out bestRangeIndex))
			{
				triggersInRange.Add(inContact[i]);
				bestRangeIndexes.Add(bestRangeIndex);
			}
		}
		lookAt.Update();
	}

	public void method_11()
	{
		if ((int)camRaycastLayers != -1 && !(FPSCamera == null))
		{
			Physics.Raycast(FPSCamera.position, FPSCamera.forward, out raycastHit, camRaycastDistance, camRaycastLayers);
		}
	}

	public void method_12()
	{
		if (characterCollider == null)
		{
			characterCollider = c;
		}
		if (characterCollider != null && characterCollider != c)
		{
			if (characterCollider.GetComponent<TriggerEventBroadcaster>() == null)
			{
				characterCollider.gameObject.AddComponent<TriggerEventBroadcaster>().target = base.gameObject;
			}
			if (lastCollider != null && lastCollider != c && lastCollider != characterCollider)
			{
				TriggerEventBroadcaster component = lastCollider.GetComponent<TriggerEventBroadcaster>();
				if (component != null)
				{
					UnityEngine.Object.Destroy(component);
				}
			}
		}
		lastCollider = characterCollider;
	}

	public void LateUpdate()
	{
		if (!(fullBody == null))
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].Update(base.transform, speed);
			}
			for (int j = 0; j < interactionEffectors.Length; j++)
			{
				interactionEffectors[j].ResetToDefaults(resetToDefaultsSpeed * speed);
			}
		}
	}

	public void method_13()
	{
		if (base.enabled && !(fullBody == null))
		{
			lookAt.SolveSpine();
		}
	}

	public void method_14()
	{
		if (base.enabled && !(fullBody == null))
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].OnPostFBBIK();
			}
			lookAt.SolveHead();
		}
	}

	public void OnDestroy()
	{
		if (!(fullBody == null))
		{
			IKSolverFullBodyBiped solver = fullBody.solver;
			solver.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Remove(solver.OnPreUpdate, new IKSolver.GDelegate47(method_13));
			IKSolverFullBodyBiped solver2 = fullBody.solver;
			solver2.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Remove(solver2.OnPostUpdate, new IKSolver.GDelegate47(method_14));
			OnInteractionStart = (GDelegate49)Delegate.Remove(OnInteractionStart, new GDelegate49(method_9));
			OnInteractionPause = (GDelegate49)Delegate.Remove(OnInteractionPause, new GDelegate49(method_6));
			OnInteractionResume = (GDelegate49)Delegate.Remove(OnInteractionResume, new GDelegate49(method_7));
			OnInteractionStop = (GDelegate49)Delegate.Remove(OnInteractionStop, new GDelegate49(method_8));
		}
	}

	public bool method_15(bool log)
	{
		if (fullBody == null)
		{
			if (log)
			{
				GClass1465.Log("FBBIK is null. Will not update the InteractionSystem", base.transform);
			}
			return false;
		}
		if (!initiated)
		{
			if (log)
			{
				GClass1465.Log("The InteractionSystem has not been initiated yet.", base.transform);
			}
			return false;
		}
		return true;
	}

	public bool method_16(int index)
	{
		if (index >= 0 && index < triggersInRange.Count)
		{
			if (triggersInRange[index] == null)
			{
				GClass1465.Log("The InteractionTrigger in the list 'inContact' has been destroyed", base.transform);
				return false;
			}
			return true;
		}
		GClass1465.Log("Index out of range.", base.transform);
		return false;
	}

	[ContextMenu("User Manual")]
	public void method_17()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	public void method_18()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_system.html");
	}
}
