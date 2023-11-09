
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elegy.Text.JsonAdapters
{
	internal class GodotVector4Converter : JsonConverter<Vector4>
	{
		public override Vector4 Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			Vector4 value = Vector4.Zero;

			reader.Read();
			value.X = reader.GetSingle();
			reader.Read();
			value.Y = reader.GetSingle();
			reader.Read();
			value.Z = reader.GetSingle();
			reader.Read();
			value.W = reader.GetSingle();
			reader.Read();

			return value;
		}

		public override void Write( Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options )
		{
			writer.WriteRawValue( $"[{value.X}, {value.Y}, {value.Z}, {value.W}]", true );
		}
	}

	internal class GodotVector3Converter : JsonConverter<Vector3>
	{
		public override Vector3 Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			Vector3 value = Vector3.Zero;

			reader.Read();
			value.X = reader.GetSingle();
			reader.Read();
			value.Y = reader.GetSingle();
			reader.Read();
			value.Z = reader.GetSingle();
			reader.Read();

			return value;
		}

		public override void Write( Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options )
		{
			writer.WriteRawValue( $"[{value.X}, {value.Y}, {value.Z}]", true );
		}
	}

	internal class GodotVector2Converter : JsonConverter<Vector2>
	{
		public override Vector2 Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			Vector2 value = Vector2.Zero;

			reader.Read();
			value.X = reader.GetSingle();
			reader.Read();
			value.Y = reader.GetSingle();
			reader.Read();

			return value;
		}

		public override void Write( Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options )
		{
			writer.WriteRawValue( $"[{value.X}, {value.Y}]", true );
		}
	}

	internal class GodotAabbConverter : JsonConverter<Aabb>
	{
		public override Aabb Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			Vector3 position = Vector3.Zero;
			Vector3 size = Vector3.Zero;

			reader.Read();
			position.X = reader.GetSingle();
			reader.Read();
			position.Y = reader.GetSingle();
			reader.Read();
			position.Z = reader.GetSingle();
			reader.Read();
			size.X = reader.GetSingle();
			reader.Read();
			size.Y = reader.GetSingle();
			reader.Read();
			size.Z = reader.GetSingle();
			reader.Read();

			Aabb value = new( position, size );
			return value;
		}

		public override void Write( Utf8JsonWriter writer, Aabb value, JsonSerializerOptions options )
		{
			writer.WriteRawValue( $"[{value.Position.X}, {value.Position.Y}, {value.Position.Z}, {value.Size.X}, {value.Size.Y}, {value.Size.Z}]", true );
		}
	}

	internal class GodotRect2Converter : JsonConverter<Rect2>
	{
		public override Rect2 Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			Vector2 position = Vector2.Zero;
			Vector2 size = Vector2.Zero;

			reader.Read();
			position.X = reader.GetSingle();
			reader.Read();
			position.Y = reader.GetSingle();
			reader.Read();
			size.X = reader.GetSingle();
			reader.Read();
			size.Y = reader.GetSingle();
			reader.Read();

			Rect2 value = new( position, size );
			return value;
		}

		public override void Write( Utf8JsonWriter writer, Rect2 value, JsonSerializerOptions options )
		{
			writer.WriteRawValue( $"[{value.Position.X}, {value.Position.Y}, {value.Size.X}, {value.Size.Y}]", true );
		}
	}
}
