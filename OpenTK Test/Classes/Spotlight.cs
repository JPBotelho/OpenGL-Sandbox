using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_Test
{
	public class Spotlight
	{
		public Vector3 position;
		public Vector3 direction;
		public float cutoff = (float)Math.Cos(MathHelper.DegreesToRadians(25f));
		public float outercutoff = (float)Math.Cos(MathHelper.DegreesToRadians(30.0f));

		public float constant = 1;
		public float linear = 0.007f;
		public float quadratic = 0.0002f;

		public Vector3 ambient = new Vector3(0.1f, 0.1f, 0.1f);
		public Vector3 diffuse = new Vector3(0.8f, 0.8f, 0.8f);
		public Vector3 specular = new Vector3(1.0f, 1.0f, 1.0f);

		public void Set(Shader shader, int index)
		{
			shader.Use();
			string lightRef = string.Format("spotlights[{0}].", index);
			shader.SetVec3(lightRef + "position", position);
			shader.SetVec3(lightRef + "direction", direction);
			shader.SetFloat(lightRef + "cutoff", cutoff);
			shader.SetFloat(lightRef + "outercutoff", outercutoff);

			shader.SetFloat(lightRef + "constant", constant);
			shader.SetFloat(lightRef + "linear", linear);
			shader.SetFloat(lightRef + "quadratic", quadratic);

			shader.SetVec3(lightRef + "ambient", ambient);
			shader.SetVec3(lightRef + "diffuse", diffuse);
			shader.SetVec3(lightRef + "specular", specular);
		}
	}
}
