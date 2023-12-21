using System.Text;

namespace DisCatSharp.Ascii;

internal sealed class Generator
{
	public AsciiArt GenerateAsciiArtFromImage(
		IImageSource image
	)
	{
		var asciiChars = "@%#*+=-:,. ";

		var aspect = image.Width / (double)image.Height;
		var outputWidth = image.Width / 16;
		var widthStep = image.Width / outputWidth;
		var outputHeight = (int)(outputWidth / aspect);
		var heightStep = image.Height / outputHeight;

		StringBuilder asciiBuilder = new(outputWidth * outputHeight);
		for (var h = 0; h < image.Height; h += heightStep)
		{
			for (var w = 0; w < image.Width; w += widthStep)
			{
				var pixelColor = image.GetPixel(w, h);
				var grayValue = (int)((pixelColor.Red * 0.3) + (pixelColor.Green * 0.59) + (pixelColor.Blue * 0.11));
				var asciiChar = asciiChars[grayValue * (asciiChars.Length - 1) / 255];
				asciiBuilder.Append(asciiChar);
				asciiBuilder.Append(asciiChar);
			}

			asciiBuilder.AppendLine();
		}

		AsciiArt art = new(
			asciiBuilder.ToString(),
			outputWidth,
			outputHeight);
		return art;
	}
}
