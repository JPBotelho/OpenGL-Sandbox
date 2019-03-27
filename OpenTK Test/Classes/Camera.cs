using System;
using OpenTK;

namespace OpenTK_Test
{
	public class Camera
	{
		public Vector3 Position;
		private Vector3 front = -Vector3.UnitZ;
		public Vector3 Front => front;
		private Vector3 up = Vector3.UnitY;
		public Vector3 Up => up;
		private Vector3 right = Vector3.UnitX;
		public Vector3 Right => right;

		private float _pitch;
		public float Pitch
		{
			get => _pitch;
			set
			{
				if (value > 89.0f)
				{
					_pitch = 89.0f;
				}
				else if (value <= -89.0f)
				{
					_pitch = -89.0f;
				}
				else
				{
					_pitch = value;
				}
				UpdateVertices();
			}
		}
		private float yaw;
		public float Yaw
		{
			get => yaw;
			set
			{
				yaw = value;
				UpdateVertices();
			}
		}

		public float Speed = 15;
		public float Sensitivity = 1f;

		private float fov = 90;
		public float Fov
		{
			get => fov;
			set
			{
				if (value >= 45)
				{
					fov = 45;
				}
				else if (value <= 1.0f)
				{
					fov = 1.0f;
				}
				else
				{
					fov = value;
				}
			}
		}
		public float AspectRatio { get; set; }

		public Camera(Vector3 position)
		{
			Position = position;
		}

		public Matrix4 GetViewMatrix() =>
			Matrix4.LookAt(Position, Position + Front, Up);
		public Matrix4 GetProjectionMatrix() =>
			Matrix4.CreatePerspectiveFieldOfView((float)rad(65), 1600.0f / 1200.0f, 0.01f, 300);

		private void UpdateVertices()
		{
			front.X = (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(Yaw));
			front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
			front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(Yaw));
			front = Vector3.Normalize(front);

			right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
			up = Vector3.Normalize(Vector3.Cross(Right, Front));
		}
		private double rad(double angle)
		{
			return Math.PI * angle / 180.0;
		}
	}
}