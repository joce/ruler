using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Ruler
{
	public partial class SetSizeForm : Form
	{
		private int originalLength;
		private int originalThickness;

		public SetSizeForm(int initLength, int initThickness)
		{
			this.InitializeComponent();

			this.originalLength = initLength;
			this.originalThickness = initThickness;

			this.txtLength.Text = initLength.ToString();
			this.txtThickness.Text = initThickness.ToString();
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

		public Dimension GetNewDimension()
		{
			int length;
			int thickness;

			Dimension dimension = new Dimension();

			dimension.Length = int.TryParse(this.txtLength.Text, out length) ? length : originalLength;
			dimension.Thickness = int.TryParse(this.txtThickness.Text, out thickness) ? thickness : originalThickness;

			return dimension;
		}
	}
}
