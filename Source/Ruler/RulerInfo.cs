using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Ruler
{
	public class RulerInfo : IRulerInfo
	{
		private static readonly Dictionary<string, Color> s_ColorDict = new Dictionary<string, Color>
		{
			{"White", Color.White},
			{"Yellow", Color.LightYellow},
			{"Blue", Color.LightBlue},
			{"Red", Color.LightSalmon},
			{"Green", Color.LightGreen}
		};

		public static IDictionary<string, Color> Colors
		{
			get { return s_ColorDict; }
		}

		public static string GetNameFromColor(Color color)
		{
			var ret = s_ColorDict.FirstOrDefault(kvp => kvp.Value == color);
			if (string.IsNullOrEmpty(ret.Key))
			{
				return "White";
			}
			return ret.Key;
		}

		public static Color GetColorFromName(string name)
		{
			var ret = s_ColorDict.FirstOrDefault(kvp => kvp.Key == name);
			if (string.IsNullOrEmpty(ret.Key))
			{
				return Color.White;
			}
			return ret.Value;
		}

		public int Length
		{
			get;
			set;
		}

		public int Thickness
		{
			get;
			set;
		}

		public bool IsVertical
		{
			get;
			set;
		}

		public double Opacity
		{
			get;
			set;
		}

		public bool ShowToolTip
		{
			get;
			set;
		}

		public bool IsLocked
		{
			get;
			set;
		}

		public bool TopMost
		{
			get;
			set;
		}

		public Color BackColor
		{
			get;
			set;
		}

		public bool ShowUpTicks
		{
			get;
			set;
		}

		public bool ShowDownTicks
		{
			get;
			set;
		}

		public RulerInfo()
		{
			// IsVertical needs to be set first to ensure the min size are properly set
			// before Length and Thickness are set.
			IsVertical = Properties.Settings.Default.IsVertical;
			Length = Properties.Settings.Default.Length;
			Thickness = Properties.Settings.Default.Thickness;
			Opacity = Properties.Settings.Default.Opacity;
			ShowToolTip = Properties.Settings.Default.ShowToolTip;
			IsLocked = Properties.Settings.Default.IsLocked;
			TopMost = Properties.Settings.Default.TopMost;
			BackColor = GetColorFromName(Properties.Settings.Default.BackColor);
			ShowUpTicks = Properties.Settings.Default.ShowUpTicks;
			ShowDownTicks = Properties.Settings.Default.ShowDownTicks;
		}

		public string ConvertToParameters()
		{
			return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", this.Length, this.Thickness, this.IsVertical, this.Opacity, this.ShowToolTip, this.IsLocked, this.TopMost, this.BackColor.Name, this.ShowUpTicks, this.ShowDownTicks);
		}

		public static RulerInfo CovertToRulerInfo(string[] args)
		{
			if (args.Length != 10)
			{
				// We need better handling of start arguments
				return new RulerInfo();
			}

			string width = args[0];
			string height = args[1];
			string isVertical = args[2];
			string opacity = args[3];
			string showToolTip = args[4];
			string isLocked = args[5];
			string topMost = args[6];
			string backColor = args[7];
			string showUpTicks = args[8];
			string showDownTicks = args[9];

			RulerInfo rulerInfo = new RulerInfo();

			// IsVertical needs to be set first to ensure the min size are properly set
			// before Length and Thickness are set.
			rulerInfo.IsVertical = bool.Parse(isVertical);
			rulerInfo.Length = int.Parse(width);
			rulerInfo.Thickness = int.Parse(height);
			rulerInfo.Opacity = double.Parse(opacity);
			rulerInfo.ShowToolTip = bool.Parse(showToolTip);
			rulerInfo.IsLocked = bool.Parse(isLocked);
			rulerInfo.TopMost = bool.Parse(topMost);
			rulerInfo.BackColor = GetColorFromName(backColor);
			rulerInfo.ShowUpTicks = bool.Parse(showUpTicks);
			rulerInfo.ShowDownTicks = bool.Parse(showDownTicks);

			return rulerInfo;
		}
	}

	public static class IRulerInfoExtentension
	{
		public static void CopyInto(this IRulerInfo ruler, IRulerInfo targetInstance)
		{
			// IsVertical needs to be set first to ensure the min size are properly set
			// before Length and Thickness are set.
			targetInstance.IsVertical = ruler.IsVertical;
			targetInstance.Length = ruler.Length;
			targetInstance.Thickness = ruler.Thickness;
			targetInstance.Opacity = ruler.Opacity;
			targetInstance.ShowToolTip = ruler.ShowToolTip;
			targetInstance.IsLocked = ruler.IsLocked;
			targetInstance.TopMost = ruler.TopMost;
			targetInstance.BackColor = ruler.BackColor;
			targetInstance.ShowUpTicks = ruler.ShowUpTicks;
			targetInstance.ShowDownTicks = ruler.ShowDownTicks;
		}

		public static void SaveInfo(this IRulerInfo ruler)
		{
			Properties.Settings.Default.IsVertical = ruler.IsVertical;
			Properties.Settings.Default.Length = ruler.Length;
			Properties.Settings.Default.Thickness = ruler.Thickness;
			Properties.Settings.Default.Opacity = ruler.Opacity;
			Properties.Settings.Default.ShowToolTip = ruler.ShowToolTip;
			Properties.Settings.Default.IsLocked = ruler.IsLocked;
			Properties.Settings.Default.TopMost = ruler.TopMost;
			Properties.Settings.Default.BackColor = RulerInfo.GetNameFromColor(ruler.BackColor);
			Properties.Settings.Default.ShowUpTicks = ruler.ShowUpTicks;
			Properties.Settings.Default.ShowDownTicks = ruler.ShowDownTicks;

			Properties.Settings.Default.Save();
		}
	}
}
