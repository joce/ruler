using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Ruler
{
	sealed public class MainForm : Form, IRulerInfo
	{
		[Flags]
		private enum ResizeRegion
		{
			None,
			N = 1,
			E = 2,
			S = 4,
			W = 8,
			NE = N | E,
			NW = N | W,
			SE = S | E,
			SW = S | W
		}

		private enum DragMode
		{
			None, Move, Resize
		}

		private ToolTip _toolTip = new ToolTip();
		private ResourceManager _resources = new ResourceManager(typeof(MainForm));
		private Point _offset;
		private Rectangle _mouseDownRect;
		private int _resizeBorderWidth = 5;
		private Point _mouseDownPoint;
		private ResizeRegion _resizeRegion = ResizeRegion.None;
		private ContextMenu _menu = new ContextMenu();
		private MenuItem _lockMenuItem;
		private DragMode _dragMode = DragMode.None;
		private static Color _TickColor = ColorTranslator.FromHtml("#3E2815");
		private static Color _CursorColor = Color.FromArgb(200, _TickColor);
		private static Region _lockIconRegion;
		private static Region _rotateIconRegion;
		private static Rectangle _lockIconRegionR;
		private static Rectangle _rotateIconRegionR;

		private static readonly Dictionary<string, Color> s_ColorDict = new Dictionary<string, Color>()
		{
			{"White", Color.White},
			{"Yellow", Color.LightYellow},
			{"Blue", Color.LightBlue},
			{"Red", Color.LightSalmon},
			{"Green", Color.LightGreen}
		};

		public MainForm()
		{
			RulerInfo rulerInfo = RulerInfo.GetDefaultRulerInfo();

			this.Init(rulerInfo);
		}

		public MainForm(RulerInfo rulerInfo)
		{
			this.Init(rulerInfo);
		}

		public bool IsVertical
		{
			get;
			set;
		}

		public bool IsLocked
		{
			get;
			set;
		}

		public bool ShowToolTip
		{
			get;
			set;
		}

		private void Init(RulerInfo rulerInfo)
		{
			InitializeComponent();
			rulerInfo.CopyInto(this);

			this.SetStyle(ControlStyles.ResizeRedraw, true);
			this.UpdateStyles();

			this.Icon = GetIcon("$this.Icon");

			this.SetUpMenu();

			this.Text = "Ruler";

			this.FormBorderStyle = FormBorderStyle.None;

			this.ContextMenu = _menu;
			this.Font = new Font("Segoe UI", 9, FontStyle.Bold);

			this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}

		private RulerInfo GetRulerInfo()
		{
			RulerInfo rulerInfo = new RulerInfo();

			this.CopyInto(rulerInfo);

			return rulerInfo;
		}

		private void SetUpMenu()
		{
			this.AddMenuItem("Stay On Top", Shortcut.None, this.StayOnTopHandler, TopMost);
			this.AddMenuItem("Vertical", Shortcut.None, this.VerticalHandler, IsVertical);
			this.AddMenuItem("Tool Tip", Shortcut.None, this.ToolTipHandler, ShowToolTip);
			MenuItem opacityMenuItem = this.AddMenuItem("Opacity");
			MenuItem colorMenuItem = this.AddMenuItem("Color");
			_lockMenuItem = this.AddMenuItem("Lock resizing", Shortcut.None, this.LockHandler, IsLocked);
			this.AddMenuItem("Set size...", Shortcut.None, this.SetWidthHeightHandler);
			this.AddMenuItem("Duplicate", Shortcut.None, this.DuplicateHandler);
			this.AddMenuItem("-");
			this.AddMenuItem("About...");
			this.AddMenuItem("-");
			this.AddMenuItem("Exit");

			for (int i = 10; i <= 100; i += 10)
			{
				MenuItem subMenu = new MenuItem(i + "%");
				subMenu.Checked = (i == Opacity * 100);
				subMenu.Click += new EventHandler(OpacityMenuHandler);
				opacityMenuItem.MenuItems.Add(subMenu);
			}

			// Add colors to color menus
			foreach (var color in s_ColorDict)
			{
				MenuItem subMenu = new MenuItem(color.Key);
				if (color.Value == BackColor)
				{
					subMenu.Checked = true;
				}
				subMenu.Click += new EventHandler(ColorMenuHandler);
				colorMenuItem.MenuItems.Add(subMenu);
			}
		}

		private Icon GetIcon(string name)
		{
			return (Icon)(_resources.GetObject(name));
		}

		private void SetWidthHeightHandler(object sender, EventArgs e)
		{
			SetSizeForm form = new SetSizeForm(this.Width, this.Height);

			if (this.TopMost)
			{
				form.TopMost = true;
			}

			if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Size size = form.GetNewSize();

				this.Width = size.Width;
				this.Height = size.Height;
			}
		}

		private void ToolTipHandler(object sender, EventArgs e)
		{
			ShowToolTip = !ShowToolTip;
			((MenuItem)sender).Checked = this.ShowToolTip;
			this.SetToolTip();
		}

		private void VerticalHandler(object sender, EventArgs e)
		{
			ChangeOrientation();
			((MenuItem)sender).Checked = this.IsVertical;
		}

		private void LockHandler(object sender, EventArgs e)
		{
			this.IsLocked = !this.IsLocked;
			_lockMenuItem.Checked = this.IsLocked;
			Invalidate();
		}

		private void StayOnTopHandler(object sender, EventArgs e)
		{
			TopMost = !TopMost;
			((MenuItem)sender).Checked = TopMost;
		}

		private void DuplicateHandler(object sender, EventArgs e)
		{
			RulerInfo rulerInfo = this.GetRulerInfo();
			var copy = new MainForm(rulerInfo);
			copy.Show();
		}

		private MenuItem AddMenuItem(string text)
		{
			return AddMenuItem(text, Shortcut.None, this.MenuHandler);
		}

		private MenuItem AddMenuItem(string text, Shortcut shortcut, EventHandler handler)
		{
			MenuItem mi = new MenuItem(text);
			mi.Click += new EventHandler(handler);
			mi.Shortcut = shortcut;
			_menu.MenuItems.Add(mi);

			return mi;
		}

		private MenuItem AddMenuItem(string text, Shortcut shortcut, EventHandler handler, bool isChecked)
		{
			MenuItem mi = AddMenuItem(text, shortcut, handler);
			mi.Checked = isChecked;
			return mi;
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			ChangeOrientation();
			base.OnDoubleClick(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			_offset = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);
			_mouseDownPoint = MousePosition;
			_mouseDownRect = ClientRectangle;

			if (IsInResizableArea())
				_dragMode = DragMode.Resize;
			else
				_dragMode = DragMode.Move;

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_resizeRegion = ResizeRegion.None;
			if (_lockIconRegion.IsVisible(e.Location) )
			{
				LockHandler(this, e);
			}

			if (_rotateIconRegion.IsVisible(e.Location))
			{
				ChangeOrientation();
			}

			_dragMode = DragMode.None;

			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			switch (_dragMode)
			{
				case DragMode.Move:
					Location = new Point(MousePosition.X - _offset.X, MousePosition.Y - _offset.Y);
					break;

				case DragMode.Resize:
					HandleResize();
					break;

				default:
					if (IsInResizableArea() && !IsLocked)
					{
						Point clientCursorPos = PointToClient(MousePosition);
						_resizeRegion = GetResizeRegion(clientCursorPos);
						SetResizeCursor(_resizeRegion);
					}
					else
					{
						Cursor = Cursors.Default;
					}
					break;
			}

			Invalidate();

			base.OnMouseMove(e);
		}

		protected override void OnResize(EventArgs e)
		{
			this.SetToolTip();

			base.OnResize(e);
		}

		private bool IsInResizableArea()
		{
			Point clientCursorPos = PointToClient(MousePosition);
			Rectangle resizeInnerRect = ClientRectangle;
			resizeInnerRect.Inflate(-_resizeBorderWidth, -_resizeBorderWidth);

			return ClientRectangle.Contains(clientCursorPos) && !resizeInnerRect.Contains(clientCursorPos);
		}

		private void SetToolTip()
		{
			if (ShowToolTip)
				_toolTip.SetToolTip(this, string.Format("Width: {0} pixels\nHeight: {1} pixels", Width, Height));
			else
				_toolTip.RemoveAll();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Right:
				case Keys.Left:
				case Keys.Up:
				case Keys.Down:
					HandleMoveResizeKeystroke(e);
					break;

				case Keys.Space:
					ChangeOrientation();
					break;
			}

			base.OnKeyDown(e);
		}

		private void HandleMoveResizeKeystroke(KeyEventArgs e)
		{
			int sign = (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)? 1 : -1;
			if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						if (!IsLocked)
						{
							Width += (1 * sign);
						}
					}
					else
					{
						Left += (5 * sign);
					}
				}
				else
				{
					Left += (1 * sign);
				}
			}
			else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						if (!IsLocked)
						{
							Height += (1 * sign);
						}
					}
					else
					{
						Top += (5 * sign);
					}
				}
				else
				{
					Top += (1 * sign);
				}
			}
		}

		private void HandleResize()
		{
			if (this.IsLocked)
			{
				return;
			}

			int xDiff = MousePosition.X - _mouseDownPoint.X;
			if ((_resizeRegion & ResizeRegion.E) == ResizeRegion.E)
			{
				Width = _mouseDownRect.Width + xDiff;
			}
			else if ((_resizeRegion & ResizeRegion.W) == ResizeRegion.W)
			{
				Left = MousePosition.X;
				Width = _mouseDownRect.Width - xDiff;
			}

			int yDiff = MousePosition.Y - _mouseDownPoint.Y;
			if ((_resizeRegion & ResizeRegion.S) == ResizeRegion.S)
			{
				Height = _mouseDownRect.Height + yDiff;
			}
			else if ((_resizeRegion & ResizeRegion.N) == ResizeRegion.N)
			{
				Top = MousePosition.Y;
				Height = _mouseDownRect.Height - yDiff;
			}
		}

		private void SetResizeCursor(ResizeRegion region)
		{
			switch (region)
			{
				case ResizeRegion.N:
				case ResizeRegion.S:
					Cursor = Cursors.SizeNS;
					break;

				case ResizeRegion.E:
				case ResizeRegion.W:
					Cursor = Cursors.SizeWE;
					break;

				case ResizeRegion.NW:
				case ResizeRegion.SE:
					Cursor = Cursors.SizeNWSE;
					break;

				default:
					Cursor = Cursors.SizeNESW;
					break;
			}
		}

		private ResizeRegion GetResizeRegion(Point clientCursorPos)
		{
			ResizeRegion ret = ResizeRegion.None;
			if (clientCursorPos.Y <= _resizeBorderWidth)
			{
				ret |= ResizeRegion.N;
			}
			else if (clientCursorPos.Y >= Height - _resizeBorderWidth)
			{
				ret |= ResizeRegion.S;
			}

			if (clientCursorPos.X <= _resizeBorderWidth)
			{
				ret |= ResizeRegion.W;
			}
			else if (clientCursorPos.X >= Width - _resizeBorderWidth)
			{
				ret |= ResizeRegion.E;
			}
			return ret;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;

			int height = Height;
			int width = Width;

			if (IsVertical)
			{
				graphics.RotateTransform(90);
				graphics.TranslateTransform(0, -Width + 1);
				height = Width;
				width = Height;
			}

			DrawRuler(graphics, width, height);

			base.OnPaint(e);
		}

		private void DrawRuler(Graphics g, int formWidth, int formHeight)
		{
			// Border
			g.DrawRectangle(new Pen(_TickColor), 0, 0, formWidth - 1, formHeight - 1);

			DrawTicks(g, formWidth, formHeight);

			DrawCursor(g, formWidth, formHeight);

			// Rotate everything after this if we are vertical
			if (IsVertical)
				g.RotateTransform(-90);

			DrawDynamicLabels(g, formWidth, formHeight);

			DrawIcons(g, formWidth, formHeight);

		}

		private void DrawTicks(Graphics g, int formWidth, int formHeight)
		{
			for (int i = 0; i < formWidth; i++)
			{
				if (i % 2 == 0)
				{
					int tickHeight;
					if (i % 100 == 0)
					{
						tickHeight = 15;
						DrawTickLabel(g, i.ToString(), i, formHeight, tickHeight);
					}
					else if (i % 10 == 0)
					{
						tickHeight = 10;
					}
					else
					{
						tickHeight = 5;
					}

					DrawTick(g, i, formHeight, tickHeight);
				}
			}
		}

		private void DrawDynamicLabels(Graphics g, int formWidth, int formHeight)
		{
			Point pos = PointToClient(MousePosition);
			int taX = 10;
			int taY = formHeight - (Font.Height * 3);
			int tbX = taX;
			int tbY = formHeight - (Font.Height * 2);
			string dimensionLabelText = formWidth + "W x " + formHeight + "H px";
			string cursorLabelText = pos.X + "px";

			// Rotate the labels if we are Vertical
			if (IsVertical)
			{
				taX = (formHeight * -1) + 10;
				taY = formWidth - (Font.Height * 5);
				tbX = (formHeight * -1) + 10;
				tbY = formWidth - (Font.Height * 4);
				dimensionLabelText = formHeight + "W x " + formWidth + "H px";
				cursorLabelText = pos.Y + "px";
			}

			// Dimensions labels
			g.DrawString(dimensionLabelText, Font, new SolidBrush(_TickColor), taX, taY);
			g.DrawString(cursorLabelText, Font, new SolidBrush(_CursorColor), tbX, tbY);
		}

		private void DrawIcons(Graphics g, int formWidth, int formHeight)
		{

			// Lock Icon
			Icon lockIcon = IsLocked ? GetIcon("LockIcon") : GetIcon("UnlockIcon");
			Point lockIconPoint = new Point((formWidth - lockIcon.Width) - 10, formHeight - (lockIcon.Height * 2));
			_lockIconRegionR = new Rectangle(lockIconPoint, lockIcon.Size);

			if (IsVertical)
			{
				lockIconPoint = new Point((formHeight * -1) + (20 + lockIcon.Width), formWidth - (lockIcon.Height * 2));
				_lockIconRegionR = new Rectangle(new Point((20 + lockIcon.Width), lockIconPoint.Y) , lockIcon.Size);
			}

			// Keep a reference of the region where the icon is to detect a click on it
			_lockIconRegion = new Region(_lockIconRegionR);

			// Rotate Icon
			Icon RotateIcon = GetIcon("RotateIcon");
			Point RotateIconPoint = new Point(lockIconPoint.X - (10 + RotateIcon.Width), lockIconPoint.Y);
			_rotateIconRegionR = new Rectangle(RotateIconPoint, RotateIcon.Size);

			if (IsVertical)
			{
				_rotateIconRegionR = new Rectangle(new Point(10 , lockIconPoint.Y), lockIcon.Size);

			}

			_rotateIconRegion = new Region(_rotateIconRegionR);


			g.DrawIcon(lockIcon, lockIconPoint.X, lockIconPoint.Y);
			g.DrawIcon(RotateIcon, RotateIconPoint.X, RotateIconPoint.Y);
		}


		private void DrawCursor(Graphics g, int formWidth, int formHeight)
		{
			Point p = PointToClient(MousePosition);
			int op = IsVertical ? p.Y : p.X;
			g.DrawLine(new Pen(_CursorColor), new Point(op, 0), new Point(op, formHeight));
		}

		private static void DrawTick(Graphics g, int xPos, int formHeight, int tickHeight)
		{
			g.DrawLine(new Pen(_TickColor), xPos, 0, xPos, tickHeight);
		}

		private void DrawTickLabel(Graphics g, string text, int xPos, int formHeight, int height)
		{
			g.DrawString(text, Font, new SolidBrush(_TickColor), xPos, height);
		}

		private void UncheckMenuItems(Menu parent)
		{
			for (int i = 0; i < parent.MenuItems.Count; i++)
			{
				parent.MenuItems[i].Checked = false;
			}
		}

		private void OpacityMenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;
			UncheckMenuItems(mi.Parent);
			Opacity = double.Parse(mi.Text.Replace("%", "")) / 100;
			mi.Checked = true;
		}

		private void ColorMenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;
			UncheckMenuItems(mi.Parent);
			BackColor = s_ColorDict[mi.Text];
			mi.Checked = true;
		}

		private void MenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;

			switch (mi.Text)
			{
				case "Exit":
					Close();
					break;

				case "About...":
					string message = string.Format("Ruler v{0} by Jeff Key\nwww.sliver.com", Application.ProductVersion);
					MessageBox.Show(message, "About Ruler", MessageBoxButtons.OK, MessageBoxIcon.Information);
					break;

				default:
					MessageBox.Show("Unknown menu item.");
					break;
			}
		}

		private void ChangeOrientation()
		{
			this.IsVertical = !IsVertical;
			int width = Width;
			this.Width = Height;
			this.Height = width;
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.SuspendLayout();
			//
			// MainForm
			//
			this.ClientSize = new System.Drawing.Size(25, 25);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(25, 25);
			this.Name = "MainForm";
			this.ResumeLayout(false);

		}
	}
}
