using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace OpenTK_Test.Classes
{
	public struct DirectionalLight
	{
		Vector3 direction;
		Vector3 ambient;
		Vector3 diffuse;
		Vector3 specular;

		public void Set(Shader shader, int index)
		{
			string lightRef = string.Format("directionalLights[{0}].", index);
			shader.SetVec3(lightRef + "direction", direction);
			shader.SetVec3(lightRef + "ambient", ambient);
			shader.SetVec3(lightRef + "diffuse", diffuse);
			shader.SetVec3(lightRef + "specular", specular);
		}
	}
}
