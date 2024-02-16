// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Utilities;
using Elegy.Utilities.Interfaces;
using System.Numerics;

namespace Elegy.Rendering
{
	/// <summary>
	/// RenderFrame entity.
	/// </summary>
	public interface IEntity : ITransformable
	{
		/// <summary>
		/// Layers that this entity will be rendered into.
		/// </summary>
		int Mask { get; set; }

		/// <summary>
		/// Reference to an <see cref="IMesh"/>.
		/// </summary>
		int MeshId { get; set; }

		/// <summary>
		/// Animation data.
		/// </summary>
		SpanIndirect<Matrix4x4> BoneBuffer { get; set; }
	}
}
