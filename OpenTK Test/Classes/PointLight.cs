using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
namespace OpenTK_Test
{
	public class PointLight
	{
		public Vector3 position;

		public float constant = 1;
		public float linear = 0.09f;
		public float quadratic = 0.0064f;

		public Vector3 ambient = new Vector3(1, 1.0f, 1f);
		public Vector3 diffuse = new Vector3(1f, 1.0f, 1f);
		public Vector3 specular = new Vector3(1f, 1.0f, 1f);

		public void Set(Shader shader, int index)
		{
			string lightRef = string.Format("pointLights[{0}].", index);
			shader.SetVec3(lightRef + "position", position);

			shader.SetFloat(lightRef + "constant", constant);
			shader.SetFloat(lightRef + "linear", linear);
			shader.SetFloat(lightRef + "quadratic", quadratic);

			shader.SetVec3(lightRef + "ambient", ambient);
			shader.SetVec3(lightRef + "diffuse", diffuse);
			shader.SetVec3(lightRef + "specular", specular);
		}
	}
}
