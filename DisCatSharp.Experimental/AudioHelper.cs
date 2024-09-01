using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Concentus;
using Concentus.Oggfile;

namespace DisCatSharp.Experimental;

/// <summary>
///     Represents various <see cref="AudioHelper" />s.
/// </summary>
public static class AudioHelper
{
	/// <summary>
	///     Gets the audio duration in seconds.
	/// </summary>
	/// <param name="opusStream">The audio stream.</param>
	/// <returns>The duration in seconds.</returns>
	public static (Stream Stream, float DurationSeconds) GetOpusAudioDurationInSeconds(this Stream opusStream)
	{
		opusStream.Seek(0, SeekOrigin.Begin);

		float totalSeconds = 0;

		using var decoder = OpusCodecFactory.CreateDecoder(48000, 1);
		var oggIn = new OpusOggReadStream(decoder, opusStream);

		while (oggIn.HasNextPacket)
		{
			var packet = oggIn.DecodeNextPacket();
			if (packet != null)
				totalSeconds += packet.Length / (48000f * 1);
		}

		opusStream.Seek(0, SeekOrigin.Begin);

		return totalSeconds > 1200
			? throw new InvalidOperationException("Voice message duration exceeds the maximum allowed length of 20 minutes.")
			: (opusStream, totalSeconds);
	}

	/// <summary>
	///     Generates the waveform data.
	/// </summary>
	/// <param name="opusStream">The audio stream.</param>
	/// <param name="maxWaveformSize">The maximal waveform size.</param>
	/// <returns>The waveform as a byte array.</returns>
	public static (Stream Stream, byte[] Waveform) GenerateWaveformBytes(this Stream opusStream, int maxWaveformSize = 200)
	{
		opusStream.Seek(0, SeekOrigin.Begin);

		using var decoder = OpusCodecFactory.CreateDecoder(48000, 1);
		var oggIn = new OpusOggReadStream(decoder, opusStream);

		var waveformBytes = new byte[maxWaveformSize];
		var samples = new float[maxWaveformSize];
		var totalSamples = 0;

		while (oggIn.HasNextPacket)
		{
			var packet = oggIn.DecodeNextPacket();
			if (packet == null)
				continue;

			foreach (var t in packet)
			{
				samples[totalSamples % maxWaveformSize] += Math.Abs(t);
				totalSamples++;
			}
		}

		for (var i = 0; i < maxWaveformSize; i++)
			waveformBytes[i] = (byte)(samples[i] / totalSamples * 255);

		opusStream.Seek(0, SeekOrigin.Begin);

		return (opusStream, waveformBytes);
	}

	/// <summary>
	///     Generates the waveform bytes and calculates the duration in seconds.
	/// </summary>
	/// <param name="opusStream">The audio stream.</param>
	/// <returns>The generated data.</returns>
	public static (Stream Stream, float DurationSeconds, byte[] Waveform) GetDurationAndWaveformBytes(this Stream opusStream)
	{
		var (durationStream, duration) = GetOpusAudioDurationInSeconds(opusStream);
		var (waveStream, waveform) = GenerateWaveformBytes(durationStream);

		return (waveStream, duration, waveform);
	}

	/// <summary>
	///     Converts an input stream to Discord's expected voice message format.
	/// </summary>
	/// <param name="inputStream">The audio source stream.</param>
	/// <returns>The audio result stream.</returns>
	public static async Task<Stream> ConvertToOggOpusAsync(this Stream inputStream)
	{
		inputStream.Seek(0, SeekOrigin.Begin);
		var outputStream = new MemoryStream();
		var tempFileName = Path.GetTempFileName();
		await using (var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
		{
			await inputStream.CopyToAsync(fileStream);
		}

		var ffmpeg = new ProcessStartInfo
		{
			FileName = "ffmpeg",
			Arguments = $"-i {tempFileName} -ar 48000 -c:a libopus -b:a 28k -ac 1 -compression_level 10 -buffer_size 2048000 -f ogg -threads 8 -err_detect ignore_err pipe:1",
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		Console.WriteLine("Starting FFmpeg process...");
		var process = Process.Start(ffmpeg) ?? throw new InvalidOperationException("Failed to start FFmpeg process");

		try
		{
			process.ErrorDataReceived += (s, e) =>
			{
				if (!string.IsNullOrWhiteSpace(e.Data))
					Console.WriteLine($"FFmpeg error: {e.Data}");
			};

			Console.WriteLine("Copying input stream to FFmpeg...");
			await inputStream.CopyToAsync(process.StandardInput.BaseStream);
			process.StandardInput.Close();

			Console.WriteLine("Reading output stream from FFmpeg...");
			await process.StandardOutput.BaseStream.CopyToAsync(outputStream);
			process.StandardOutput.Close();

			Console.WriteLine("Waiting for FFmpeg process to exit...");
			var processTask = process.WaitForExitAsync();
			if (await Task.WhenAny(processTask, Task.Delay(TimeSpan.FromMinutes(5))) == processTask)
				Console.WriteLine("FFmpeg process completed successfully.");
			else
			{
				process.Kill();
				throw new TimeoutException("FFmpeg process timed out.");
			}

			if (process.ExitCode != 0)
				throw new InvalidOperationException($"FFmpeg process failed with exit code {process.ExitCode}.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"An error occurred during the FFmpeg process: {ex.Message}");
			throw;
		}
		finally
		{
			outputStream.Seek(0, SeekOrigin.Begin);
			process.Dispose();
			await inputStream.DisposeAsync();
			File.Delete(tempFileName);
		}

		return outputStream;
	}
}
