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
namespace DisCatSharp.Enums;

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
	/// Represents the embedded application Fishington.io.
	/// </summary>
	Fishington = 814288819477020702,

	/// <summary>
	/// Represents the embedded application Sketch Heads.
	/// </summary>
	SketchHeads = 902271654783242291,

	/// <summary>
	/// Represents the embedded application Letter League.
	/// </summary>
	LetterLeague = 879863686565621790,

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
	/// Represents the embedded application Word Snacks.
	/// </summary>
	WordSnacks = 879863976006127627,

	/// <summary>
	/// Represents the embedded application YouTube Together.
	/// </summary>
	YouTubeTogether = 755600276941176913,

	/// <summary>
	/// Represents the embedded application Putt Party.
	/// </summary>
	PuttParty = 945737671223947305,

	/// <summary>
	/// Represents the embedded application Land-io.
	/// </summary>
	LandIo = 903769130790969345,

	/// <summary>
	/// Represents the embedded application Bobble League.
	/// </summary>
	BobbleLeague = 947957217959759964,

	/// <summary>
	/// Represents the embedded application Ask Away.
	/// </summary>
	AskAway = 976052223358406656,

	/// <summary>
	/// Represents the embedded application Know What I Meme.
	/// </summary>
	KnowWhatIMeme = 950505761862189096,

	/// <summary>
	/// Represents the embedded application Bash Out.
	/// </summary>
	BashOut = 1006584476094177371,


	#region New Prod
	/// <summary>
	/// Represents the embedded application Checkers In The Park.
	/// </summary>
	CheckersInThePark = 832013003968348200,

	/// <summary>
	/// Represents the embedded application Blazing 8s.
	/// </summary>
	Blazing8s = 832025144389533716,
	#endregion

	#region New Dev
	/// <summary>
	/// Represents the embedded application Word Snacks.
	/// This is the dev version.
	/// </summary>
	WordSnacksDev = 879864010126786570,

	/// <summary>
	/// Represents the embedded application CG 2 Dev.
	/// This is the dev version.
	/// </summary>
	CG2Dev = 832012586023256104,

	/// <summary>
	/// Represents the embedded application CG3 Dev.
	/// This is the dev version.
	/// </summary>
	CG3Dev = 832012682520428625,

	/// <summary>
	/// Represents the embedded application CG4 Dev.
	/// This is the dev version.
	/// </summary>
	CG4Dev = 832013108234289153,


	/// <summary>
	/// Represents the embedded application PN Dev.
	/// This is the dev version.
	/// </summary>
	PNDev = 763133495793942528,

	/// <summary>
	/// Represents the embedded application Watch Together Dev.
	/// This is the dev version.
	/// </summary>
	WatchTogetherDev = 880218832743055411,

	/// <summary>
	/// Represents the embedded application Sketch Heads Dev.
	/// This is the dev version.
	/// </summary>
	SketchHeadsDev = 902271746701414431,

	/// <summary>
	/// Represents the embedded application Better League Dev.
	/// This is the dev version.
	/// </summary>
	BetterLeagueDev = 879863753519292467,

	/// <summary>
	/// Represents the embedded application Putt Party Dev.
	/// This is the dev version.
	/// </summary>
	PuttPartyDev = 910224161476083792,
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

	/// <summary>
	/// Represents the embedded application Putt Party.
	/// This is the staging version.
	/// </summary>
	PuttPartyStaging = 945732077960188005,

	/// <summary>
	/// Represents the embedded application Spell Cast.
	/// This is the staging version.
	/// </summary>
	SpellCastStaging = 893449443918086174,
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

	/// <summary>
	/// Represents the embedded application Putt Party.
	/// This is the QA version.
	/// </summary>
	PuttPartyQA = 945748195256979606
	#endregion
}
