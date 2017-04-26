using System.ServiceModel;
using System.Windows.Forms;

namespace Ruler
{
	[ServiceContract(SessionMode = SessionMode.Required)]
	interface ISingleInstanceService
	{
		[OperationContract(IsOneWay = true)]
		void StartNewRuler(string args);
	}

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	class SingleInstanceService : ISingleInstanceService
	{
		public void StartNewRuler(string args)
		{
			lock (RulerApplicationContext.CurrentContext)
			{
				// Get the main form so we can start the new ruler on the main (UI) thread
				if (RulerApplicationContext.CurrentContext.MainForm != null)
				{
					RulerApplicationContext.CurrentContext.MainForm.Invoke((MethodInvoker)delegate
					{
						var newRuler = new RulerForm();
						newRuler.Show();
					});
				}
			}
		}
	}
}
