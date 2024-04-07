// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Assets.BrushMapData
{
	/// <summary></summary>
	public class Entity
	{
		/// <summary></summary>
		public Vector3 Centre = Vector3.Zero;
		/// <summary></summary>
		public Box3 BoundingBox = new();
		/// <summary></summary>
		public List<Brush> Brushes = new();

		/// <summary></summary>
		public string ClassName = string.Empty;
		/// <summary></summary>
		public Dictionary<string, string> Pairs = new();

		/// <summary></summary>
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
