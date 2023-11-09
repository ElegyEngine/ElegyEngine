// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Assets.BrushMapData
{
	public class Entity
	{
		public Vector3 Centre = Vector3.Zero;
		public Aabb BoundingBox = new();
		public List<Brush> Brushes = new();

		public string ClassName = string.Empty;
		public Dictionary<string, string> Pairs = new();

		public Vector3 GetBrushOrigin()
		{
			if ( ClassName == "worldspawn" )
			{
				return Vector3.Zero;
			}

			Vector3 origin = new();
			int count = 0;

			// The brush polygons haven't been created yet, but we
			// likely have enough information from the plane definitions
			Brushes.ForEach( brush =>
			{
				brush.Faces.ForEach( face =>
				{
					if ( face.MaterialName.ToLower().Contains( "origin" ) )
					{
						return;
					}

					origin += face.Centre;
					count++;
				} );
			} );

			if ( count == 0 )
			{
				return BoundingBox.GetCenter();
			}

			return origin / count;
		}
	}
}
