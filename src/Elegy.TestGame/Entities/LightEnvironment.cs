// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Entities
{
	public class LightEnvironment : Entity
	{
		public override void Spawn()
		{
			base.Spawn();

			mLight = Nodes.CreateNode<DirectionalLight3D>();
			mRootNode = mLight;

			ShadowEnabled = false;
			ShadowBias = 0.02f;
			ShadowNormalBias = 1.2f;

			AngularDistance = 0.1f;
			IndirectEnergy = 2.0f;
			PointLightSize = 0.4f;
			VolumetricFogEnergy = 1.5f;

			Colour = Colors.White;
			Energy = 1.0f;
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

			if ( pairs.TryGetValue( "angles", out string? angles ) )
			{
				mLight.GlobalRotationDegrees = angles.ToVector3() * new Vector3( -1.0f, 1.0f, 0.0f );
			}

			if ( pairs.TryGetValue( "_pitch", out string? pitch ) )
			{
				mLight.GlobalRotationDegrees = new Vector3(
					-Parse.Float( pitch ),
					mLight.GlobalRotationDegrees.Y,
					0.0f );
			}
		}

		public float AngularDistance
		{
			get => mLight.LightAngularDistance;
			set => mLight.LightAngularDistance = value;
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
		public float VolumetricFogEnergy
		{
			get => mLight.LightVolumetricFogEnergy;
			set => mLight.LightVolumetricFogEnergy = value;
		}

		DirectionalLight3D mLight;
	}
}
