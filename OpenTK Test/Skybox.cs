using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenTK_Test
{
	class Skybox
	{
		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		public static void CheckLastError()
		{
			ErrorCode errorCode = GL.GetError();
			if (errorCode != ErrorCode.NoError)
			{
				throw new Exception(errorCode.ToString());
			}
		}
		private float[] skyboxVertices = {
			// positions          
			-1.0f,  1.0f, -1.0f,
			-1.0f, -1.0f, -1.0f,
			 1.0f, -1.0f, -1.0f,
			 1.0f, -1.0f, -1.0f,
			 1.0f,  1.0f, -1.0f,
			-1.0f,  1.0f, -1.0f,

			-1.0f, -1.0f,  1.0f,
			-1.0f, -1.0f, -1.0f,
			-1.0f,  1.0f, -1.0f,
			-1.0f,  1.0f, -1.0f,
			-1.0f,  1.0f,  1.0f,
			-1.0f, -1.0f,  1.0f,

			 1.0f, -1.0f, -1.0f,
			 1.0f, -1.0f,  1.0f,
			 1.0f,  1.0f,  1.0f,
			 1.0f,  1.0f,  1.0f,
			 1.0f,  1.0f, -1.0f,
			 1.0f, -1.0f, -1.0f,

			-1.0f, -1.0f,  1.0f,
			-1.0f,  1.0f,  1.0f,
			 1.0f,  1.0f,  1.0f,
			 1.0f,  1.0f,  1.0f,
			 1.0f, -1.0f,  1.0f,
			-1.0f, -1.0f,  1.0f,

			-1.0f,  1.0f, -1.0f,
			 1.0f,  1.0f, -1.0f,
			 1.0f,  1.0f,  1.0f,
			 1.0f,  1.0f,  1.0f,
			-1.0f,  1.0f,  1.0f,
			-1.0f,  1.0f, -1.0f,

			-1.0f, -1.0f, -1.0f,
			-1.0f, -1.0f,  1.0f,
			 1.0f, -1.0f, -1.0f,
			 1.0f, -1.0f, -1.0f,
			-1.0f, -1.0f,  1.0f,
			 1.0f, -1.0f,  1.0f
		};
		private int vbo, vao;
		private string[] faces;
		public Shader shader;
		private int cubemapID;

		public Skybox(string[] faces)
		{		
			if (faces.Length != 6)
				throw new ArgumentException("Skybox constructors require exactly six textures.");
			this.faces = faces;
			LoadCubemap(faces);
			Setup();
		}

		public void Draw(Matrix4 mvp)
		{
			shader.Use();
			shader.SetMatrix4("mvp", mvp);
			GL.BindVertexArray(vao);
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.TextureCubeMap, cubemapID);

			GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
		}

		private void Setup()
		{
			vbo = GL.GenBuffer();
			vao = GL.GenVertexArray();
			GL.BindVertexArray(vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * skyboxVertices.Length, skyboxVertices, BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

			shader = new Shader("../../skybox.vs", "../../skybox.fs");
			shader.Use();
			shader.SetInt("skybox", 0);
		}

		private int LoadCubemap(string[] faces)
		{
			cubemapID = GL.GenTexture();
			GL.BindTexture(TextureTarget.TextureCubeMap, cubemapID);

			for (int i = 0; i < faces.Length; i++)
			{
				Image<Rgba32> image = Image.Load("C:/Users/User/source/repos/OpenTK Test/OpenTK Test/bin/Debug/" + faces[i]);
				Console.WriteLine("C:/Users/User/source/repos/OpenTK Test/OpenTK Test/bin/Debug/" + faces[i]);
				image.Mutate(x => x.Flip(FlipMode.Vertical));
				Rgba32[] tempPixels = image.GetPixelSpan().ToArray();
				List<byte> pixels = new List<byte>();
				foreach (Rgba32 p in tempPixels)
				{
					pixels.Add(p.R);
					pixels.Add(p.G);
					pixels.Add(p.B);
					pixels.Add(p.A);
				}
				Console.WriteLine(pixels.Count);

				GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
				Console.WriteLine(i);
			}

			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
			return cubemapID;
		}
	}
}
