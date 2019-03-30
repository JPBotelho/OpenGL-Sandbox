using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace OpenTK_Test
{
	public class Shader : IDisposable
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
		public int Handle;

		public Shader(string vertexPath, string fragmentPath, string geometryPath = null)
		{
			int VertexShader, FragmentShader, GeometryShader = 0;
			
			string VertexShaderSource;

			using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
			{
				VertexShaderSource = reader.ReadToEnd();
			}

			string FragmentShaderSource;

			using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
			{
				FragmentShaderSource = reader.ReadToEnd();
			}

			string GeometryShaderSource;

			if (geometryPath != null)
			{
				using (StreamReader reader = new StreamReader(geometryPath, Encoding.UTF8))
				{
					GeometryShaderSource = reader.ReadToEnd();
				}

				GeometryShader = GL.CreateShader(ShaderType.GeometryShader);
				GL.ShaderSource(GeometryShader, GeometryShaderSource);
				GL.CompileShader(GeometryShader);

				string log = GL.GetShaderInfoLog(GeometryShader);
				if (log != "")
					Console.WriteLine(log);
			}

			VertexShader = GL.CreateShader(ShaderType.VertexShader);
			GL.ShaderSource(VertexShader, VertexShaderSource);

			FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(FragmentShader, FragmentShaderSource);

			GL.CompileShader(VertexShader);

			string infoLogVert = GL.GetShaderInfoLog(VertexShader);
			if (infoLogVert != "")
				Console.WriteLine(infoLogVert);

			GL.CompileShader(FragmentShader);

			string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);

			if (infoLogFrag != "")
				Console.WriteLine(infoLogFrag);

			Handle = GL.CreateProgram();

			GL.AttachShader(Handle, VertexShader);
			GL.AttachShader(Handle, FragmentShader);
			if (geometryPath != null)
			{
				GL.AttachShader(Handle, GeometryShader);
			}

			GL.LinkProgram(Handle);
		}

		public void Use()
		{
			GL.UseProgram(Handle);
		}

		public void SetFloat(string name, float value)
		{
			GL.UseProgram(Handle);
			GL.Uniform1(GL.GetUniformLocation(Handle, name), value);
		}

		public void SetInt(string name, int value)
		{
			GL.UseProgram(Handle);
			GL.Uniform1(GL.GetUniformLocation(Handle, name), value);
		}

		public void SetMatrix4(string name, Matrix4 data)
		{
			GL.UseProgram(Handle);
			var location = GL.GetUniformLocation(Handle, name);
			GL.UniformMatrix4(location, true, ref data);
		}

		public void SetVec3(string name, Vector3 data)
		{
			GL.UseProgram(Handle);
			var location = GL.GetUniformLocation(Handle, name);
			GL.Uniform3(location, data);
		}

		public int GetAttribLocation(string attribName)
		{
			return GL.GetAttribLocation(Handle, attribName);
		}

		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				GL.DeleteProgram(Handle);

				disposedValue = true;
			}
		}

		~Shader()
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
