// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Text
{
	/// <summary>
	/// Lexicological parser, parses tokens out of a continuous piece of text.
	/// Ported from adm-utils: https://github.com/Admer456/adm-utils/blob/master/src/Text/Lexer.hpp
	/// </summary>
	/// <remarks>Right now it works with <see cref="string"/>, it may be worth investigating a <see cref="ReadOnlySpan{T}"/> version.</remarks>
	public class Lexer2 : ILexer
	{
		#region Constructors
		/// <summary></summary>
		public Lexer2( string text, string delimiters = DelimitersSimple )
		{
			mText = text;
			mDelimiters = delimiters;
		}
		#endregion

		#region Constants
		/// <summary></summary>
		public const string DelimitersFull = "()[]{}.:,;=+-*/&@'?!#%";
		/// <summary></summary>
		public const string DelimitersSimple = "()[]{}.:,;";
		#endregion

		#region Methods
		/// <summary>
		/// Clears the text buffer.
		/// </summary>
		public void Clear()
		{
			mText = "";
		}

		/// <summary>
		/// Loads the given <paramref name="text"/>
		/// </summary>
		/// <param name="text"></param>
		public void Load( string text )
		{
			mText = text;
		}

		/// <summary>
		/// Sets new delimiters.
		/// </summary>
		public void SetDelimiters( string delimiters )
		{
			mDelimiters = delimiters;
		}

		/// <summary>
		/// Continues parsing the string and extracts the next token from it.
		/// </summary>
		/// <returns>The next token in line, "" if EOF is reached.</returns>
		public ReadOnlySpan<char> Next()
		{
			if ( IsEnd() )
			{
				return string.Empty;
			}

			int GetNextValidCharacter( int i )
			{
				// Situation: We have just started looking for tokens
				// First, we gotta skip all passable characters until we get
				// an alphanumerical or a quotation mark.
				while ( !IsEnd() && !IsValidCharacter() )
				{
					// Special case: we may encounter a comment along the way
					if ( IsComment() )
					{
						NewLine();
					}
					
					IncrementPosition();
				}
				
				// Okay, we have stopped advancing. This means EOF was encountered,
				// or a valid character was encountered, or a quotation mark.
				if ( IsEnd() )
				{
					return mText.Length + 1;
				}

				if ( IsQuote() )
				{
					ToggleQuoteMode();
					IncrementPosition();
				}
				
				return mPosition;
			}

			int GetLastValidCharacter( int i )
			{
				// Situation: We are looking for the tail of the token
				// This means search until a whitespace, tab, newline or comment is found
				while ( IsValidCharacter() )
				{
					IncrementPosition();
					
					// If we have encountered a quote along the way, make sure to remember it
					if ( IsQuote() )
					{
						ToggleQuoteMode();
						return mPosition - 1;
					}
				}
				
				return mPosition;
			}
			
			int start = GetNextValidCharacter( mPosition );
			// Special case: reached EOF
			if ( start >= mText.Length )
			{
				return string.Empty;
			}
			
			// Special case: it's a delimiter!
			if ( IsDelimiter() )
			{
				IncrementPosition();
				return mText.AsSpan().Slice( mPosition - 1, 1 );
			}
			
			// This one will handle quote mode
			int end = GetLastValidCharacter( start );
			if ( start == end )
			{
				return string.Empty;
			}
			
			// Nudge it forward a bit so the next token starts *after* this one
			IncrementPosition();
			return mText.AsSpan().Slice( start, end - start );
		}

		/// <summary>
		/// Peeks at the next string.
		/// </summary>
		/// <returns>The next token in line, "" if EOF is reached.</returns>
		public ReadOnlySpan<char> Peek()
		{
			StashPosition();
			var token = Next();
			StashPop();
			return token;
		}

		/// <summary>
		/// Returns all characters that were encountered between the current position
		/// and <paramref name="what"/>. If <paramref name="skipPeeked"/> is <c>true</c>,
		/// the cursor will skip the result too.
		/// </summary>
		public ReadOnlySpan<char> PeekUntil( string what, bool skipPeeked = false, bool skipWhatToo = true )
		{
			StashPosition();
			SkipUntil( what, skipWhatToo );

			ReadOnlySpan<char> result = string.Empty;
			if ( mPositionStash != mPosition )
			{
				result = mText.AsSpan().Slice( mPositionStash, mPosition - mPositionStash );
			}

			if ( !skipPeeked )
			{
				StashPop();
			}

			return result;
		}

		/// <summary>
		/// An array of tokens until the newline.
		/// </summary>
		public IList<string> TokenListBeforeNewline()
		{
			List<string> strings = new();
			while ( ExpectAnythingUntilNewline() )
			{
				strings.Add( Next().ToString() );
			}
			return strings;
		}

		/// <summary>
		/// Peeks at the next string and, optionally, advances seeking.
		/// </summary>
		/// <param name="expectedToken">The token to expect.</param>
		/// <param name="advanceIfTrue">Advance the seeking upon success? Behaves about the same as Next then.</param>
		/// <returns>true if the next token and expectedToken match.</returns>
		public bool Expect( ReadOnlySpan<char> expectedToken, bool advanceIfTrue = false )
		{
			StashPosition();

			var token = Next();
			bool equal = token == expectedToken;

			// The only situation where you don't want mPosition = oldPosition is when both advanceIfTrue and equal are true, ergo the !().
			// It's a little counter-intuitive because you'd expect something like 'if ( advanceIfTrue && equal ) -> mPosition += advanceAmount'.
			// But no, that's not how it works here, the position is already advanced, so we're just kinda reverting it in 3 out of 4 possible cases.
			if ( !(advanceIfTrue && equal) )
			{
				StashPop();
			}

			return equal;
		}

		/// <summary>
		/// Is there anything other than whitespaces until the newline?
		/// </summary>
		public bool ExpectAnythingUntilNewline()
		{
			while ( !IsEnd() && !IsEndOfLine() )
			{
				IncrementPosition();
				if ( IsEndOfLine() )
				{
					break;
				}

				if ( mText[mPosition] != ' ' )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Skips characters until a given string <paramref name="what"/> has been found.
		/// If <paramref name="skipThatToo"/> is <c>true</c>, it'll skip that string as well.
		/// </summary>
		/// <param name="what"></param>
		/// <param name="skipThatToo"></param>
		public void SkipUntil( ReadOnlySpan<char> what, bool skipThatToo = false )
		{
			while ( !IsEnd() )
			{
				IncrementPosition();
				if ( mText.AsSpan().Slice( mPosition ).StartsWith( what ) )
				{
					if ( skipThatToo )
					{
						for ( int i = 0; i < what.Length; i++ )
						{
							IncrementPosition();
						}
					}

					return;
				}
			}
		}

		/// <summary>
		/// Whether or not the end of the text has been reached.
		/// </summary>
		public bool IsEnd()
		{
			return mPosition >= mText.Length;
		}

		/// <summary>
		/// Skip the current line.
		/// </summary>
		public void NewLine()
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

		/// <summary>
		/// Throws an exception with the attached <paramref name="errorMessage"/>.
		/// Includes line and column info.
		/// </summary>
		public Exception ParsingException( string errorMessage )
		{
			return new Exception( $"{errorMessage} {GetLineInfo()}" );
		}
		#endregion

		#region Properties
		/// <summary>
		/// Whether or not to ignore delimiters while scanning for characters.
		/// </summary>
		public bool IgnoreDelimiters { get; set; }
		#endregion

		#region Private methods
		private bool IsValidCharacter()
		{
			if ( IsEnd() )
			{
				return false;
			}
			
			// Anything is valid within quotes
			if ( mInQuote )
			{
				return true;
			}

			return !CanSkipCharacter();
		}

		private bool CanSkipCharacter()
		{
			// Whitespace, tabs and newlines can be skipped
			char c = mText[mPosition];
			if ( c is ' ' or '\t' or '\n' or '\r' )
			{
				return true;
			}
			
			// Everything else: alphanumerical, delimiters, quotes etc. are implied
			// Comments are also meant to be skipped
			return IsComment();
		}
		
		private void IncrementPosition()
		{
			if ( !IsEnd() )
			{
				if ( IsEndOfLine() )
				{
					mLineNumber++;
					mLineColumn = -1;
				}

				mPosition++;
				mLineColumn++;
			}
		}

		private bool IsComment()
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

		private bool IsQuote()
		{
			return mText[mPosition] == '"';
		}
		
		private bool IsDelimiter()
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

		private bool IsEndOfLine()
		{
			return mText[mPosition] == '\n' || mText[mPosition] == '\r';
		}

		private void ToggleQuoteMode()
		{
			mInQuote = !mInQuote;
		}

		private void StashPosition()
		{
			mPositionStash = mPosition;
			mLineNumberStash = mLineNumber;
			mLineColumnStash = mLineColumn;
			mInQuoteStash = mInQuote;
		}

		private void StashPop()
		{
			mPosition = mPositionStash;
			mLineNumber = mLineNumberStash;
			mLineColumn = mLineColumnStash;
			mInQuote = mInQuoteStash;
		}
		#endregion

		#region Private members
		private int mPosition;
		private int mLineNumber;
		private int mLineColumn;
		private bool mInQuote;

		private int mPositionStash;
		private int mLineNumberStash;
		private int mLineColumnStash;
		private bool mInQuoteStash;

		private string mText;
		private string mDelimiters;
		#endregion
	}
}
