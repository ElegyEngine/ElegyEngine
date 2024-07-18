// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets.ElegyMapData;
using SharpGLTF.IO;
using SharpGLTF.Schema2;
using System.Text.Json;

namespace Elegy.Common.Assets.GltfExtensions
{
	/// <summary>
	/// GLTF extension for Elegy levels data.
	/// </summary>
	public class ElegyLevelExtension : ExtraProperties
	{
		/// <summary>
		/// IDs of world meshes.
		/// </summary>
		public List<int> WorldMeshIds { get; set; } = new();

		/// <summary>
		/// Map entities.
		/// </summary>
		public List<Entity> Entities { get; set; } = new();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ElegyLevelExtension()
		{

		}

		/// <summary>
		/// Default constructor for extension registration.
		/// </summary>
		public ElegyLevelExtension( ModelRoot root )
		{

		}

		protected override void DeserializeProperty( string jsonPropertyName, ref Utf8JsonReader reader )
		{
			switch ( jsonPropertyName )
			{
				case "WorldMeshIds": DeserializePropertyList( ref reader, WorldMeshIds ); break;
				case "Entities": DeserialiseEntities( ref reader ); break;
				default: base.DeserializeProperty( jsonPropertyName, ref reader ); break;
			}
		}

		protected override void SerializeProperties( Utf8JsonWriter writer )
		{
			base.SerializeProperties( writer );

			SerializeProperty( writer, "WorldMeshIds", WorldMeshIds );
			writer.WritePropertyName( "Entities" );
			writer.WriteStartArray();
			foreach ( var ent in Entities )
			{
				SerialiseEntity( writer, ent );
			}
			writer.WriteEndArray();
		}

		private void DeserialiseEntities( ref Utf8JsonReader reader )
		{
			if ( reader.TokenType != JsonTokenType.PropertyName )
			{
				throw new Exception();
			}

			// We have entered the entity property, are expecting the start
			// of an array now: [
			reader.Read();
			if ( reader.TokenType != JsonTokenType.StartArray )
			{
				throw new Exception();
			}

			// We have entered the entity array, are expecting a { now
			while ( reader.Read() && reader.TokenType != JsonTokenType.EndArray )
			{
				Entity entity = DeserialiseEntity( ref reader );
				Entities.Add( entity );
			}
		}

		private Entity DeserialiseEntity( ref Utf8JsonReader reader )
		{
			if ( reader.TokenType != JsonTokenType.StartObject )
			{
				throw new Exception();
			}

			Entity entity = new();

			while ( reader.Read() && reader.TokenType != JsonTokenType.EndObject )
			{
				if ( reader.TokenType != JsonTokenType.PropertyName )
				{
					throw new Exception();
				}

				string propertyName = reader.GetString();
				reader.Read();

				switch ( propertyName )
				{
					case "RenderMeshId": entity.RenderMeshId = reader.GetInt32(); break;
					case "CollisionMeshId": entity.CollisionMeshId = reader.GetInt32(); break;
					case "OccluderMeshId": entity.OccluderMeshId = reader.GetInt32(); break;
					case "Attributes": DeserializePropertyDictionary( ref reader, entity.Attributes ); break;
				}
			}

			return entity;
		}

		private void SerialiseEntity( Utf8JsonWriter writer, Entity entity )
		{
			writer.WriteStartObject();
			SerializeProperty( writer, "RenderMeshId", entity.RenderMeshId );
			SerializeProperty( writer, "CollisionMeshId", entity.CollisionMeshId );
			SerializeProperty( writer, "OccluderMeshId", entity.OccluderMeshId );
			SerializeProperty( writer, "Attributes", entity.Attributes );
			writer.WriteEndObject();
		}
	}
}
