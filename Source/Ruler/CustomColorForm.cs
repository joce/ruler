using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Ruler
{
	public partial class CustomColorForm : Form
	{
		private String originalHexCode;

		public CustomColorForm(String initHexCode)
		{
			this.InitializeComponent();

			this.originalHexCode = initHexCode;

			this.hexCode.Text = initHexCode;
		}

		private void BtnCancelClick(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close();
		}

		private void BtnOkClick(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		public Color GetNewColor()
		{

			Color color = new Color();

			//Check if color is correct
			try
			{
				color = ColorTranslator.FromHtml(this.hexCode.Text);
			}
			catch (Exception)
			{
				color = ColorTranslator.FromHtml(this.originalHexCode);
			}

			//Check if color is transparent
			if(color.A == 0)
			{
				color = ColorTranslator.FromHtml(this.originalHexCode);
			}

			return color;
		}
	}
}
