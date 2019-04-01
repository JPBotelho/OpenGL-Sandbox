using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
namespace OpenTK_Test
{
	static class Program
	{
		static void Main()
		{
			new MainWindow(800, 600, new GraphicsMode(new ColorFormat(8, 8, 8, 0), 24, 8, 4), "Test OpenTK").Run(120.0);
		}
	}
}
