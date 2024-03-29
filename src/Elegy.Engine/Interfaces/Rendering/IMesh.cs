﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Interfaces;

namespace Elegy.Engine.Interfaces.Rendering
{
	/// <summary>
	/// RenderFrame mesh.
	/// </summary>
	public interface IMesh
	{
		/// <summary>
		/// Actual mesh data.
		/// </summary>
		Model Data { get; set; }

		/// <summary>
		/// Updates vertex and index buffers.
		/// </summary>
		void RegenerateBuffers();
	}
}
