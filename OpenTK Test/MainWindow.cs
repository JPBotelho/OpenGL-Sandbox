﻿using System;
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
		Model model;
		Vector2 lastMousePos = new Vector2();

		Camera cam;
		Matrix4 ViewProjectionMatrix;
		Shader shader;
		
		public MainWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

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

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
			CheckLastError();
			base.OnResize(e);
		}
		protected override void OnLoad(EventArgs e)
		{
			model = new Model("C:/Users/User/source/repos/OpenTK Test/OpenTK Test/bin/Debug/resources/nanosuit.obj");
			cam = new Camera(this);
			lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
			CursorVisible = false;

			GL.Enable(EnableCap.DepthTest);
			CheckLastError();
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
			CheckLastError();
			shader = new Shader("../../shader.vert", "../../shader.frag");
			base.OnLoad(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";
			shader.Use();
			ViewProjectionMatrix = cam.GetViewMatrix() * cam.ProjectionMatrix;
			shader.SetMatrix4("mvp", ViewProjectionMatrix);
			CheckLastError();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			CheckLastError();
			model.Draw(shader);
			Context.SwapBuffers();
			
			base.OnRenderFrame(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			HandleKeyboard();
			base.OnUpdateFrame(e);
		}

		private void HandleKeyboard()
		{
			var keyState = Keyboard.GetState();

			if (keyState.IsKeyDown(Key.Escape))
			{
				Exit();
			}
			if (Keyboard.GetState().IsKeyDown(Key.W))
			{
				cam.Move(0f, 0.1f, 0f);
			}

			if (Keyboard.GetState().IsKeyDown(Key.S))
			{
				cam.Move(0f, -0.1f, 0f);
			}

			if (Keyboard.GetState().IsKeyDown(Key.A))
			{
				cam.Move(-0.1f, 0f, 0f);
			}

			if (Keyboard.GetState().IsKeyDown(Key.D))
			{
				cam.Move(0.1f, 0f, 0f);
			}

			if (Keyboard.GetState().IsKeyDown(Key.Q))
			{
				cam.Move(0f, 0f, 0.1f);
			}

			if (Keyboard.GetState().IsKeyDown(Key.E))
			{
				cam.Move(0f, 0f, -0.1f);
			}

			if (Focused)
			{
				Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
				lastMousePos += delta;

				cam.AddRotation(delta.X, delta.Y);
				lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
			}

		}

		protected override void OnFocusedChanged(EventArgs e)
		{
			base.OnFocusedChanged(e);
			lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
		}

		protected override void OnUnload(EventArgs e)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			shader.Dispose();
			base.OnUnload(e);
		}

	}
}
