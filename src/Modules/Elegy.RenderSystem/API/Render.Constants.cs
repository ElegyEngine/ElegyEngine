// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Veldrid;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		public static class Layouts
		{
			public static ResourceLayout PerEntity { get; internal set; }
			public static ResourceLayout PerView { get; internal set; }
			public static ResourceLayout Window { get; internal set; }
		}

		public static class Samplers
		{
			public static Sampler Nearest { get; internal set; }
			public static Sampler Linear { get; internal set; }
			public static Sampler Aniso2x { get; internal set; }
			public static Sampler Aniso4x { get; internal set; }
			public static Sampler Aniso8x { get; internal set; }
			public static Sampler Aniso16x { get; internal set; }

			public static Sampler NearestClamp { get; internal set; }
			public static Sampler LinearClamp { get; internal set; }
			public static Sampler Aniso2xClamp { get; internal set; }
			public static Sampler Aniso4xClamp { get; internal set; }
			public static Sampler Aniso8xClamp { get; internal set; }
			public static Sampler Aniso16xClamp { get; internal set; }

			public static Sampler NearestBorder { get; internal set; }
			public static Sampler LinearBorder { get; internal set; }
			public static Sampler Aniso2xBorder { get; internal set; }
			public static Sampler Aniso4xBorder { get; internal set; }
			public static Sampler Aniso8xBorder { get; internal set; }
			public static Sampler Aniso16xBorder { get; internal set; }
		}

		public static readonly OutputDescription OutputDescNormal = new()
		{
			SampleCount = TextureSampleCount.Count1,
			DepthAttachment = new( PixelFormat.D32_Float_S8_UInt ),
			ColorAttachments =
			[
				new( PixelFormat.B8_G8_R8_A8_UNorm )
			]
		};

		public static readonly OutputDescription OutputDescDepthOnly = new()
		{
			SampleCount = TextureSampleCount.Count1,
			DepthAttachment = new( PixelFormat.D32_Float_S8_UInt ),
			ColorAttachments = null
		};

		public static readonly OutputDescription OutputDescBackbuffer = new()
		{
			SampleCount = TextureSampleCount.Count1,
			DepthAttachment = null,
			ColorAttachments =
			[
				new( PixelFormat.B8_G8_R8_A8_UNorm )
			]
		};

	}
}
