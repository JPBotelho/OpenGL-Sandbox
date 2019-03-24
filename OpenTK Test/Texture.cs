using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenTK_Test
{
	//A helper class, much like Shader, meant to simplify loading textures.
	public class Texture : IDisposable
	{
		int Handle;

		//Create texture from path.
		public Texture(string path)
		{
			Handle = GL.GenTexture();			
			Use();

			Image<Rgba32> image = Image.Load(path);

			//ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
			//This will correct that, making the texture display properly.
			image.Mutate(x => x.Flip(FlipMode.Vertical));

			//Get an array of the pixels, in ImageSharp's internal format.
			Rgba32[] tempPixels = image.GetPixelSpan().ToArray();
			
			List<byte> pixels = new List<byte>();

			foreach (Rgba32 p in tempPixels)
			{
				pixels.Add(p.R);
				pixels.Add(p.G);
				pixels.Add(p.B);
				pixels.Add(p.A);
			}

			
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);


			//Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
			//We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());


			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
		}

		public void Use(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2D, Handle);
		}

		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				GL.DeleteProgram(Handle);
				disposedValue = true;
			}
		}

		~Texture()
		{
			GL.DeleteProgram(Handle);
		}


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}