using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;

namespace Ruler
{
	class RulerApplicationContext : ApplicationContext
	{
		private static RulerApplicationContext _currContext;

		readonly HashSet<RulerForm> _openRulers = new HashSet<RulerForm>();

		public static RulerApplicationContext CurrentContext
		{
			get { return _currContext; }
		}

		public RulerApplicationContext(string singleInstanceServiceAddress)
		{
			_currContext = this;

			// Start the listener for the single instance service
			var host = new ServiceHost(typeof(SingleInstanceService),
									   new Uri("net.pipe://localhost"));
			host.AddServiceEndpoint(typeof(ISingleInstanceService),
									new NetNamedPipeBinding(), singleInstanceServiceAddress);
			host.Open();

			// And open the first ruler
			var ruler = new RulerForm();
			ruler.Show();
		}

		// Disabled;
		private RulerApplicationContext(Form AppMainForm)
		{ }

		public void RegisterRuler(RulerForm newRuler)
		{
			_openRulers.Add(newRuler);
			if (MainForm == null)
			{
				MainForm = newRuler;
			}
		}

		public void UnregisterRuler(RulerForm ruler)
		{
			_openRulers.Remove(ruler);

			if (!_openRulers.Any())
			{
				Application.Exit();
			}
			else if (MainForm == ruler)
			{
				MainForm = _openRulers.First();
			}
		}
	}
}
