using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;

namespace DisCatSharp.Common.Utilities;

/// <summary>
///     Provides a <see cref="WaveformVisualizer" />.
/// </summary>
public sealed class WaveformVisualizer
{
	/// <summary>
	///     Gets the default background color.
	/// </summary>
	private readonly Color _defaultBackgroundColor = Colors.Black;

	/// <summary>
	///     Gets the default colors for <see cref="CreateColorfulWaveformImage" />.
	/// </summary>
	private readonly Color[] _defaultColorfulColors = [Colors.DarkOrange, Colors.Orange, Colors.Gold, Colors.Yellow, Colors.LightBlue, Colors.Blue, Colors.BlueViolet, Colors.Pink, Colors.DeepPink, Colors.MediumVioletRed, Colors.Red];

	/// <summary>
	///     Gets the default colors for <see cref="CreateWaveformImage" />.
	/// </summary>
	private readonly Color[] _defaultColors = [Colors.LightBlue, Colors.LightSkyBlue, Colors.DeepSkyBlue];

	/// <summary>
	///     Gets the image.
	/// </summary>
	public IImage? Image { get; internal set; }

	/// <summary>
	///     Gets or sets the waveform byte data.
	/// </summary>
	private byte[]? WAVEFORM_BYTE_DATA { get; set; }

	/// <summary>
	///     Decodes the base 64 encoded waveform.
	/// </summary>
	/// <param name="base64Waveform">The base64 encoded waveform string.</param>
	/// <returns></returns>
	private static byte[] DecodeWaveform(string base64Waveform)
		=> Convert.FromBase64String(base64Waveform);

	/// <summary>
	///     Attaches the raw waveform data as <see cref="byte" /> array.
	/// </summary>
	/// <param name="waveformByteData">The waveforms byte array.</param>
	public WaveformVisualizer WithWaveformByteData(byte[] waveformByteData)
	{
		this.WAVEFORM_BYTE_DATA = waveformByteData;
		return this;
	}

	/// <summary>
	///     Attaches the raw waveform data as <see cref="byte" /> array.
	/// </summary>
	/// <param name="base64Waveform">The waveforms byte array encoded as base64.</param>
	public WaveformVisualizer WithWaveformData(string base64Waveform)
	{
		this.WAVEFORM_BYTE_DATA = DecodeWaveform(base64Waveform);
		return this;
	}

	/// <summary>
	///     Creates a simple waveform image.
	/// </summary>
	/// <param name="width">The width.</param>
	/// <param name="height">The height.</param>
	public WaveformVisualizer CreateWaveformImage(int width = 500, int height = 100)
	{
		ArgumentNullException.ThrowIfNull(this.WAVEFORM_BYTE_DATA, nameof(this.WithWaveformByteData));

		if (this.WAVEFORM_BYTE_DATA.Length is 0)
			throw new ArgumentException("Waveform data is empty.", nameof(this.WithWaveformByteData));

		var image = new PlatformBitmapExportService().CreateContext(width, height);
		var canvas = image.Canvas;
		canvas.FillColor = this._defaultBackgroundColor;
		canvas.FillRectangle(0, 0, width, height);

		var barWidth = (float)width / this.WAVEFORM_BYTE_DATA.Length / 2;
		var xScale = (float)width / this.WAVEFORM_BYTE_DATA.Length;
		var yScale = (float)height / 2 / 255;

		var gradientStops = this._defaultColors
			.Select((color, index) => new PaintGradientStop((float)index / (this._defaultColors.Length - 1), color))
			.ToArray();

		var gradient = new LinearGradientPaint(gradientStops, new PointF(0, 0), new PointF(0, height));
		canvas.SetFillPaint(gradient, new(0, 0, width, height));

		for (var i = 0; i < this.WAVEFORM_BYTE_DATA.Length; i++)
		{
			var x1 = i * xScale;
			var barHeight = Math.Max(2, this.WAVEFORM_BYTE_DATA[i] * yScale);
			var y1 = ((float)height / 2) - barHeight;
			var y2 = ((float)height / 2) + barHeight;

			canvas.FillRoundedRectangle(x1, y1, barWidth, barHeight * 2, barWidth / 4);

			canvas.SetShadow(new(2, 2), 5, this._defaultBackgroundColor.WithAlpha(0.5f));
		}

		this.Image = image.Image;

		return this;
	}

	/// <summary>
	///     Creates a colorful waveform image.
	/// </summary>
	/// <param name="width">The width.</param>
	/// <param name="height">The height.</param>
	/// <param name="backgroundColor">The background color.</param>
	/// <param name="barColors">The bar colors.</param>
	public WaveformVisualizer CreateColorfulWaveformImage(int width = 500, int height = 100, Color? backgroundColor = null, params Color[]? barColors)
	{
		ArgumentNullException.ThrowIfNull(this.WAVEFORM_BYTE_DATA, nameof(this.WithWaveformByteData));

		if (this.WAVEFORM_BYTE_DATA.Length is 0)
			throw new ArgumentException("Waveform data is empty.", nameof(this.WithWaveformByteData));

		backgroundColor ??= this._defaultBackgroundColor;
		barColors ??= this._defaultColorfulColors;

		if (barColors.Length is 0)
			barColors = this._defaultColorfulColors;

		var image = new PlatformBitmapExportService().CreateContext(width, height);
		var canvas = image.Canvas;
		canvas.FillColor = backgroundColor;
		canvas.FillRectangle(0, 0, width, height);

		var barWidth = (float)width / this.WAVEFORM_BYTE_DATA.Length / 2;
		var xScale = (float)width / this.WAVEFORM_BYTE_DATA.Length;
		var yScale = (float)height / 2 / 255;

		for (var i = 0; i < this.WAVEFORM_BYTE_DATA.Length; i++)
		{
			var x1 = i * xScale;
			var barHeight = Math.Max(2, this.WAVEFORM_BYTE_DATA[i] * yScale);
			var y1 = ((float)height / 2) - barHeight;
			var y2 = ((float)height / 2) + barHeight;

			var color1 = barColors[i % barColors.Length];
			var color2 = barColors[(i + 1) % barColors.Length];

			var gradientStops = new PaintGradientStop[] { new(0, color1), new(1, color2) };

			var gradient = new LinearGradientPaint(gradientStops, new PointF(x1, y1), new PointF(x1, y2));
			canvas.SetFillPaint(gradient, new(x1, y1, barWidth, barHeight * 2));

			canvas.FillRoundedRectangle(x1, y1, barWidth, barHeight * 2, barWidth / 4);

			canvas.SetShadow(new(2, 2), 5, backgroundColor.WithAlpha(0.5f));
		}

		this.Image = image.Image;

		return this;
	}

	/// <summary>
	///     Saves a waveform image to given <paramref name="filePath" />.
	/// </summary>
	/// <param name="filePath">The path, including the files name, to save the image to.</param>
	public async Task<WaveformVisualizer> SaveImageAsync(string filePath)
	{
		ArgumentNullException.ThrowIfNull(this.Image, nameof(this.Image));
		await using var stream = File.OpenWrite(filePath);
		await this.Image.SaveAsync(stream);
		return this;
	}

	/// <summary>
	///     Converts the waveform image to a <see cref="Stream" />.
	/// </summary>
	/// <param name="format">The image format. Defaults to <see cref="ImageFormat.Png" />.</param>
	public Stream AsStream(ImageFormat format = ImageFormat.Png)
		=> this.Image is not null
			? this.Image.AsStream(format)
			: throw new NullReferenceException("Image was null, did you even generate a waveform image?");
}
