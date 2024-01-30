// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.MapCompiler.Data.Processing
{
	public class Entity
	{
		public Vector3 Centre = Vector3.Zero;
		public Box3 BoundingBox = new();
		public List<Brush> Brushes = new();

		public string ClassName = string.Empty;
		public Dictionary<string, string> Pairs = new();

		public Entity( BrushMapEntity entity )
		{
			Centre = entity.Centre;
			BoundingBox = entity.BoundingBox;
			ClassName = entity.ClassName;
			Pairs = new( entity.Pairs );

			Brushes = new( entity.Brushes.Count );
			for ( int i = 0; i < entity.Brushes.Count; i++ )
			{
				Brushes.Add( new( entity.Brushes[i] ) );
			}
		}

		public bool IsWorld()
		{
			return ClassName == "worldspawn";
		}

		public bool IsPointEntity()
		{
			return Brushes.Count == 0;
		}

		public void RegenerateBounds( bool brushesToo = false )
		{
			// Point entities are points after all
			if ( Brushes.Count == 0 )
			{
				BoundingBox = new( Centre, Vector3.One * 0.001f );
				return;
			}

			if ( brushesToo )
			{
				for ( int i = 0; i < Brushes.Count; i++ )
				{
					Brushes[i].RegenerateBounds();
				}
			}

			BoundingBox = Brushes[0].BoundingBox;
			for ( int i = 1; i < Brushes.Count; i++ )
			{
				BoundingBox = BoundingBox.Merge( Brushes[i].BoundingBox );
			}
		}
	}
}
