using System;
using System.Collections.Generic;
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

		public string ConvertToParameters()
		{
			return string.Format("{0} {1} {2} {3} {4} {5}", this.IsVertical ? this.Height : this.Width, this.IsVertical, this.Opacity, this.ShowToolTip, this.IsLocked, this.TopMost);
		}

		public static RulerInfo CovertToRulerInfo(string[] args)
		{
			string height = args[0];
			string isVertical = args[1];
			string opacity = args[2];
			string showToolTip = args[3];
			string isLocked = args[4];
			string topMost = args[5];

			RulerInfo rulerInfo = new RulerInfo();

			rulerInfo.IsVertical = bool.Parse(isVertical);
			rulerInfo.Opacity = double.Parse(opacity);
			rulerInfo.ShowToolTip = bool.Parse(showToolTip);
			rulerInfo.IsLocked = bool.Parse(isLocked);
			rulerInfo.TopMost = bool.Parse(topMost);

			int length = int.Parse(height);
			if (rulerInfo.IsVertical)
			{
				rulerInfo.Height = length;
				rulerInfo.Width = 75;
			}
			else
			{
				rulerInfo.Height = 75;
				rulerInfo.Width = length;
			}

			return rulerInfo;
		}

		public static RulerInfo GetDefaultRulerInfo()
		{
			RulerInfo rulerInfo = new RulerInfo();

			rulerInfo.Width = 400;
			rulerInfo.Height = 75;
			rulerInfo.Opacity = 0.6;
			rulerInfo.ShowToolTip = false;
			rulerInfo.IsLocked = false;
			rulerInfo.IsVertical = false;
			rulerInfo.TopMost = false;

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
		}
	}
}
