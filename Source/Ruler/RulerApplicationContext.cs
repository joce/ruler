using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Ruler
{
	class RulerApplicationContext : ApplicationContext
	{
		private static RulerApplicationContext _currContext;

		readonly HashSet<MainForm> _openRulers = new HashSet<MainForm>();

		public static RulerApplicationContext CurrentContext
		{
			get { return _currContext; }
		}

		public RulerApplicationContext()
		{
			_currContext = this;
			var ruler = new MainForm();
			ruler.Show();
		}

		// Disabled;
		private RulerApplicationContext(Form AppMainForm)
		{ }

		public void RegisterRuler(MainForm newRuler)
		{
			_openRulers.Add(newRuler);
		}

		public void UnregisterRuler(MainForm ruler)
		{
			_openRulers.Remove(ruler);
			if (!_openRulers.Any())
			{
				Application.Exit();
			}
		}
	}
}
