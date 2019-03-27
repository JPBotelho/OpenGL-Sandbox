using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTK_Test
{
	static class Program
	{
		static void Main()
		{
			new MainWindow(800, 600, "Test OpenTK").Run(60.0);
		}
	}
}
