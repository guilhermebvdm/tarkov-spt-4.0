using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GClass1913
{
	public static void SafeOnItemAddedInvoke(this IOnItemAdded handler, GEventArgs2 obj)
	{
		try
		{
			handler.OnItemAdded(obj);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnItemAddedInvoke");
		}
	}

	public static void SafeOnItemRemovedInvoke(this IOnItemRemoved handler, GEventArgs3 obj)
	{
		try
		{
			handler.OnItemRemoved(obj);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnItemRemovedInvoke");
		}
	}

	public static void SafeOnMagazineChangeInvoke(this GInterface182 handler, MagazineItemClass magazine)
	{
		try
		{
			handler.OnMagazineChange(magazine);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnMagazineChangeInvoke");
		}
	}

	public static void SafeOnLoadMagazineInvoke(this GInterface183 handler, GEventArgs7 args)
	{
		try
		{
			handler.OnLoadMagazine(args);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnLoadMagazineInvoke");
		}
	}

	public static void SafeOnUnloadMagazineInvoke(this GInterface184 handler, GEventArgs8 args)
	{
		try
		{
			handler.OnUnloadMagazine(args);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnUnloadMagazineInvoke");
		}
	}

	public static void SafeOnInventoryMagazineCheckInvoke(this GInterface185 handler, MagazineItemClass item, float time, bool status)
	{
		try
		{
			handler.OnInventoryMagazineCheck(item, time, status);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnInventoryMagazineCheckInvoke");
		}
	}

	public static void SafeOnRefreshItemInvoke(this GInterface186 handler, GEventArgs18 args)
	{
		try
		{
			handler.OnRefreshItem(args);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnRefreshItemInvoke");
		}
	}

	public static void SafeOnDrainInvoke(this GInterface187 handler, GEventArgs13 args)
	{
		try
		{
			handler.OnDrain(args);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnDrainInvoke");
		}
	}

	public static void SafeOnBindItemInvoke(this GInterface188 handler, GEventArgs11 args)
	{
		try
		{
			handler.OnBindItem(args);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnBindItemInvoke");
		}
	}

	public static void SafeOnUnbindItemInvoke(this GInterface189 handler, GEventArgs12 args)
	{
		try
		{
			handler.OnUnbindItem(args);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnUnbindItemInvoke");
		}
	}

	public static void SafeOnSetInHandsInvoke(this IOnSetInHands handler, GEventArgs9 args)
	{
		try
		{
			handler.OnSetInHands(args);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnSetInHandsInvoke");
		}
	}

	public static void SafeOnRemoveFromHandsInvoke(this GInterface191 handler, GEventArgs10 args)
	{
		try
		{
			handler.OnRemoveFromHands(args);
		}
		catch (Exception e)
		{
			smethod_0(handler, e, "SafeOnRemoveFromHandsInvoke");
		}
	}

	public static void smethod_0<T>(T handler, Exception e, [CallerMemberName] string methodName = "UNKNOWN")
	{
		Debug.LogErrorFormat("{0}: {1}, can't process {2}:{3}", typeof(T).Name, handler, methodName, e);
	}
}
