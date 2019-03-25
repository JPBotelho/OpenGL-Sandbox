using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace OpenTK_Test
{
	class Transform
	{
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;

		public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
		{
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
		}

		public Matrix4 GetModelMatrix()
		{
			return Matrix4.Identity;
		}

		private Matrix4 GetTranslationMatrix()
		{
			return Matrix4.Identity;
		}

		private Matrix4 GetRotationMatrix()
		{
			return Matrix4.Identity;
		}

		private Matrix4 GetScaleMatrix()
		{
			return Matrix4.Identity;
		}
	}
}
