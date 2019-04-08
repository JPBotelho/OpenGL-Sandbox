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
		PointLight[] pointLights = new PointLight[]
		{
			new PointLight()
			{
				position = new Vector3(0.0f, 4.0f, 0.0f)/*,
				linear = 0.027f,
				quadratic = 0.0028f	*/			
			}/*,
			new PointLight()
			{
				position = new Vector3(111, 20, -41),
				/*ambient = new Vector3(0, 229f/255, 1),
				diffuse = new Vector3(0, 229f/255, 1),
				specular = new Vector3(0, 229f/255, 1)
			},
			new PointLight()
			{
				position = new Vector3(111, 20, 41),
				/*ambient = new Vector3(246f/255, 0, 1),
				diffuse = new Vector3(246f/255, 0, 1),
				specular = new Vector3(246f/255, 0, 1)
			},
			new PointLight()
			{
				position = new Vector3(-111, 20, 41),
				/*ambient = new Vector3(0, 1, 110f/255),
				diffuse = new Vector3(0, 1, 1f/255),
				specular = new Vector3(0, 1, 1f/255)
			},
			new PointLight()
			{
				position = new Vector3(-111, 20, -41),
				/*ambient = new Vector3(1, 178f/255, 0),
				diffuse = new Vector3(1, 178f/255, 0),
				specular = new Vector3(1, 178f/255, 0)
			}*/
		};
		string[] skyboxFaces =
		{
			"resources/skybox/right.jpg",
			"resources/skybox/left.jpg",
			"resources/skybox/top.jpg",
			"resources/skybox/bottom.jpg",
			"resources/skybox/front.jpg",
			"resources/skybox/back.jpg"
		};

		private DateTime startTime;

		int shadowWidth = 128, shadowHeight = 128;
		public MainWindow(int width, int height, GraphicsMode mode, string title) : base(width, height, mode, title) { }

		protected override void OnLoad(EventArgs e)
		{
			startTime = DateTime.Now;
			//VSync = VSyncMode.Off;
			//skybox = new Skybox(skyboxFaces);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			CheckLastError();
			CursorVisible = false;

			shader = new Shader("shader.vert", "shader.frag");
			depthShader = new Shader("depth.vs", "depth.fs");
			model = new Model("C:/Users/User/source/repos/OpenTK Test/OpenTK Test/bin/Debug/resources/sponza/sponza.obj");
			SetupShadowmaps(pointLights);

			cam = new Camera(Vector3.UnitZ * 3);
			cam.AspectRatio = (float)Width / Height;
			base.OnLoad(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";

			GL.Viewport(0, 0, Width, Height);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			SetShadowMaps(pointLights, depthShader, shader);

			/*shader.Use();			
			Matrix4 view = cam.GetViewMatrix();
			Matrix4 proj = cam.GetProjectionMatrix();
			Matrix4 modelMatrix = Matrix4.CreateScale(0.1f);

			shader.SetMatrix4("viewMatrix", view);
			shader.SetMatrix4("projMatrix", proj);
			shader.SetMatrix4("modelMatrix", modelMatrix);
			shader.SetVec3("cameraPos", cam.Position);
			
			for(int i = 0; i < pointLights.Length; i++)
			{
				pointLights[i].Set(shader, i);
			}
			//pointLight.Set(shader, 0);

			model.Draw(shader);*/
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
			var input = Keyboard.GetState();

			if (Focused)
			{
				if (input.IsKeyDown(Key.ControlLeft))
					CursorVisible = !CursorVisible;
			}
			if (Focused && !CursorVisible)
			{
				float deltatime = (float)(1f / e.Time);

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

			if (Focused && !CursorVisible)
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
			if(Focused && !CursorVisible)
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

		protected override void OnResize(EventArgs e)
		{
			cam.AspectRatio = (float)Width / Height;
			GL.Viewport(0, 0, Width, Height);
			base.OnResize(e);
		}

		protected override void OnUnload(EventArgs e)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			shader.Dispose();
			depthShader.Dispose();
			//skybox.shader.Dispose();
			base.OnUnload(e);
		}

		private int[] shadowAtlases;
		private int[] shadowFBOs;
		private void SetupShadowmaps(PointLight[] lights)
		{
			shadowAtlases = new int[lights.Length];
			shadowFBOs = new int[lights.Length];

			for(int i = 0; i < lights.Length; i++)
			{
				shadowAtlases[i] = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, shadowAtlases[i]);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, shadowWidth * 3, shadowHeight * 2, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

				shadowFBOs[i] = GL.GenFramebuffer();
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBOs[i]);
				GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, shadowAtlases[i], 0);
				GL.DrawBuffer(DrawBufferMode.None);
				GL.ReadBuffer(ReadBufferMode.None);
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
				CheckLastError();
			}			
		}

		private void SetShadowMaps(PointLight[] lights, Shader depthShader, Shader targetShader)
		{
			for (int i = 0; i < lights.Length; i++)
			{
				Vector3 lightPos = pointLights[i].position;
				Matrix4 shadowProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), (float)shadowWidth / shadowHeight, 0.1f, 300f);
				Matrix4[] shadowTransforms = new Matrix4[]
				{
					Matrix4.LookAt(lightPos, lightPos + new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
					Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)) * shadowProj,
					Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
					Matrix4.LookAt(lightPos, lightPos + new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
					Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)) * shadowProj,
					Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj					
				};

				GL.ClearColor(Color.DeepPink);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				GL.Viewport(0, 0, shadowWidth, shadowHeight);
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBOs[i]);
				GL.Clear(ClearBufferMask.DepthBufferBit);

				float near_plane = 0.1f;
				float far_plane = 300f;
				Matrix4 modelMatrix = Matrix4.CreateScale(0.1f);


				for (int height = 0; height < 2; height++)
				{
					for(int width = 0; width < 3; width++)
					{
						GL.Viewport(width * shadowWidth, height * shadowHeight, shadowWidth, shadowHeight);
						// +X +Y +Z
						// -X -Y -Z
						depthShader.SetVec3("lightPos", lightPos);
						depthShader.SetFloat("far_plane", far_plane);
						depthShader.SetMatrix4("modelMatrix", modelMatrix);
						depthShader.SetMatrix4("viewProjMatrix", shadowTransforms[width + 3 * height]);


						GL.Enable(EnableCap.PolygonOffsetFill);
						GL.PolygonOffset(1.1f, 1.5f);
						model.Draw(depthShader);
						GL.Disable(EnableCap.PolygonOffsetFill);
					}
				}

				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

				targetShader.Use();
				GL.ActiveTexture(TextureUnit.Texture10+i);
				GL.BindTexture(TextureTarget.Texture2D, shadowAtlases[i]);
				targetShader.SetInt("shadowAtlases["+i+"]", 10+i);
				CheckLastError();
				GL.DeleteFramebuffer(shadowFBOs[i]);
				//GL.DeleteTexture(shadowCubemaps[i]);
			}
		}
	}
}
