// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Entities
{
	public class Light : Entity
	{
		public override void Spawn()
		{
			base.Spawn();

			mLight = Nodes.CreateNode<OmniLight3D>();
			mRootNode = mLight;

			ShadowEnabled = false;
			ShadowBias = 0.02f;
			ShadowNormalBias = 1.2f;
			ShadowMode = OmniLight3D.ShadowMode.Cube;

			AngularDistance = 0.1f;
			Attenuation = 1.0f;
			IndirectEnergy = 2.0f;
			PointLightSize = 0.4f;
			VolumetricFogEnergy = 1.5f;

			Colour = Colors.White;
			Energy = 1.0f;
			Range = 6.5f;
		}

		public override void KeyValue( Dictionary<string, string> pairs )
		{
			base.KeyValue( pairs );

			if ( pairs.TryGetValue( "targetname", out string? targetname ) )
			{
				mLight.Name = targetname;
			}

			if ( pairs.ContainsKey( "_light" ) )
			{
				Vector4 lightValues = pairs["_light"].ToVector4();
				if ( lightValues.W == 0.0f )
				{
					lightValues.W = 300.0f;
				}

				// Convert from [0-255] to [0-1]
				lightValues /= 255.0f;

				Colour = new Color( lightValues.X, lightValues.Y, lightValues.Z, 1.0f );
				Energy = lightValues.W * 1.2f;
				Range = Mathf.Sqrt( lightValues.W ) * 12.0f;
			}

			if ( pairs.TryGetValue( "_pointsize", out string? pointsize ) )
			{
				PointLightSize = Parse.Float( pointsize );
			}

			if ( pairs.TryGetValue( "_shadows", out string? shadows ) )
			{
				ShadowEnabled = shadows != "0";
			}
			else
			{
				ShadowEnabled = true;
			}
		}

		public float AngularDistance
		{
			get => mLight.LightAngularDistance;
			set => mLight.LightAngularDistance = value;
		}
		public float Attenuation
		{
			get => mLight.OmniAttenuation;
			set => mLight.OmniAttenuation = value;
		}
		public Color Colour
		{
			get => mLight.LightColor;
			set => mLight.LightColor = value;
		}
		public float Energy
		{
			get => mLight.LightEnergy;
			set => mLight.LightEnergy = value;
		}
		public float IndirectEnergy
		{
			get => mLight.LightIndirectEnergy;
			set => mLight.LightIndirectEnergy = value;
		}
		public float Range
		{
			get => mLight.OmniRange;
			set => mLight.OmniRange = value;
		}
		public float PointLightSize
		{
			get => mLight.LightSize;
			// mLight.LightSize = X does not work straight away when spawning...
			set => mLight.SetDeferred( "light_size", value );
		}
		public float ShadowBias
		{
			get => mLight.ShadowBias;
			set => mLight.ShadowBias = value;
		}
		public float ShadowNormalBias
		{
			get => mLight.ShadowNormalBias;
			set => mLight.ShadowNormalBias = value;
		}
		public bool ShadowEnabled
		{
			get => mLight.ShadowEnabled;
			set => mLight.ShadowEnabled = value;
		}
		public OmniLight3D.ShadowMode ShadowMode
		{
			get => mLight.OmniShadowMode;
			set => mLight.OmniShadowMode = value;
		}
		public float VolumetricFogEnergy
		{
			get => mLight.LightVolumetricFogEnergy;
			set => mLight.LightVolumetricFogEnergy = value;
		}

		OmniLight3D mLight;
	}
}
