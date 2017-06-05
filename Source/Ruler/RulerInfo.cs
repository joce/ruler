using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Ruler
{
	public class RulerInfo : IRulerInfo
	{
		private static readonly Dictionary<string, Color> s_ColorDict = new Dictionary<string, Color>
		{
			{"white", Color.White},
			{"yellow", Color.LightYellow},
			{"blue", Color.LightBlue},
			{"red", Color.LightSalmon},
			{"green", Color.LightGreen}
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

		public static RulerInfo GetDefaultRulerInfo()
		{
			RulerInfo ret = new RulerInfo();

			Func<string, bool> isFlag =
				flagName =>
					(Properties.Settings.Default.Properties[flagName]?.DefaultValue.ToString() == "True");
			ret.IsVertical = isFlag("IsVertical");
			ret.IsLocked = isFlag("IsLocked");
			ret.ShowToolTip = isFlag("ShowToolTip");
			ret.ShowUpTicks = isFlag("ShowUpTicks");
			ret.ShowDownTicks = isFlag("ShowDownTicks");
			ret.TopMost = isFlag("TopMost");
			ret.IsFlipped = isFlag("IsFlipped");

			ret.Length = int.Parse(Properties.Settings.Default.Properties["Length"]?.DefaultValue.ToString() ?? string.Empty);
			ret.Thickness = int.Parse(Properties.Settings.Default.Properties["Thickness"]?.DefaultValue.ToString() ?? string.Empty);
			ret.Opacity = double.Parse(Properties.Settings.Default.Properties["Opacity"]?.DefaultValue.ToString() ?? string.Empty);

			string defaultColor = Properties.Settings.Default.Properties["BackColor"]?.DefaultValue.ToString() ?? string.Empty;
			ret.BackColor = Colors.ContainsKey(defaultColor) ? Colors[defaultColor] : Color.White;

			return ret;
		}

		public int Length { get; set; }
		public int Thickness { get; set; }
		public bool IsVertical { get; set; }
		public double Opacity { get; set; }
		public bool ShowToolTip { get; set; }
		public bool IsLocked { get; set; }
		public bool TopMost { get; set; }
		public Color BackColor { get; set; }
		public bool ShowUpTicks { get; set; }
		public bool ShowDownTicks { get; set; }
		public bool IsFlipped { get; set; }

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
			IsFlipped = Properties.Settings.Default.IsFlipped;
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
			targetInstance.IsFlipped = ruler.IsFlipped;
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
			Properties.Settings.Default.IsFlipped = ruler.IsFlipped;

			Properties.Settings.Default.Save();
		}
	}
}
