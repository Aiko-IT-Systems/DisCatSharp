// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// ReSharper disable InconsistentNaming
namespace DisCatSharp
{
	/// <summary>
	/// Represents the activity this invite is for.
	/// </summary>
	public enum TargetActivity : ulong
	{
		/// <summary>
		/// Represents no embedded application.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents the embedded application Betrayal.io.
		/// </summary>
		Betrayal = 773336526917861400,

		/// <summary>
		/// Represents the embedded application Chess in the park.
		/// Dev?
		/// </summary>
		ChessInThePark = 832012774040141894,

		/// <summary>
		/// Represents the embedded application Doodle Crew.
		/// </summary>
		DoodleCrew = 878067389634314250,

		/// <summary>
		/// Represents the embedded application Fishington.io.
		/// </summary>
		Fishington = 814288819477020702,

		/// <summary>
		/// Represents the embedded application Letter Tile.
		/// </summary>
		LetterTile = 879863686565621790,

		/// <summary>
		/// Represents the embedded application Poker Night.
		/// </summary>
		PokerNight = 755827207812677713,

		/// <summary>
		/// Represents the embedded application Spell Cast.
		/// </summary>
		SpellCast = 852509694341283871,

		/// <summary>
		/// Represents the embedded application Watch Together.
		/// </summary>
		WatchTogether = 880218394199220334,

		/// <summary>
		/// Represents the embedded application Watch Together.
		/// This is the dev version.
		/// </summary>
		WatchTogetherDev = 880218832743055411,

		/// <summary>
		/// Represents the embedded application Word Snacks.
		/// </summary>
		WordSnacks = 879863976006127627,

		/// <summary>
		/// Represents the embedded application YouTube Together.
		/// </summary>
		YouTubeTogether = 755600276941176913,

		#region New Prod
		/// <summary>
		/// Represents the embedded application Awkword.
		/// </summary>
		Awkword = 879863881349087252,

		/// <summary>
		/// Represents the embedded application Putts.
		/// </summary>
		Putts = 832012854282158180,

		/// <summary>
		/// Represents the embedded application CG3 Prod.
		/// </summary>
		CG3Prod = 832013003968348200,

		/// <summary>
		/// Represents the embedded application CG4 Prod.
		/// </summary>
		CG4Prod = 832025144389533716,

		/// <summary>
		/// Represents the embedded application Sketchy Artist.
		/// </summary>
		SketchyArtist = 879864070101172255,
		#endregion

		#region New Dev
		/// <summary>
		/// Represents the embedded application Sketchy Artist.
		/// This is the dev version.
		/// </summary>
		SketchyArtistDev = 879864104980979792,

		/// <summary>
		/// Represents the embedded application Word Snacks.
		/// This is the dev version.
		/// </summary>
		WordSnacksDev = 879864010126786570,

		/// <summary>
		/// Represents the embedded application Doodle Crew.
		/// This is the dev version.
		/// </summary>
		DoodleCrewDev = 878067427668275241,

		/// <summary>
		/// Represents the embedded application Chess in the park.
		/// This is the dev version.
		/// </summary>
		ChessInTheParkDev = 832012586023256104,

		/// <summary>
		/// Represents the embedded application CG3 Dev.
		/// This is the dev version.
		/// </summary>
		CG3Dev = 832012682520428625,

		/// <summary>
		/// Represents the embedded application CG4 Dev.
		/// This is the dev version.
		/// </summary>
		CG4kDev = 832013108234289153,

		/// <summary>
		/// Represents the embedded application Decoders.
		/// This is the dev version.
		/// </summary>
		DecodersDev = 891001866073296967,
		#endregion

		#region New Staging
		/// <summary>
		/// Represents the embedded application PN.
		/// This is the staging version.
		/// </summary>
		PNStaging = 763116274876022855,

		/// <summary>
		/// Represents the embedded application CG2.
		/// This is the staging version.
		/// </summary>
		CG2Staging = 832012730599735326,

		/// <summary>
		/// Represents the embedded application CG3.
		/// This is the staging version.
		/// </summary>
		CG3Staging = 832012938398400562,

		/// <summary>
		/// Represents the embedded application CG4.
		/// This is the staging version.
		/// </summary>
		CG4Staging = 832025061657280566,
		#endregion

		#region New QA
		/// <summary>
		/// Represents the embedded application Poker.
		/// This is the QA version.
		/// </summary>
		PokerQA = 801133024841957428,

		/// <summary>
		/// Represents the embedded application CG2.
		/// This is the QA version.
		/// </summary>
		CG2QA = 832012815819604009,

		/// <summary>
		/// Represents the embedded application CG 3.
		/// This is the QA version.
		/// </summary>
		CG3QA = 832012894068801636,

		/// <summary>
		/// Represents the embedded application CG 4.
		/// This is the QA version.
		/// </summary>
		CG4QA = 832025114077298718,
		#endregion
	}
}
