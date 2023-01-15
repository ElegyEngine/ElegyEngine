// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace Elegy
{
	public class PluginLibraryMetadata
	{
		internal PluginLibraryMetadata( PluginConfig config )
		{
			AssemblyName = config.AssemblyName;
			Description = config.Description;
			Author = config.Author;
			VersionDate = DateTime.ParseExact( config.VersionDateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture );
			ImplementedInterface = config.ImplementedInterface;

			string[] engineVersionSubstrings = config.EngineVersion.Split( '.' );

			if ( int.TryParse( engineVersionSubstrings[0], out int major ) )
			{
				EngineVersionMajor = major;
			}
			if ( int.TryParse( engineVersionSubstrings[1], out int minor ) )
			{
				EngineVersionMinor = minor;
			}
		}

		public string AssemblyName { get; } = string.Empty;
		public string Description { get; } = string.Empty;
		public string Author { get; } = string.Empty;
		public DateTime VersionDate { get; } = DateTime.UnixEpoch;
		public int EngineVersionMajor { get; } = -1;
		public int EngineVersionMinor { get; } = -1;
		public string ImplementedInterface { get; } = string.Empty;

		public string EngineVersionString => $"{EngineVersionMajor}.{EngineVersionMinor}";

		internal bool IsCompatible( int major, int minor )
		{
			if ( EngineVersionMajor < major )
			{
				return false;
			}

			if ( EngineVersionMinor < minor )
			{
				return false;
			}

			return true;
		}

		internal bool Validate( out List<string> errorMessages )
		{
			List<string> errorMessageList = new();

			var validateCondition = ( bool condition, string messageIfIncorrect ) =>
			{
				if ( !condition )
				{
					errorMessageList.Add( messageIfIncorrect );
				}
			};

			validateCondition( AssemblyName != string.Empty,
				"AssemblyName is empty (examples: 'MyGame')" );
			validateCondition( !AssemblyName[0].IsNumeric(),
				"AssemblyName cannot start with a number (examples: 'MyGame')" );
			validateCondition( EngineVersionMajor >= 0 || EngineVersionMinor >= 0,
				$"EngineVersion is incorrect (parsed '{EngineVersionString}'; examples: '1.2', '0.5', '4.5')" );
			validateCondition( ImplementedInterface != string.Empty,
				"ImplementedInterface is empty (examples: 'IPlugin', 'IApplication', 'IGame')" );

			errorMessages = errorMessageList;
			return errorMessages.Count <= 0;
		}
	}
}
