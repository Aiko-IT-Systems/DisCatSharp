using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DisCatSharp.Ascii;

internal sealed class ImageSharpImageSource : IImageSource
{
	private readonly Image<Rgba32> _image;

	public ImageSharpImageSource(Image<Rgba32> image)
	{
		this._image = image;
	}

	public int Width => this._image.Width;

	public int Height => this._image.Height;

	public float AspectRatio => this._image.Width / (float)this._image.Height;

	public Rgb GetPixel(int x, int y)
	{
		var pixel = this._image[x, y];
		return new(
			pixel.R,
			pixel.G,
			pixel.B);
	}

	public void Dispose() => this._image.Dispose();
}
