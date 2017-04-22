using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Ruler
{
	public class RulerInfo : IRulerInfo
	{
		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// TODO
		/// </summary>
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

		/// <summary>
		/// TODO
		/// </summary>
		public bool ShowToolTip
		{
			get;
			set;
		}

		/// <summary>
		/// TODO
		/// </summary>
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

		public string ConvertToParameters()
		{
			return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", this.Width, this.Height, this.IsVertical, this.Opacity, this.ShowToolTip, this.IsLocked, this.TopMost, this.BackColor.Name);
		}

		public static RulerInfo CovertToRulerInfo(string[] args)
		{
			string width = args[0];
			string height = args[1];
			string isVertical = args[2];
			string opacity = args[3];
			string showToolTip = args[4];
			string isLocked = args[5];
			string topMost = args[6];
			string backColor = args[7];

			RulerInfo rulerInfo = new RulerInfo();

			rulerInfo.Width = int.Parse(width);
			rulerInfo.Height = int.Parse(height);
			rulerInfo.IsVertical = bool.Parse(isVertical);
			rulerInfo.Opacity = double.Parse(opacity);
			rulerInfo.ShowToolTip = bool.Parse(showToolTip);
			rulerInfo.IsLocked = bool.Parse(isLocked);
			rulerInfo.TopMost = bool.Parse(topMost);
			rulerInfo.BackColor = Color.FromName(backColor);

			return rulerInfo;
		}

		public static RulerInfo GetDefaultRulerInfo()
		{
			RulerInfo rulerInfo = new RulerInfo();

			rulerInfo.Width = 500;
			rulerInfo.Height = 126;
			rulerInfo.Opacity = 0.6;
			rulerInfo.ShowToolTip = false;
			rulerInfo.IsLocked = false;
			rulerInfo.IsVertical = false;
			rulerInfo.TopMost = false;
			rulerInfo.BackColor = Color.White;

			return rulerInfo;
		}
	}

	public static class IRulerInfoExtentension
	{
		public static void CopyInto<T>(this IRulerInfo ruler, T targetInstance)
			where T : IRulerInfo
		{
			targetInstance.Width = ruler.Width;
			targetInstance.Height = ruler.Height;
			targetInstance.IsVertical = ruler.IsVertical;
			targetInstance.Opacity = ruler.Opacity;
			targetInstance.ShowToolTip = ruler.ShowToolTip;
			targetInstance.IsLocked = ruler.IsLocked;
			targetInstance.TopMost = ruler.TopMost;
			targetInstance.BackColor = ruler.BackColor;
		}
	}
}
