// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Text;
using Elegy.RenderBackend.Assets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elegy.RenderBackend.Text
{
	public static class MaterialTemplateJsonOptions
	{
		public readonly static JsonSerializerOptions Instance = JsonHelpers.Create( new()
		{
			Converters =
			{
				new JsonStringEnumConverter<Blending>(),
				new JsonStringEnumConverter<Veldrid.FaceCullMode>()
			}
		} );
	}
}
