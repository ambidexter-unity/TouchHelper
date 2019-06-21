using UnityEngine;

namespace Common.TouchHelper
{
	public class TouchCreator
	{
		private Touch _touch;

		public float DeltaTime
		{
			get { return _touch.deltaTime; }
			set { _touch.deltaTime = value; }
		}

		public int TapCount
		{
			get { return _touch.tapCount; }
			set { _touch.tapCount = value; }
		}

		public TouchPhase Phase
		{
			get { return _touch.phase; }
			set { _touch.phase = value; }
		}

		public Vector2 DeltaPosition
		{
			get { return _touch.deltaPosition; }
			set { _touch.deltaPosition = value; }
		}

		public int FingerId
		{
			get { return _touch.fingerId; }
			set { _touch.fingerId = value; }
		}

		public Vector2 Position
		{
			get { return _touch.position; }
			set { _touch.position = value; }
		}

		public Vector2 RawPosition
		{
			get { return _touch.rawPosition; }
			set { _touch.rawPosition = value; }
		}

		public Touch Create()
		{
			return _touch;
		}

		public TouchCreator()
		{
			_touch = new Touch();
		}
	}
}