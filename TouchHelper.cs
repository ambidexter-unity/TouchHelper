using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Common.TouchHelper
{
	/// <summary>
	/// Вспомогательный класс для имитации тачей в Unity editor-е.
	/// </summary>
	public static class TouchHelper
	{
		private static readonly List<RaycastResult> Res = new List<RaycastResult>();

		private static readonly TouchCreator LastFakeTouch = new TouchCreator();

		private static readonly TouchCreator ZoomGestureTouch1 = new TouchCreator();
		private static readonly TouchCreator ZoomGestureTouch2 = new TouchCreator();

		private static readonly Touch EmptyTouch = new Touch();

		private static int _lockerId;
		private static readonly List<int> Lockers = new List<int>();

		public static int Lock()
		{
			var id = ++_lockerId;
			Lockers.Add(id);
			return id;
		}

		public static void Unlock(int id)
		{
			Lockers.Remove(id);
		}

		public static bool IsLocked => Lockers.Any();

		public static Touch[] GetTouches()
		{
			if (IsLocked)
			{
				return new Touch[0];
			}

			return MakeFakeTouch(out var isZoomGesture)
				? isZoomGesture
					? new[] {ZoomGestureTouch1.Create(), ZoomGestureTouch2.Create()}
					: new[] {LastFakeTouch.Create()}
				: Input.touches;
		}

		public static bool GetTouch(out Touch touch, int touchNum = 0, bool ignoreLockers = false)
		{
			if (!ignoreLockers && IsLocked)
			{
				touch = EmptyTouch;
				return false;
			}

			if (Input.touchCount > touchNum)
			{
				touch = Input.GetTouch(touchNum);
				return true;
			}

			if (touchNum == 0 && MakeFakeTouch(out _))
			{
				touch = LastFakeTouch.Create();
				return true;
			}

			touch = EmptyTouch;
			return false;
		}

		public static bool CheckUiTouches(GameObject target)
		{
			Assert.IsNotNull(target);

			var ped = new PointerEventData(EventSystem.current)
			{
				position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2) Input.mousePosition
			};
			Res.Clear();
			EventSystem.current.RaycastAll(ped, Res);
			if (Res.Count <= 0) return false;

			var targetIsUnlocked = Res
				.Select(result => result.gameObject.GetComponent<ILocker>())
				.Where(locker => locker != null)
				.Any(locker => locker.UnlockedObjects.Contains(target));


			if (targetIsUnlocked)
			{
				DebugConditional.LogFormat("Object {0} was detected as unlocked.", target.name);
				return true;
			}

			return !IsLocked;
		}

		public static bool IsPointerOverGameObject()
		{
#if UNITY_EDITOR
			return EventSystem.current.IsPointerOverGameObject();
#else
			var ped = new PointerEventData(EventSystem.current)
			{
				position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2) Input.mousePosition
			};
			Res.Clear();
			EventSystem.current.RaycastAll(ped, Res);
			return Res.Count > 0;
#endif
		}

		private static bool MakeFakeTouch(out bool isZoomGesture)
		{
			isZoomGesture = false;
			if (Input.touchSupported) return false;

			if (Input.GetMouseButtonDown(0))
			{
				LastFakeTouch.Phase = TouchPhase.Began;
				LastFakeTouch.DeltaPosition = Vector2.zero;
				LastFakeTouch.Position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				LastFakeTouch.FingerId = 0;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				LastFakeTouch.Phase = TouchPhase.Ended;
				var newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				LastFakeTouch.DeltaPosition = newPosition - LastFakeTouch.Position;
				LastFakeTouch.Position = newPosition;
				LastFakeTouch.FingerId = 0;
			}
			else if (Input.GetMouseButton(0))
			{
				LastFakeTouch.Phase = TouchPhase.Moved;
				var newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				LastFakeTouch.DeltaPosition = newPosition - LastFakeTouch.Position;
				LastFakeTouch.Position = newPosition;
				LastFakeTouch.FingerId = 0;
			}
			else if (Input.GetMouseButtonDown(1))
			{
				isZoomGesture = true;

				var p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				ZoomGestureTouch1.Phase = TouchPhase.Began;
				ZoomGestureTouch1.DeltaPosition = Vector2.zero;
				ZoomGestureTouch1.Position = p;
				ZoomGestureTouch1.FingerId = 0;

				ZoomGestureTouch2.Phase = TouchPhase.Began;
				ZoomGestureTouch2.DeltaPosition = Vector2.zero;
				ZoomGestureTouch2.Position = Mirror(p);
				ZoomGestureTouch2.FingerId = 1;
			}
			else if (Input.GetMouseButtonUp(1))
			{
				isZoomGesture = true;

				var p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				ZoomGestureTouch1.Phase = TouchPhase.Ended;
				ZoomGestureTouch1.DeltaPosition = p - ZoomGestureTouch1.Position;
				ZoomGestureTouch1.Position = p;
				ZoomGestureTouch1.FingerId = 0;

				p = Mirror(p);
				ZoomGestureTouch2.Phase = TouchPhase.Ended;
				ZoomGestureTouch2.DeltaPosition = p - ZoomGestureTouch2.Position;
				ZoomGestureTouch2.Position = p;
				ZoomGestureTouch2.FingerId = 1;
			}
			else if (Input.GetMouseButton(1))
			{
				isZoomGesture = true;

				var p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				ZoomGestureTouch1.Phase = TouchPhase.Moved;
				ZoomGestureTouch1.DeltaPosition = p - ZoomGestureTouch1.Position;
				ZoomGestureTouch1.Position = p;
				ZoomGestureTouch1.FingerId = 0;

				p = Mirror(p);
				ZoomGestureTouch2.Phase = TouchPhase.Moved;
				ZoomGestureTouch2.DeltaPosition = p - ZoomGestureTouch2.Position;
				ZoomGestureTouch2.Position = p;
				ZoomGestureTouch2.FingerId = 1;
			}
			else
			{
				return false;
			}

			return true;
		}

		private static Vector2 Mirror(Vector2 pos)
		{
			var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
			return center + (pos - center) * -1f;
		}
	}
}