// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public interface IPlugin
	{
		bool Init();
		void Shutdown();

		string Name { get; }
		string Error { get; }
		bool Initialised { get; }
	}
}
