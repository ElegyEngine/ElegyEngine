// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Text;

// Temporary for tests
public interface ILexer
{
	void Load( string text );
	
	ReadOnlySpan<char> Next();

	bool IsEnd();
}
