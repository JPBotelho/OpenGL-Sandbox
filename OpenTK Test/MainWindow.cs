using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Drawing;
namespace OpenTK_Test
{
	public sealed class MainWindow : GameWindow
	{
		Shader shader;
		Texture texture, texture1;
		private int vbo, ebo, vao;
		float[] vertices =
	    {
            //Position          Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left 
        };
		uint[] indices = {  // note that we start from 0!
			0, 1, 3,   // first triangle
			1, 2, 3    // second triangle
		};
		public MainWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }


		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
			base.OnResize(e);
		}
		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

			vbo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			ebo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);


			//The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
			shader = new Shader("../../shader.vert", "../../shader.frag");
			shader.Use();


			texture = new Texture("../../container.png");
			texture.Use(TextureUnit.Texture0);
			texture1 = new Texture("../../awesomeface.png");
			texture1.Use(TextureUnit.Texture1);

			shader.SetInt("texture0", 0);
			shader.SetInt("texture1", 1);



			vao = GL.GenVertexArray();
			GL.BindVertexArray(vao);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

			//Because there's now 5 floats between the start of the first vertex and the start of the second,
			//we modify this from 3 * sizeof(float) to 5 * sizeof(float).
			//This will now pass the new vertex array to the buffer.
			int vertexLocation = shader.GetAttribLocation("vPos");
			GL.EnableVertexAttribArray(vertexLocation);
			GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
	
			//Next, we also setup texture coordinates. It works in much the same way.
			//We add an offset of 3, since the first vertex coordinate comes after the first vertex
			//and change the amount of data to 2 because there's only 2 floats for vertex coordinates
			int texCoordLocation = shader.GetAttribLocation("texcoord");
			GL.EnableVertexAttribArray(texCoordLocation);
			GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

			base.OnLoad(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.BindVertexArray(vao);

			texture.Use(TextureUnit.Texture0);
			texture1.Use(TextureUnit.Texture1);
			shader.Use();

			GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
			Context.SwapBuffers();
			base.OnRenderFrame(e);
		}

		private void HandleKeyboard()
		{
			var keyState = Keyboard.GetState();

			if (keyState.IsKeyDown(Key.Escape))
			{
				Exit();
			}
		}

		protected override void OnUnload(EventArgs e)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.DeleteBuffer(vbo);
			GL.DeleteBuffer(ebo);
			shader.Dispose();
			texture.Dispose();
			texture1.Dispose();
			base.OnUnload(e);
		}

	}
}
