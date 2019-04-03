using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_Test
{

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct Vertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 TexCoords;
		public Vector3 Tangent;
		public Vector3 Bitangent;
	};


	struct TextureInfo
	{
		public uint id;
		public string type;
		public string path;
	};

	class Mesh
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
		public Vertex[] vertices;
		public uint[] indices;
		public TextureInfo[] textures;
		private int vbo, vao, ebo;

		public Mesh(Vertex[] vertices, uint[] indices, TextureInfo[] textures)
		{
			this.vertices = vertices;
			this.indices = indices;
			this.textures = textures;
			SetupMesh();
		}

		public void Draw(Shader shader)
		{
			shader.Use();
			uint diffuseNr = 1;
			uint specularNr = 1;
			uint normalNr = 1;
			uint heightNr = 1;
			for (int i = 0; i < textures.Length; i++)
			{
				GL.ActiveTexture(TextureUnit.Texture0 + i); // active proper texture unit before binding
															// retrieve texture number (the N in diffuse_textureN)
				string number = "";
				string name = textures[i].type;
				if (name == "texture_diffuse")
					number = diffuseNr++.ToString();
				else if (name == "texture_specular")
					number = specularNr++.ToString(); // transfer unsigned int to stream
				else if (name == "texture_normal")
					number = normalNr++.ToString(); // transfer unsigned int to stream
				else if (name == "texture_height")
					number = heightNr++.ToString(); // transfer unsigned int to stream

				// now set the sampler to the correct texture unit
				GL.Uniform1(GL.GetUniformLocation(shader.Handle, (name + number)), i);
				// and finally bind the texture
				GL.BindTexture(TextureTarget.Texture2D, textures[i].id);
			}

			GL.ActiveTexture(TextureUnit.Texture0);

			// draw mesh
			GL.BindVertexArray(vao);
			GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
			GL.BindVertexArray(0);
		}


		private void SetupMesh()
		{
			unsafe
			{
				vao = GL.GenVertexArray();
				vbo = GL.GenBuffer();
				ebo = GL.GenBuffer();

				GL.BindVertexArray(vao);

				GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
				GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(Vertex), vertices, BufferUsageHint.StaticDraw);

				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
				GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);


				// set the vertex attribute pointers
				// vertex Positions
				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 0);
				// vertex normals
				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 12);
				// vertex texture coords
				GL.EnableVertexAttribArray(2);
				GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), 24);
				// vertex tangent
				GL.EnableVertexAttribArray(3);
				GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 32);
				// vertex bitangent
				GL.EnableVertexAttribArray(4);
				GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 44);

				GL.BindVertexArray(0);
			}
		}

	}
}
