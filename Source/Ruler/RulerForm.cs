using System;
using System.Collections.Generic;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Ruler
{
	sealed public class RulerForm : Form, IRulerInfo
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
		private ResourceManager _resources = new ResourceManager(typeof(RulerForm));
		private Point _offset;
		private Rectangle _mouseDownRect;
		private int _resizeBorderWidth = 5;
		private Point _mouseDownPoint;
		private ResizeRegion _resizeRegion = ResizeRegion.None;
		private ContextMenu _menu = new ContextMenu();
		private MenuItem _topmostMenuItem;
		private MenuItem _lockMenuItem;
		private MenuItem _verticalMenuItem;
		private MenuItem _flipMenuItem;
		private MenuItem _tooltipMenuItem;
		private MenuItem _opacityParentMenuItem;
		private MenuItem _colorParentMenuItem;
		private MenuItem _upTicksMenuItem;
		private MenuItem _downTicksMenuItem;
		private Dictionary<Color, MenuItem> _colorMenuItems = new Dictionary<Color, MenuItem>();
		private DragMode _dragMode = DragMode.None;
		private Region _lockIconRegion;
		private Region _flipIconRegion;
		private static Color _TickColor = ColorTranslator.FromHtml("#3E2815");
		private static Color _CursorColor = Color.FromArgb(200, _TickColor);

		public RulerForm() : this(new RulerInfo())
		{
		}

		public RulerForm(RulerInfo rulerInfo)
		{
			this.Init(rulerInfo);
			RulerApplicationContext context = RulerApplicationContext.CurrentContext;
			context.RegisterRuler(this);
		}

		private int _length;
		public int Length
		{
			get { return _length; }
			set
			{
				if (value == _length) return;
				if (IsVertical)
				{
					base.Height = value;
					_length = base.Height;
				}
				else
				{
					base.Width = value;
					_length = base.Width;
				}
				this.SaveInfo();
			}
		}

		private int _thickness;
		public int Thickness
		{
			get { return _thickness; }
			set
			{
				if (value == _thickness) return;
				if (IsVertical)
				{
					base.Width = value;
					_thickness = base.Width;
				}
				else
				{
					base.Height = value;
					_thickness = base.Height;

				}
				this.SaveInfo();
			}
		}

		private bool _isVertical;
		public bool IsVertical
		{
			get { return _isVertical; }
			set
			{
				if (value == _isVertical) return;
				MinimumSize = new Size(MinimumSize.Height, MinimumSize.Width);
				_isVertical = value;
				base.Width = IsVertical ? Thickness : Length;
				base.Height = IsVertical ? Length : Thickness;
				this.SaveInfo();
			}
		}

		public new int Width
		{
			get { return base.Width; }
			set
			{
				if (IsVertical)
				{
					Thickness = value;
				}
				else
				{
					Length = value;
				}
			}
		}

		public new int Height
		{
			get { return base.Height; }
			set
			{
				if (IsVertical)
				{
					Length = value;
				}
				else
				{
					Thickness = value;
				}
			}
		}

		private bool _isLocked;
		public bool IsLocked
		{
			get { return _isLocked; }
			set
			{
				if (_isLocked == value) return;
				_isLocked = value;
				this.SaveInfo();
			}
		}

		private bool _showToolTip;
		public bool ShowToolTip
		{
			get { return _showToolTip; }
			set
			{
				if (_showToolTip == value) return;
				_showToolTip = value;
				this.SaveInfo();
			}
		}

		private bool _showUpTicks;
		public bool ShowUpTicks
		{
			get { return _showUpTicks; }
			set
			{
				if (value == _showUpTicks) return;
				_showUpTicks = value;
				Invalidate();
				this.SaveInfo();
			}
		}

		private bool _showDownTicks;
		public bool ShowDownTicks
		{
			get { return _showDownTicks; }
			set
			{
				if (value == _showDownTicks) return;
				_showDownTicks = value;
				Invalidate();
				this.SaveInfo();
			}
		}

		private bool _isFlipped;
		public bool IsFlipped
		{
			get { return _isFlipped; }
			set
			{
				if (value == _isFlipped) return;
				_isFlipped = value;
				Invalidate();
				this.SaveInfo();
			}
		}

		public new double Opacity
		{
			get { return base.Opacity; }
			set
			{
				if (value == base.Opacity)
					return;
				base.Opacity = value;
				this.SaveInfo();
			}
		}

		public new bool TopMost
		{
			get { return base.TopMost; }
			set
			{
				if (value == base.TopMost) return;
				base.TopMost = value;
				this.SaveInfo();
			}
		}

		public new Color BackColor
		{
			get { return base.BackColor; }
			set
			{
				if (value == base.BackColor)
					return;
				base.BackColor = value;
				this.SaveInfo();
			}
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

		private void SetUpMenu()
		{
			_topmostMenuItem = AddMenuItem("Stay On Top", Shortcut.None, (s, e) => ChangeStayOnTop(), TopMost);
			_verticalMenuItem = AddMenuItem("Vertical", Shortcut.None, (s, e) => ChangeOrientation(), IsVertical);
			_flipMenuItem = AddMenuItem("Flip", Shortcut.None, (s, e) => ChangeDirection(), IsFlipped);
			_lockMenuItem = AddMenuItem("Lock resizing", Shortcut.None, (s, e) => ChangeLock(), IsLocked);
			_tooltipMenuItem = AddMenuItem("Tool Tip", Shortcut.None, (s, e) => ChangeShowTooltip(), ShowToolTip);
			_opacityParentMenuItem = AddMenuItem("Opacity");
			_colorParentMenuItem = AddMenuItem("Color");
			MenuItem ticksMenuItem = AddMenuItem("Ticks");
			AddMenuItem("Set size...", Shortcut.None, SetSizeHandler);
			AddMenuItem("Duplicate", Shortcut.None, DuplicateHandler);
			AddMenuItem("Reset ruler", Shortcut.None, ResetHandler);
			AddMenuItem("-");
			AddMenuItem("About...");
			AddMenuItem("-");
			AddMenuItem("Close");

			for (int i = 10; i <= 100; i += 10)
			{
				MenuItem subMenu = new MenuItem(i + "%");
				subMenu.Checked = (i == Opacity * 100);
				subMenu.Click += OpacityMenuHandler;
				_opacityParentMenuItem.MenuItems.Add(subMenu);
			}

			// Add colors to color menus
			foreach (var color in RulerInfo.Colors)
			{
				// Capitalize
				char[] a = color.Key.ToCharArray();
				a[0] = char.ToUpper(a[0]);
				var name = new string(a);

				MenuItem subMenu = new MenuItem(name);
				if (color.Value == BackColor)
				{
					subMenu.Checked = true;
				}
				subMenu.Click += (s, e) => ColorMenuHandler(s, color.Value);
				_colorParentMenuItem.MenuItems.Add(subMenu);

				_colorMenuItems[color.Value] = subMenu;
			}

			// Ticks sub menus
			_upTicksMenuItem = new MenuItem("Show up ticks");
			_upTicksMenuItem.Checked = ShowUpTicks;
			_upTicksMenuItem.Click += (s, e) => ChangeShowUpTicks();
			ticksMenuItem.MenuItems.Add(_upTicksMenuItem);

			_downTicksMenuItem = new MenuItem("Show down ticks");
			_downTicksMenuItem.Checked = ShowDownTicks;
			_downTicksMenuItem.Click += (s, e) => ChangeShowDownTicks();
			ticksMenuItem.MenuItems.Add(_downTicksMenuItem);
		}

		private Icon GetIcon(string name)
		{
			return (Icon)(_resources.GetObject(name));
		}

		private void SetSizeHandler(object sender, EventArgs e)
		{
			SetSizeForm form = new SetSizeForm(this.Length, this.Thickness);

			if (this.TopMost)
			{
				form.TopMost = true;
			}

			if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Dimension dim = form.GetNewDimension();

				this.Length = dim.Length;
				this.Thickness = dim.Thickness;
			}
		}

		private void DuplicateHandler(object sender, EventArgs e)
		{
			lock (RulerApplicationContext.CurrentContext)
			{
				var copy = new RulerForm();
				copy.Show();
			}
		}

		private void ResetHandler(object sender, EventArgs e)
		{
			Properties.Settings.Default.Reset();
			new RulerInfo().CopyInto(this);

			// Now update all the menus
			_topmostMenuItem.Checked = TopMost;
			_lockMenuItem.Checked = IsLocked;
			_verticalMenuItem.Checked = IsVertical;
			_flipMenuItem.Checked = IsFlipped;
			_tooltipMenuItem.Checked = ShowToolTip;
			_upTicksMenuItem.Checked = ShowUpTicks;
			_downTicksMenuItem.Checked = ShowDownTicks;

			// Check proper opacity and color...
			UncheckMenuItems(_opacityParentMenuItem);
			UncheckMenuItems(_colorParentMenuItem);
			_opacityParentMenuItem.MenuItems[(int) (Opacity * 10) - 1].Checked = true;
			_colorMenuItems[BackColor].Checked = true;

			// Reset tooltip
			SetToolTip();

			Invalidate();
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

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			lock (RulerApplicationContext.CurrentContext)
			{
				base.OnFormClosed(e);
				RulerApplicationContext context = RulerApplicationContext.CurrentContext;
				context.UnregisterRuler(this);
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			if (_lockIconRegion.IsVisible(PointToClient(MousePosition)) ||
				_flipIconRegion.IsVisible(PointToClient(MousePosition)))
				return;

			if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				ChangeDirection();
			}
			else
			{
				ChangeOrientation();
			}
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
			if (_lockIconRegion.IsVisible(e.Location))
			{
				ChangeLock();
			}
			else if (_flipIconRegion.IsVisible(e.Location))
			{
				ChangeDirection();
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

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			this.SaveInfo();
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
				_toolTip.SetToolTip(this, string.Format("Length: {0} pixels\nThickness: {1} pixels", Length, Thickness));
			else
				_toolTip.RemoveAll();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					if (e.Shift)
						Application.Exit();
					else
						Close();
					break;

				case Keys.Right:
				case Keys.Left:
				case Keys.Up:
				case Keys.Down:
					HandleMoveResizeKeystroke(e);
					break;

				case Keys.Space:
					if (e.Shift)
						ChangeDirection();
					else
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
				int prevWidth = Width;
				Width = _mouseDownRect.Width - xDiff;
				if (prevWidth != Width)
					Left = MousePosition.X;
			}

			int yDiff = MousePosition.Y - _mouseDownPoint.Y;
			if ((_resizeRegion & ResizeRegion.S) == ResizeRegion.S)
			{
				Height = _mouseDownRect.Height + yDiff;
			}
			else if ((_resizeRegion & ResizeRegion.N) == ResizeRegion.N)
			{
				int prevHeight = Height;
				Height = _mouseDownRect.Height - yDiff;
				if (prevHeight != Height)
					Top = MousePosition.Y;
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

			if (IsVertical)
			{
				graphics.RotateTransform(90);
				graphics.TranslateTransform(0, -Width + 1);
			}

			DrawRuler(graphics, Length, Thickness);

			base.OnPaint(e);
		}

		private void DrawRuler(Graphics g, int formWidth, int formHeight)
		{
			// Border
			g.DrawRectangle(new Pen(_TickColor), 0, 0, formWidth - 1, formHeight - 1);

			DrawTicks(g, formWidth, formHeight);

			DrawCursor(g, formHeight);

			// Rotate everything after this if we are vertical
			if (IsVertical)
			{
				g.TranslateTransform(0, Width - 1);
				g.RotateTransform(-90);
			}

			DrawDynamicLabelsAndIcon(g);
		}

		private void DrawTicks(Graphics g, int formWidth, int formHeight)
		{
			for (int i = 0; i < formWidth; i+=2)
			{
				int tickHeight;
				int pos = IsFlipped ? (formWidth - 1 - i) : i;
				if (i % 100 == 0)
				{
					tickHeight = 15;
					string label = i.ToString();
					int labelSize = (int)g.MeasureString(label, Font).Width;
					int labelPos = IsFlipped ? (pos - labelSize) : pos;
					DrawTickLabel(g, label, labelPos, formHeight, tickHeight);
				}
				else if (i % 10 == 0)
				{
					tickHeight = 10;
				}
				else
				{
					tickHeight = 5;
				}

				DrawTick(g, pos, formHeight, tickHeight);
			}
		}

		private Point GetDimensionLabelPos(SizeF dimensionTextSize, bool useVertical)
		{
			int textDimensionToUse = (int)(useVertical ? dimensionTextSize.Height * 2 : dimensionTextSize.Width);
			const int distanceToBorder = 15;
			if (useVertical)
			{
				int yPlacement = distanceToBorder;
				if (IsFlipped)
					yPlacement = Length - (textDimensionToUse + distanceToBorder);

				if (!ShowDownTicks)
					return new Point(distanceToBorder, yPlacement);

				if (!ShowUpTicks)
					return new Point(Width-(int)dimensionTextSize.Width - 2*distanceToBorder, yPlacement);

				return new Point((Width - (int)dimensionTextSize.Width - distanceToBorder)/2, yPlacement);
			}

			int xPlacement = distanceToBorder;
			if (IsFlipped)
				xPlacement = Length - (textDimensionToUse + 2 * distanceToBorder);

			// For very slim rulers, center the labels in height
			if ((Thickness - (Font.Height * 2)) <= 2 * distanceToBorder)
				return new Point(xPlacement, (Thickness / 2)- Font.Height);

			if (!ShowDownTicks)
				return new Point(xPlacement, Thickness - (Font.Height * 2 + distanceToBorder));

			if (!ShowUpTicks)
				return new Point(xPlacement, distanceToBorder);

			return new Point(xPlacement, (Thickness / 2)- Font.Height);
		}

		private int GetCursorPos()
		{
			Point pos = PointToClient(MousePosition);
			int op = IsVertical ? pos.Y : pos.X;
			return (op < 0) ? 0 : ((op > Length) ? Length : op);
		}

		private void DrawDynamicLabelsAndIcon(Graphics g)
		{
			string dimensionLabelText = Width + "x" + Height + " px";
			string cursorLabelText = (IsFlipped ? Length - GetCursorPos() : GetCursorPos()) + " px";
			SizeF dimensionTextSize = g.MeasureString(dimensionLabelText, Font);

			bool transformLockRegion = false;
			bool useVertical = IsVertical;
			if (IsVertical && Width < 105)
			{
				useVertical = false;
				g.RotateTransform(90);
				g.TranslateTransform(0, -Width + 1);
				transformLockRegion = true;
			}

			Point labelPos = GetDimensionLabelPos(dimensionTextSize, useVertical);
			int taX = labelPos.X;
			int taY = labelPos.Y;
			int tbX = labelPos.X;
			int tbY = labelPos.Y + Font.Height;

			// Dimensions labels
			g.DrawString(dimensionLabelText, Font, new SolidBrush(_TickColor), taX, taY);
			g.DrawString(cursorLabelText, Font, new SolidBrush(_CursorColor), tbX, tbY);

			DrawFlipIcon(g, taX + (int)dimensionTextSize.Width, taY, transformLockRegion);
			DrawLockIcon(g, taX + (int)dimensionTextSize.Width, tbY, transformLockRegion);
		}

		private void DrawFlipIcon(Graphics g, int leftPos, int bottomPos, bool transformLockRegion)
		{
			// Lock Icon
			Bitmap lockIcon = (IsVertical && !transformLockRegion ? GetIcon("FlipVertIcon") : GetIcon("FlipIcon")).ToBitmap();
			Point lockIconPoint = new Point(leftPos, bottomPos);
			Point lockRegionPt = lockIconPoint;
			if (transformLockRegion)
			{
				lockRegionPt = new Point(Width - lockIconPoint.Y - lockIcon.Height, lockIconPoint.X);
			}
			// Keep a reference of the region where the icon is to detect a click on it
			_flipIconRegion = new Region(new Rectangle(lockRegionPt, lockIcon.Size));

			g.DrawImage(lockIcon, lockIconPoint.X, lockIconPoint.Y);
		}

		private void DrawLockIcon(Graphics g, int leftPos, int bottomPos, bool transformLockRegion)
		{
			// Lock Icon
			Bitmap lockIcon = (IsLocked ? GetIcon("LockIcon") : GetIcon("UnlockIcon")).ToBitmap();
			Point lockIconPoint = new Point(leftPos, bottomPos);
			Point lockRegionPt = lockIconPoint;
			if (transformLockRegion)
			{
				lockRegionPt = new Point(Width - lockIconPoint.Y - lockIcon.Height, lockIconPoint.X);
			}
			// Keep a reference of the region where the icon is to detect a click on it
			_lockIconRegion = new Region(new Rectangle(lockRegionPt, lockIcon.Size));

			g.DrawImage(lockIcon, lockIconPoint.X, lockIconPoint.Y);
		}

		private void DrawCursor(Graphics g, int formHeight)
		{
			int pos = GetCursorPos();
			g.DrawLine(new Pen(_CursorColor), new Point(pos, 0), new Point(pos, formHeight));
		}

		private void DrawTick(Graphics g, int xPos, int formHeight, int tickHeight)
		{
			if (ShowUpTicks)
				g.DrawLine(new Pen(_TickColor), xPos, 0, xPos, tickHeight);

			if (ShowDownTicks)
				g.DrawLine(Pens.Black, xPos, formHeight, xPos, formHeight - tickHeight);
		}

		private void DrawTickLabel(Graphics g, string text, int xPos, int formHeight, int height)
		{
			if (ShowUpTicks && ShowDownTicks && formHeight <= 60)
			{
				// When space is limited and we have to draw labels for both up and down ticks, only draw in the middle of the ruler
				g.DrawString(text, Font, new SolidBrush(_TickColor), xPos, (formHeight-FontHeight)/2);
				return;
			}

			if (ShowUpTicks)
				g.DrawString(text, Font, new SolidBrush(_TickColor), xPos, height);

			if (ShowDownTicks)
				g.DrawString(text, Font, Brushes.Black, xPos, formHeight - height - Font.Height);
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

		private void ColorMenuHandler(object sender, Color color)
		{
			MenuItem mi = (MenuItem)sender;
			UncheckMenuItems(mi.Parent);
			BackColor = color;
			mi.Checked = true;
		}

		private void MenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;

			switch (mi.Text)
			{
				case "Close":
					Close();
					break;

				case "About...":
					using (AboutForm form = new Ruler.AboutForm())
					{
						form.ShowDialog(this);
					}
					break;

				default:
					MessageBox.Show("Unknown menu item.");
					break;
			}
		}

		private void ChangeOrientation()
		{
			IsVertical = !IsVertical;
			_verticalMenuItem.Checked = IsVertical;
			Invalidate();
		}

		private void ChangeDirection()
		{
			IsFlipped = !IsFlipped;
			_flipMenuItem.Checked = IsFlipped;
			Invalidate();
		}

		private void ChangeLock()
		{
			IsLocked = !IsLocked;
			_lockMenuItem.Checked = IsLocked;
			Invalidate();
		}

		private void ChangeStayOnTop()
		{
			TopMost = !TopMost;
			_topmostMenuItem.Checked = TopMost;
			Invalidate();
		}

		private void ChangeShowTooltip()
		{
			ShowToolTip = !ShowToolTip;
			_tooltipMenuItem.Checked = ShowToolTip;
			SetToolTip();
		}

		private void ChangeShowUpTicks()
		{
			ShowUpTicks = !ShowUpTicks;
			_upTicksMenuItem.Checked = ShowUpTicks;
			Invalidate();
		}

		private void ChangeShowDownTicks()
		{
			ShowDownTicks = !ShowDownTicks;
			_downTicksMenuItem.Checked = ShowDownTicks;
			Invalidate();
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RulerForm));
			this.SuspendLayout();
			//
			// RulerForm
			//
			this.ClientSize = new System.Drawing.Size(100, 55);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(100, 55);
			this.Name = "RulerForm";
			this.ResumeLayout(false);
		}
	}
}
