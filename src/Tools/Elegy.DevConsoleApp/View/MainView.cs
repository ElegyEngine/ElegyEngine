// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.DevConsoleApp.Model;
using Elegy.DevConsoleApp.View.Components;
using Spectre.Console;

namespace Elegy.DevConsoleApp.View
{
	internal class MainView
	{
		private Thread mInputThread;
		private AppState mState;
		private TextInputBar mInputBar;

		private object mConsoleEventListLock = new();
		private List<ConsoleKeyInfo> mConsoleEventList = new( 1024 );

		public MainView()
		{
			mState = new();
			mInputBar = new();
			mInputThread = new( InputThread );
		}

		private void InputThread()
		{
			var input = AnsiConsole.Console.Input;

			while ( mState.IsRunning )
			{
				Thread.Sleep( 10 );

				var key = input.ReadKey( false );
				if ( !key.HasValue )
				{
					continue;
				}

				lock ( mConsoleEventListLock )
				{
					mConsoleEventList.Add( key.Value );
				}
			}
		}

		private void ProcessEvents()
		{
			lock ( mConsoleEventListLock )
			{
				foreach ( var key in mConsoleEventList )
				{
					mInputBar.ProcessEvent( key );

					if ( key.Key == ConsoleKey.Enter )
					{
						ExecuteCommand();
					}
				}

				mConsoleEventList.Clear();
			}
		}

		private void ExecuteCommand()
		{
			string command = mState.InputText.Trim();

			if ( command == "!exit" || command == "!quit" )
			{
				mState.IsRunning = false;
			}
		}

		public void Run()
		{
			Layout layout = new();

			layout.SplitRows(
				new Layout( "StatusBar" )
					.Size( 3 ),

				new Layout( "Content" )
					.SplitColumns(
						new Layout( "MessageArea" ),
						new Layout( "AutocompleteArea" ).Size( 36 )
					),

				new Layout( "InputBar" )
					.Size( 3 )
			);

			layout["StatusBar"]
				.Update( new Panel( "Status: OK" )
					.Header( "Elegy Developer Console", Justify.Center )
					.Expand()
				);

			layout["MessageArea"]
				.Update( new Panel( "Placeholder text - this area will be full of message content" )
					.Expand()
				);

			layout["AutocompleteArea"]
				.Update( new Panel( "Placeholder text\nGonna have a buncha stuff here" )
					.Header( "Autocomplete", Justify.Right )
					.Expand()
				);

			layout["InputBar"]
				.Update( new Panel( mInputBar )
					.Expand()
				);

			AnsiConsole.Console.Cursor.Hide();
			AnsiConsole.Live( layout )
				.Cropping( VerticalOverflowCropping.Bottom )
				.Overflow( VerticalOverflow.Ellipsis )
				.AutoClear( false ) // the user should be able to see last messages before a crash
				.Start( context =>
				{
					mInputThread.Start();

					while ( mState.IsRunning )
					{
						context.Refresh();
						Thread.Sleep( 16 );

						ProcessEvents();

						mInputBar.AdvanceTime( 16 );
						mState.InputText = mInputBar.Text;
					}

					mInputThread.Join();
					context.Refresh();
				} );
		}
	}
}
