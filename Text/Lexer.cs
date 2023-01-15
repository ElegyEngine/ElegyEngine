// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Text
{
	/// <summary>
	/// Lexicological parser, parses tokens out of a continuous piece of text.
	/// Ported from adm-utils: https://github.com/Admer456/adm-utils/blob/master/src/Text/Lexer.hpp
	/// </summary>
	public class Lexer
	{
		#region Constructors
		public Lexer( string text, string delimiters = DelimitersSimple )
		{
			Load( text );
			SetDelimiters( delimiters );
		}
		#endregion

		#region Constants
		public const string DelimitersFull = "()[]{}.:,;=+-*/&@'?!#%";
		public const string DelimitersSimple = "()[]{}.:,;";
		#endregion

		#region Methods
		public void Clear()
		{
			mText = "";
		}

		public void Load( string text )
		{
			mText = text;
		}

		public void SetDelimiters( string delimiters )
		{
			mDelimiters = delimiters;
		}

		/// <summary>
		/// Continues parsing the string and extracts the next token from it.
		/// </summary>
		/// <returns>The next token in line, "" if EOF is reached.</returns>
		public string Next()
		{
			if ( IsEnd() )
			{
				return string.Empty;
			}

			string result = string.Empty;

			// This do-while loop is responsible for skipping "empty" lines
			// and advancing to the next ones, until we get a usable token
			do
			{
				// Skip any whitespaces etc. after a token
				while ( !CanAdvance() && !IsDelimiter() && !IsEnd() )
				{
					if ( IsComment() )
					{
						NewLine();
						continue;
					}

					IncrementPosition();

					if ( IsEnd() )
					{
						return string.Empty;
					}
				}

				// Can't go any further
				if ( IsEnd() )
				{
					return string.Empty;
				}

				// Check for delimiters
				if ( IsDelimiter() )
				{
					result += mText[mPosition];
					IncrementPosition();

					return result;
				}

				while ( CanAdvance() )
				{
					// We only support single-line comments, so
					// if a comment is encountered, skip the whole line
					if ( IsComment() )
					{
						NewLine();
						continue;
					}

					// A quotation mark has been encountered while
					// we weren't in quote mode - engage
					if ( mText[mPosition] == '"' )
					{
						ToggleQuoteMode();
						IncrementPosition();
						continue;
					}

					// In quote mode, we add spaces and tabs too
					if ( CanAdd() )
					{
						result += mText[mPosition];
					}

					// Safely increment the position so we don't go out of bounds
					IncrementPosition();

					// If we're on a newline now, bump up the line counter
					if ( !IsEnd() && IsEndOfLine() )
					{
						mLineNumber++;
						mLineColumn = 0;
					}
				}
			} while ( result == string.Empty );

			// Escape from a quote
			if ( mInQuote && mText[mPosition] == '"' )
			{
				ToggleQuoteMode();
				IncrementPosition();
			}

			return result;
		}

		/// <summary>
		/// Peeks at the next string.
		/// </summary>
		/// <returns>The next token in line, "" if EOF is reached.</returns>
		public string Peek()
		{
			int oldPosition = mPosition;
			int oldLine = mLineNumber;
			int oldColumn = mLineColumn;

			string token = Next();

			mPosition = oldPosition;
			mLineNumber = oldLine;
			mLineColumn = oldColumn;

			return token;
		}

		/// <summary>
		/// Peeks at the next string and, optionally, advances seeking.
		/// </summary>
		/// <param name="expectedToken">The token to expect.</param>
		/// <param name="advanceIfTrue">Advance the seeking upon success? Behaves about the same as Next then.</param>
		/// <returns>true if the next token and expectedToken match.</returns>
		public bool Expect( string expectedToken, bool advanceIfTrue = false )
		{
			int oldPosition = mPosition;
			int oldLine = mLineNumber;
			int oldColumn = mLineColumn;

			string token = Next();
			bool equal = token == expectedToken;

			// The only situation where you don't want mPosition = oldPosition is when both advanceIfTrue and equal are true, ergo the !().
			// It's a little counter-intuitive because you'd expect something like 'if ( advanceIfTrue && equal ) -> mPosition += advanceAmount'.
			// But no, that's not how it works here, the position is already advanced, so we're just kinda reverting it in 3 out of 4 possible cases.
			if ( !(advanceIfTrue && equal) )
			{
				mPosition = oldPosition;
				mLineNumber = oldLine;
				mLineColumn = oldColumn;
			}

			return equal;
		}

		/// <summary>
		/// Whether or not the end of the text has been reached.
		/// </summary>
		public bool IsEnd()
		{
			return mPosition >= mText.Length;
		}

		/// <summary>
		/// Gets the current line and column, formatted in a string.
		/// </summary>
		public string GetLineInfo()
		{
			return $"(line {mLineNumber}, column {mLineColumn})";
		}

		/// <summary>
		/// Gets the current line number.
		/// </summary>
		public int GetLineNumber()
		{
			return mLineNumber;
		}
		#endregion

		#region Properties
		public bool IgnoreDelimiters { get; set; } = false;
		#endregion

		#region Private methods
		bool CanAdd()
		{
			char c = mText[mPosition];
			return CanAdvance() && c != '"';
		}

		bool CanAdvance()
		{
			if ( IsEnd() )
			{
				return false;
			}

			char c = mText[mPosition];

			if ( mInQuote )
			{
				return !IsEnd() && c != '"';
			}

			if ( IsDelimiter() )
			{
				return false;
			}

			return c != ' ' && c != '\t' && c != '\0' && !IsEndOfLine();
		}

		void IncrementPosition()
		{
			if ( !IsEnd() )
			{
				mPosition++;
				mLineColumn++;
			}
		}

		bool IsComment()
		{
			if ( mInQuote )
			{
				return false;
			}

			if ( mText[mPosition] == '/' )
			{
				if ( mPosition + 1 < mText.Length )
				{
					if ( mText[mPosition + 1] == '/' )
					{
						return true;
					}
				}
			}

			return false;
		}

		bool IsDelimiter()
		{
			if ( IgnoreDelimiters )
			{
				return false;
			}

			// First check if this is actually a number we're parsing
			// I.e. prevent splitting up 10.25 into tokens '10', '.' and '25
			if ( mPosition > 0 && mText[mPosition] == '.' && (mPosition + 1) < mText.Length )
			{
				// Make sure to also support numbers like .25
				if ( (mText[mPosition - 1].IsNumeric() || mText[mPosition - 1] == ' ') && mText[mPosition + 1].IsNumeric() )
				{
					return false;
				}
			}

			return mDelimiters.IndexOf( mText[mPosition] ) != -1;
		}

		bool IsEndOfLine()
		{
			return mText[mPosition] == '\n' || mText[mPosition] == '\r';
		}

		void NewLine()
		{
			int nextLinePosition = mText.IndexOf( '\n', mPosition + 1 );
			if ( nextLinePosition < 0 )
			{
				// This can happen if there's a comment in the last line
				mPosition = mText.Length + 1;
				return;
			}

			mPosition = nextLinePosition + 1;
			mLineNumber++;
			mLineColumn = 0;
		}

		void ToggleQuoteMode()
		{
			mInQuote = !mInQuote;
		}
		#endregion

		#region Private members
		private int mPosition = 0;
		private int mLineNumber = 0;
		private int mLineColumn = 0;
		private bool mInQuote = false;
		private string mText;
		private string mDelimiters;
		#endregion
	}
}
