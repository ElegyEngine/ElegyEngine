
﻿// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public class PluginLibraryMetadata
	{
		public PluginLibraryMetadata( string name, string description, string author, DateTime versionDate, string implementedInterface )
		{
			Name = name;
			Description = description;
			Author = author;
			VersionDate = versionDate;
			ImplementedInterface = implementedInterface;

			ParsedCorrectly = true;
		}

		public string Name { get; } = string.Empty;
		public string Description { get; } = string.Empty;
		public string Author { get; } = string.Empty;
		public DateTime VersionDate { get; }
		public string ImplementedInterface { get; } = string.Empty;
		public bool ParsedCorrectly { get; } = false;
	}
}
