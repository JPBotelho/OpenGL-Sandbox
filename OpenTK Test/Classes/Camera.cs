using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_Test
{
	
	class Camera
	{
		public float fovy = 1.5f;
		public const float zNear = .1f;
		public const float zFar = 40f;

		public Vector3 Position = new Vector3(0, 0, 30);
		public Vector3 Orientation = new Vector3((float)Math.PI, 0f, 0f);
		public float MoveSpeed = 0.2f;
		public float MouseSensitivity = 0.0025f;

		private GameWindow window;

		public Matrix4 ProjectionMatrix
		{
			get
			{
				return GetProjectionMatrix();
			}
		}

		public Matrix4 ViewMatrix
		{
			get
			{
				return GetViewMatrix();
			}
		}


		public Camera(GameWindow window)
		{
			this.window = window;
			fovy = DegreeToRadian(90);
		}

		public Matrix4 GetViewMatrix()
		{
			Vector3 lookat = new Vector3();

			lookat.X = (float)(Math.Sin(Orientation.X) * Math.Cos(Orientation.Y));
			lookat.Y = (float)Math.Sin(Orientation.Y);
			lookat.Z = (float)(Math.Cos(Orientation.X) * Math.Cos(Orientation.Y));

			return Matrix4.LookAt(Position, Position + lookat, Vector3.UnitY);
		}

		private Matrix4 GetProjectionMatrix()
		{
			return Matrix4.CreatePerspectiveFieldOfView(fovy, (float)window.ClientSize.Width / window.ClientSize.Height, zNear, zFar);
		}

		public void Move(float x, float y, float z)
		{
			Vector3 offset = new Vector3();

			Vector3 forward = new Vector3((float)Math.Sin(Orientation.X), 0, (float)Math.Cos(Orientation.X));
			Vector3 right = new Vector3(-forward.Z, 0, forward.X);

			offset += x * right;
			offset += y * forward;
			offset.Y += z;

			offset.NormalizeFast();
			offset = Vector3.Multiply(offset, MoveSpeed);

			Position += offset;
		}

		public void AddRotation(float x, float y)
		{
			x = x * MouseSensitivity;
			y = y * MouseSensitivity;

			Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
			Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
		}

		private float DegreeToRadian(float angle)
		{
			return (float)(Math.PI * angle / 180.0);
		}
	}
}
