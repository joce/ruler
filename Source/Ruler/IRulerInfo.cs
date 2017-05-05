using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Ruler
{
	public interface IRulerInfo
	{
		int Length
		{
			get;
			set;
		}

		int Thickness
		{
			get;
			set;
		}

		bool IsVertical
		{
			get;
			set;
		}

		double Opacity
		{
			get;
			set;
		}

		bool ShowToolTip
		{
			get;
			set;
		}

		bool IsLocked
		{
			get;
			set;
		}

		bool TopMost
		{
			get;
			set;
		}

		bool ShowUpTicks
		{
			get;
			set;
		}

		bool ShowDownTicks
		{
			get;
			set;
		}

		Color BackColor
		{
			get;
			set;
		}
	}
}
