using System;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Ruler
{
	static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			RulerInfo info;
			if (args.Length > 0)
			{
				if (args.Contains("--help"))
				{
					MessageBox.Show(GetHelpText(), "Ruler", MessageBoxButtons.OK, MessageBoxIcon.None);
					return;
				}

				string error;
				if (!ParseArguments(args, out info, out error))
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("Error parsing arguments:");
					sb.AppendLine("    " + string.Join(" ", args));
					sb.AppendLine();
					sb.AppendLine(error);
					MessageBox.Show(sb.ToString(), "Ruler", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
			else
			{
				info = new RulerInfo();
			}

			string guid = "bafc08a9-6060-4811-b3c7-76be74bd4f25";
			Mutex mutex = new Mutex(true, guid);
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				try
				{
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					Application.Run(new RulerApplicationContext(info, guid));
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
				service.StartNewRuler(info);
			}
		}

		public static string GetHelpText()
		{
			RulerInfo defaultValues = RulerInfo.GetDefaultRulerInfo();
			string defaultLength = defaultValues.Length.ToString();
			string defaultThickness = defaultValues.Thickness.ToString();

			string defaultColor = RulerInfo.GetNameFromColor(defaultValues.BackColor);
			string defaultOpacity = defaultValues.Opacity.ToString();

			Func<bool, string> flagSign = b => b ? "+" : "-";
			string defaultDownTicks = flagSign(defaultValues.ShowDownTicks) + "d";
			string defaultLock = flagSign(defaultValues.IsLocked) + "l";
			string defaultTopMost = flagSign(defaultValues.TopMost) + "m";
			string defaultToolTop = flagSign(defaultValues.ShowToolTip) + "t";
			string defaultUpTicks = flagSign(defaultValues.ShowUpTicks) + "u";
			string defaultVertical = flagSign(defaultValues.IsVertical) + "v";

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Usage: ruler");
			sb.AppendLine("    or:    ruler [OPTIONS | FLAGS] LENGTH [THICKNESS]");
			sb.AppendLine();
			sb.AppendLine("Display a ruler on screen");
			sb.AppendLine();

			sb.AppendLine("LENGTH\t\tThe length of the ruler, in pixels (default: " + defaultLength + ")");
			sb.AppendLine("THICKNESS\tThe thickness of the ruler, in pixels (default: " + defaultThickness + ")");
			sb.AppendLine();
			sb.AppendLine("OPTIONS");
			sb.AppendLine("========");

			// Colors fanciness
			sb.AppendLine("--color=COLOR\tThe color of the ruler. One of:");
			sb.Append("\t\t     ");
			string[] colorNames = RulerInfo.Colors.Keys.OrderBy(c => c).ToArray();
			sb.Append(String.Join(", ", colorNames.Take(colorNames.Length-1)));
			sb.Append(" or ");
			sb.Append(colorNames[colorNames.Length - 1]);
			sb.AppendLine(" (default: " + defaultColor + ")");

			sb.AppendLine("--opacity=OPACITY\tThe opacity of the ruler in a [0.1, 1.0] range (default: " + defaultOpacity + ")");
			sb.AppendLine("--usedefaults\tUse default values for all unspecified values");

			sb.AppendLine();
			sb.AppendLine("FLAGS");
			sb.AppendLine("========");
			sb.AppendLine("[+|-]d\t\tShow / hide down ticks (default: " + defaultDownTicks + ")");
			sb.AppendLine("[+|-]l\t\tLock / unlock the ruler (default: " + defaultLock + ")");
			sb.AppendLine("[+|-]m\t\tMake / do not make top most (default: " + defaultTopMost + ")");
			sb.AppendLine("[+|-]t\t\tShow / hide the tooltip (default: " + defaultToolTop + ")");
			sb.AppendLine("[+|-]u\t\tShow / hide up ticks (default: " + defaultUpTicks + ")");
			sb.AppendLine("[+|-]v\t\tShow the ruler vertically / horizontally (default: " + defaultVertical + ")");
			return sb.ToString();
		}

		public static bool ParseArguments(string[] args, out RulerInfo info, out string error)
		{
			info = null;
			error = string.Empty;
			RulerInfo ret = args.Contains("--defaultvalues") ? RulerInfo.GetDefaultRulerInfo() : new RulerInfo();

			int numCnt = 0;
			int[] nums = new int[3];
			// Parse numerical arguments that should be at the end, if any.
			for (int i = args.Length - 1; i >= 0; i--)
			{
				if (int.TryParse(args[i], out nums[numCnt]))
				{
					numCnt++;
					if (numCnt > 2)
					{
						numCnt--;
						error = "Too many numerical arguments";
					}
				}
			}

			if (error != string.Empty)
			{
				return false;
			}

			if (numCnt == 1)
			{
				ret.Length = nums[0];
			}
			else if (numCnt == 2)
			{
				ret.Length = nums[1];
				ret.Thickness = nums[0];
			}

			for (int i = 0; i < args.Length - numCnt; i++)
			{
				if (args[i].StartsWith("--"))
				{
					if (args[i].StartsWith("--color="))
					{
						string colorName = args[i].Substring(8);
						if (!RulerInfo.Colors.Keys.Contains(colorName))
						{
							error = "Unknown color: " + colorName;
							return false;
						}
						ret.BackColor = RulerInfo.Colors[colorName];
						continue;
					}

					if (args[i].StartsWith("--opacity="))
					{
						string opacityString = args[i].Substring(10);
						double opacity;
						if (!double.TryParse(opacityString, out opacity))
						{
							error = "Don't know how to parse opacity: " + opacityString;
							return false;
						}
						// need to clamp in [0.1 - 1.0] range and in 0.1 increments.
						ret.Opacity = opacity;
						continue;
					}

					if (args[i].Equals("--defaultvalues"))
					{
						continue;
					}

					error = "Unknown option: " + args[i];
					return false;
				}

				if (args[i].StartsWith("-") || args[i].StartsWith("+"))
				{
					if (args[i].Length != 2)
					{
						error = "Unknown flag: " + args[i];
						return false;
					}

					bool enable = args[i][0] == '+';
					char rest = args[i][1];
					switch (rest)
					{
						case 'd':
							ret.ShowDownTicks = enable;
							break;
						case 'l':
							ret.IsLocked = enable;
							break;
						case 'm':
							ret.TopMost = enable;
							break;
						case 't':
							ret.ShowToolTip = enable;
							break;
						case 'u':
							ret.ShowUpTicks = enable;
							break;
						case 'v':
							ret.IsVertical = enable;
							break;
						default:
							error = "Unknown flag: " + args[i];
							return false;
					}
					continue;
				}

				error = "Unknown option: " + args[i];
				return false;
			}

			info = ret;

			return true;
		}
	}
}
