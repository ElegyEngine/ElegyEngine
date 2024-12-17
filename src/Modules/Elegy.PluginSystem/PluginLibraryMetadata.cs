// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.Common.Utilities;

namespace Elegy.PluginSystem
{
	internal class PluginLibraryMetadata
	{
		public PluginLibraryMetadata( PluginConfig config )
		{
			AssemblyName = config.AssemblyName;
			Description = config.Description;
			Author = config.Author;
			VersionDate = Parse.Date( config.VersionDateString );
			ImplementedInterface = config.ImplementedInterface;

			int[] engineVersions = config.EngineVersion.Split( '.' ).Select( int.Parse ).ToArray();
			if ( engineVersions.Length >= 2 )
			{
				EngineVersionMajor = engineVersions[0];
				EngineVersionMinor = engineVersions[1];
			}
		}

		public string AssemblyName { get; }
		public string Description { get; }
		public string Author { get; }
		public DateTime VersionDate { get; }
		public int EngineVersionMajor { get; } = -1;
		public int EngineVersionMinor { get; } = -1;
		public string ImplementedInterface { get; }

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
