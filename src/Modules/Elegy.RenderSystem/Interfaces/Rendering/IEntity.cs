// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.Common.Interfaces;
using System.Numerics;

namespace Elegy.RenderSystem.Interfaces.Rendering
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
		IMesh Mesh { get; set; }

		/// <summary>
		/// Animation data.
		/// </summary>
		SpanIndirect<Matrix4x4> BoneBuffer { get; set; }
	}
}
