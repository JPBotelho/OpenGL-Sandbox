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
		Shader shader, depthShader;
		Vector2 lastPos;
		PointLight pointLight;
		DirectionalLight directionalLight;
		Spotlight spotlight;
		Matrix4 shadowProj;
		string[] skyboxFaces =
		{
			"resources/skybox/right.jpg",
			"resources/skybox/left.jpg",
			"resources/skybox/top.jpg",
			"resources/skybox/bottom.jpg",
			"resources/skybox/front.jpg",
			"resources/skybox/back.jpg"
		};

		int shadowWidth = 2048*2, shadowHeight = 2048*2;
		int depthMapFBO, depthCubemap;
		public MainWindow(int width, int height, GraphicsMode mode, string title) : base(width, height, mode, title) { }

		protected override void OnResize(EventArgs e)
		{
			cam.AspectRatio = (float)Width / Height;
			GL.Viewport(0, 0, Width, Height);
			base.OnResize(e);
		}
		protected override void OnLoad(EventArgs e)
		{
			VSync = VSyncMode.Off;
			//skybox = new Skybox(skyboxFaces);
			shadowProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), (float)shadowWidth / shadowHeight, 0.01f, 300f);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			CheckLastError();
			CursorVisible = false;

			shader = new Shader("shader.vert", "shader.frag");
			depthShader = new Shader("depth.vs", "depth.fs", "depth.gs");
			model = new Model("C:/Users/User/source/repos/OpenTK Test/OpenTK Test/bin/Debug/resources/sponza/sponza.obj");

			cam = new Camera(Vector3.UnitZ * 3);
			cam.AspectRatio = (float)Width / Height;

			pointLight = new PointLight
			{
				position = new Vector3(0, 5, 3)
			};

			depthCubemap = GL.GenTexture();
			GL.BindTexture(TextureTarget.TextureCubeMap, depthCubemap);

			for (int i = 0; i < 6; i++)
			{
				GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent32f, shadowWidth, shadowHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
				CheckLastError();
			}
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
			
			depthMapFBO = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthCubemap, 0);
			GL.DrawBuffer(DrawBufferMode.None);
			GL.ReadBuffer(ReadBufferMode.None);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			base.OnLoad(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";
			GL.ClearColor(Color.DeepPink);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			GL.Viewport(0, 0, shadowWidth, shadowHeight);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
			GL.Clear(ClearBufferMask.DepthBufferBit);

			float near_plane = 0.01f;
			float far_plane = 300f;
			Vector3 lightPos = pointLight.position;
			
			Matrix4[] shadowTransforms = new Matrix4[]
			{
				Matrix4.LookAt(lightPos, lightPos + new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
				Matrix4.LookAt(lightPos, lightPos + new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
				Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)) * shadowProj,
				Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)) * shadowProj,
				Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
				Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj
			};
			
			depthShader.Use();
			for (int i = 0; i < 6; ++i)
				depthShader.SetMatrix4("shadowMatrices[" + i + "]", shadowTransforms[i]);
			depthShader.SetVec3("lightPos", lightPos);
			depthShader.SetFloat("far_plane", far_plane);
			Matrix4 modelMatrix = Matrix4.CreateScale(0.1f);
			depthShader.SetMatrix4("modelMatrix", modelMatrix);
			//GL.Enable(EnableCap.PolygonOffsetFill);
			GL.Enable(EnableCap.PolygonOffsetFill);
			GL.PolygonOffset(1.1f, 1.5f);
			model.Draw(depthShader);
			GL.Disable(EnableCap.PolygonOffsetFill);
			//GL.Disable(EnableCap.PolygonOffsetFill);



			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			GL.Viewport(0, 0, Width, Height);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			shader.Use();
			GL.ActiveTexture(TextureUnit.Texture7);
			GL.BindTexture(TextureTarget.TextureCubeMap, depthCubemap);
			shader.SetInt("depthMap", 7);
			Matrix4 view = cam.GetViewMatrix();
			Matrix4 proj = cam.GetProjectionMatrix();
			shader.SetMatrix4("cubeProjMatrix", shadowProj);
			shader.SetMatrix4("viewMatrix", view);
			shader.SetMatrix4("projMatrix", proj);
			shader.SetMatrix4("modelMatrix", modelMatrix);

			shader.SetVec3("cameraPos", cam.Position);
			
			pointLight.Set(shader, 0);

			model.Draw(shader);
			CheckLastError();

			//GL.DepthFunc(DepthFunction.Lequal);
			//skybox.Draw(view, proj);
			//GL.DepthFunc(DepthFunction.Less);
			Context.SwapBuffers();
			CheckLastError();

			base.OnRenderFrame(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			HandleKeyboard(e);
			base.OnUpdateFrame(e);
		}

		private void HandleKeyboard(FrameEventArgs e)
		{
			if (Focused)
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

			if (Focused)
			{
				MouseState mouse = Mouse.GetState();

				if (firstMove) // this bool variable is initially set to true
				{
					lastPos = new Vector2(mouse.X, mouse.Y);
					firstMove = false;
				}
				else
				{
					//Calculate the offset of the mouse position
					float deltaX = mouse.X - lastPos.X;
					float deltaY = mouse.Y - lastPos.Y;
					lastPos = new Vector2(mouse.X, mouse.Y);

					//Apply the camera pitch and yaw (we clamp the pitch in the camera class)
					cam.Yaw += deltaX * cam.Sensitivity;
					cam.Pitch -= deltaY * cam.Sensitivity; // reversed since y-coordinates range from bottom to top
				}
			}
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			if(Focused)
				Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);

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
			depthShader.Dispose();
			//skybox.shader.Dispose();
			base.OnUnload(e);
		}

	}
}
