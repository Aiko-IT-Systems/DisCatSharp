using System;

namespace DisCatSharp.Ascii;

internal interface IImageSource : IDisposable
{
	int Width { get; }

	int Height { get; }

	float AspectRatio { get; }

	Rgb GetPixel(int x, int y);
}
