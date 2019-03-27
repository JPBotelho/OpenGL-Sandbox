using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Drawing;
using System.Diagnostics;

namespace OpenTK_Test
{
	public sealed class MainWindow : GameWindow
	{
		bool firstMove = true;
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

		Skybox skybox;
		Model model;

		Camera cam;
		Shader shader;

		string[] skyboxFaces =
		{
			"resources/skybox/right.jpg",
			"resources/skybox/left.jpg",
			"resources/skybox/top.jpg",
			"resources/skybox/bottom.jpg",
			"resources/skybox/front.jpg",
			"resources/skybox/back.jpg"
		};
		
		public MainWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

		protected override void OnResize(EventArgs e)
		{
			cam.AspectRatio = (float)Width / Height;
			GL.Viewport(0, 0, Width, Height);
			base.OnResize(e);
		}
		protected override void OnLoad(EventArgs e)
		{
			skybox = new Skybox(skyboxFaces);
			model = new Model("C:/Users/User/source/repos/OpenTK Test/OpenTK Test/bin/Debug/resources/sponza/sponza.obj");
			cam = new Camera(Vector3.UnitZ * 3);
			cam.AspectRatio = (float)Width / Height;
			CursorVisible = false;
			
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Always);
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
			shader = new Shader("../../shader.vert", "../../shader.frag");
			base.OnLoad(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";
			shader.Use();
			Matrix4 view = cam.GetViewMatrix();
			Matrix4 proj = cam.GetProjectionMatrix();
			shader.SetMatrix4("viewMatrix", view);
			shader.SetMatrix4("projMatrix", proj);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			model.Draw(shader);

			GL.DepthFunc(DepthFunction.Lequal);
			skybox.Draw(view, proj);
			GL.DepthFunc(DepthFunction.Less);

			Context.SwapBuffers();
			
			base.OnRenderFrame(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			HandleKeyboard(e);
			base.OnUpdateFrame(e);
		}

		private void HandleKeyboard(FrameEventArgs e)
		{
			float deltatime = (float)(1f / e.Time);
			var input = Keyboard.GetState();

			if (input.IsKeyDown(Key.Escape))
				Exit();
			if (input.IsKeyDown(Key.W))
				cam.Position += cam.Front * cam.Speed * (float)e.Time; //Forward 
			if (input.IsKeyDown(Key.S))
				cam.Position -= cam.Front * cam.Speed * (float)e.Time; //Backwards
			if (input.IsKeyDown(Key.A))
				cam.Position -= cam.Right * cam.Speed * (float)e.Time; //Left
			if (input.IsKeyDown(Key.D))
				cam.Position += cam.Right * cam.Speed * (float)e.Time; //Right
			if (input.IsKeyDown(Key.Space))
				cam.Position += cam.Up * cam.Speed * (float)e.Time; //Up 
			if (input.IsKeyDown(Key.LShift))
				cam.Position -= cam.Up * cam.Speed * (float)e.Time; //Down
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			if (firstMove)
			{
				firstMove = false;
				base.OnMouseMove(e);
				return;
			}

			cam.Pitch -= e.YDelta * cam.Sensitivity;
			cam.Yaw += e.XDelta * cam.Sensitivity;

			base.OnMouseMove(e);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			cam.Fov -= e.DeltaPrecise;
			base.OnMouseWheel(e);
		}

		protected override void OnFocusedChanged(EventArgs e)
		{
			base.OnFocusedChanged(e);
		}

		protected override void OnUnload(EventArgs e)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			shader.Dispose();
			skybox.shader.Dispose();
			base.OnUnload(e);
		}

	}
}
