// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Material system implementation.
	/// </summary>
	internal sealed class MaterialSystemInternal
	{
		private List<Material> mMaterials = new();

		public Material? LoadMaterial( string materialName )
		{
			return null;
		}

		public bool UnloadMaterial( ref Material material )
		{
			return false;
		}

		public IReadOnlyList<Material> GetMaterialList() => mMaterials;
	}
}
