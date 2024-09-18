// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Utilities
{
	/// <summary>
	/// Debounce and timing utilities based on delta time.
	/// </summary>
	public struct DeltaTimer
	{
		private float mTimer;

		/// <summary>
		/// Time span of this timer. Once the internal
		/// clock hits zero, it resets to this.
		/// </summary>
		public float Seconds { get; set; }

		public DeltaTimer( float seconds )
		{
			Seconds = seconds;
			mTimer = seconds;
		}

		public void Update( float delta, Action actionOnTime )
		{
			mTimer -= delta;
			if ( mTimer <= 0.0f )
			{
				actionOnTime();
				mTimer = Seconds;
			}
		}
	}
}
