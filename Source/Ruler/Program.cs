using System;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;

namespace Ruler
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			string guid = "bafc08a9-6060-4811-b3c7-76be74bd4f25";
			Mutex mutex = new Mutex(true, guid);
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				try
				{
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					Application.Run(new RulerApplicationContext(guid));
				}
				finally
				{
					mutex.Close();
				}
			}
			else
			{
				var pipeFactory =
					new ChannelFactory<ISingleInstanceService>(
						new NetNamedPipeBinding(),
						new EndpointAddress("net.pipe://localhost/" + guid));

				ISingleInstanceService service = pipeFactory.CreateChannel();
				service.StartNewRuler("");
			}
		}
	}
}
