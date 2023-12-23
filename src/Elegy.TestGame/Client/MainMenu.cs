// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

//using Godot;
using static Godot.Control;

namespace TestGame.Client
{
	public class MainMenu
	{
		public MainMenu()
		{
			mRoot = SetupControlAutoexpand<Control>( null, true, true );
		}

		public Action<string> OnNewGame { get; set; }
		public Action OnLeaveGame { get; set; }
		public Action OnExit { get; set; }

		public bool Visible
		{
			get => mRoot.Visible;
			set => mRoot.Visible = value;
		}

		public bool InGame
		{
			set => mLeaveGameButton.Disabled = !value;
		}

		public void Init()
		{
			mRoot.Size = mRoot.GetViewportRect().Size;

			var panel			= SetupControlAutoexpand<Panel>( mRoot, true, true );
			var hbox			= SetupControlAutoexpand<HBoxContainer>( panel, true, true );
			var containerLeft	= SetupControlAutoexpand<VBoxContainer>( hbox, true );
			var container		= SetupControlAutoexpand<VBoxContainer>( hbox, true );
			var containerRight	= SetupControlAutoexpand<VBoxContainer>( hbox, true );

			container.CustomMinimumSize = new( 200.0f, 50.0f );
			container.Alignment = BoxContainer.AlignmentMode.Center;
			container.SizeFlagsStretchRatio = 0.25f;

			mNewGameButton = ButtonTextAction( container, "New game", NewGamePressed );

			mLeaveGameButton = ButtonTextAction( container, "Leave game", () =>
			{
				OnLeaveGame();
				mLeaveGameButton.Disabled = true;
			} );
			mLeaveGameButton.Disabled = true;

			mMapSelectionButton = MapSelection( container );

			ButtonTextAction( container, "Exit", OnExit );
		}

		private Button mNewGameButton;
		private Button mLeaveGameButton;
		private OptionButton? mMapSelectionButton = null;

		private void NewGamePressed()
		{
			mLeaveGameButton.Disabled = false;
			
			if ( mMapSelectionButton != null )
			{
				int id = mMapSelectionButton.Selected;
				string mapName = mMapSelectionButton.GetItemText( id );
				OnNewGame( mapName );
			}
		}

		private TControl SetupControlAutoexpand<TControl>( Control? parent, bool anchor, bool fullRect = false ) where TControl : Control, new()
		{
			TControl control = parent?.CreateChild<TControl>() ?? Nodes.CreateNode<TControl>();

			const SizeFlags sizeFlags = SizeFlags.ExpandFill;
			control.SizeFlagsHorizontal = sizeFlags;
			control.SizeFlagsVertical = sizeFlags;

			if ( anchor )
			{
				control.LayoutMode = 1;
			}

			if ( fullRect )
			{
				control.Size = control.GetViewportRect().Size;
			}

			control.SetAnchorsPreset( LayoutPreset.FullRect );

			return control;
		}

		private void MapErrorReport( Control parent, string text )
		{
			Label noMapLabel = parent.CreateChild<Label>();
			noMapLabel.Text = text;
		}

		private OptionButton? MapSelection( Control parent )
		{
			string[]? mapFiles = FileSystem.GetFiles( "maps", "*.elf", true );
			if ( mapFiles is null || mapFiles.Length <= 0 )
			{
				MapErrorReport( parent, "There are no map files in the 'maps' folder" );
				return null;
			}

			OptionButton button = parent.CreateChild<OptionButton>();
			for ( int i = 0; i < mapFiles.Length; i++ )
			{
				// Strip off the 'game/maps/' part
				int mapsOffset = mapFiles[i].Find( "/maps/" );
				button.AddItem( Path.GetFileNameWithoutExtension( mapFiles[i].Remove( 0, mapsOffset + 6 ) ) );
			}

			return button;
		}

		private Button ButtonTextAction( Control parent, string text, Action? onPressed )
		{
			var button = parent.CreateChild<Button>();
			button.Text = text;
			if ( onPressed != null )
			{
				button.Pressed += onPressed;
			}
			return button;
		}

		private Control mRoot;
	}
}
