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

		public SetSizeForm(int initLength)
		{
			this.InitializeComponent();

			this.originalLength = initLength;

			this.txtWidth.Text = initLength.ToString();
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

		public int GetNewSize()
		{
			int length;

			length = int.TryParse(this.txtWidth.Text, out length) ? length : originalLength;

			return length;
		}
	}
}
