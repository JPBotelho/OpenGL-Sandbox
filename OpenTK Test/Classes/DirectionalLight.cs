using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace OpenTK_Test
{
	public class DirectionalLight
	{
		public Vector3 direction = new Vector3(0.2f, 1f, .3f);
		public Vector3 ambient = new Vector3(0.1f, 0.1f, 0.1f);
		public Vector3 diffuse = new Vector3(0.8f, 0.8f, 0.8f);
		public Vector3 specular = new Vector3(1.0f, 1.0f, 1.0f);

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
