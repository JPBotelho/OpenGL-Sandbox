using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
namespace OpenTK_Test.Classes
{
	public struct PointLight
	{
		Vector3 position;

		float constant;
		float linear;
		float quadratic;

		Vector3 ambient;
		Vector3 diffuse;
		Vector3 specular;

		public void Set(Shader shader, int index)
		{
			string lightRef = string.Format("directionalLights[{0}].", index);
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
