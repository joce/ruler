using System;
using System.Threading;
using System.Windows.Forms;

namespace Ruler
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Mutex mutex = new Mutex(true, "{bafc08a9-6060-4811-b3c7-76be74bd4f25}");
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				try
				{
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					Application.Run(new RulerApplicationContext());
				}
				finally
				{
					mutex.Close();
				}
			}
			else
			{
				// Need to notify the other app
				MessageBox.Show("JOCE");
			}
		}
	}
}
