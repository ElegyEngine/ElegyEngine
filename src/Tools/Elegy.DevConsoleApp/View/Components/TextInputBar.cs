// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Spectre.Console;
using Spectre.Console.Rendering;

namespace Elegy.DevConsoleApp.View.Components
{
	internal class TextInputBar : IRenderable
	{
		private Style mNoInputStyle = new Style( Color.Grey );
		private int mMilliseconds = 1000;
		private bool mCursorState = false;

		public string Text { get; set; } = string.Empty;
		public int Cursor { get; set; } = 0;
		public int BlinkRate { get; set; } = 4;

		public void Push( char character )
		{
			Text = Text.Insert( Cursor, $"{character}" );
			Cursor++;
		}

		public void AdvanceTime( int milliseconds )
		{
			int lastChunk = mMilliseconds / BlinkRate;
			mMilliseconds -= milliseconds;
			if ( mMilliseconds < 0 )
			{
				mMilliseconds = 1000 + mMilliseconds;
			}

			int currentChunk = mMilliseconds / BlinkRate;

			if ( lastChunk != currentChunk )
			{
				mCursorState = !mCursorState;
			}
		}

		public void CursorAdvance()
		{
			Cursor = Math.Min( Text.Length - 1, Cursor + 1 );
		}

		public void CursorUnadvance()
		{
			Cursor = Math.Max( 0, Cursor - 1 );
		}

		public void ProcessEvent( ConsoleKeyInfo key )
		{
			if ( (key.Key >= ConsoleKey.A && key.Key <= ConsoleKey.Z)
						|| (key.Key >= ConsoleKey.D0 && key.Key <= ConsoleKey.D9)
						|| key.Key == ConsoleKey.Spacebar )
			{
				Push( key.KeyChar );
			}
			else if ( key.Key == ConsoleKey.LeftArrow )
			{
				CursorUnadvance();
			}
			else if ( key.Key == ConsoleKey.RightArrow )
			{
				CursorAdvance();
			}
		}

		public Measurement Measure( RenderOptions options, int maxWidth )
		{
			return new Measurement( 0, maxWidth );
		}

		public IEnumerable<Segment> Render( RenderOptions options, int maxWidth )
		{
			string text = "Type a command here...";
			if ( Text != string.Empty )
			{
				text = Text;
			}

			if ( Text == string.Empty )
			{
				return [new Segment( "Type a command here...", mNoInputStyle )];
			}

			return [new Segment( Text.Substring( 0, Math.Min( Text.Length, maxWidth ) ) )];
		}
	}
}
