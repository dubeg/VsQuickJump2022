using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using QuickJump2022.CustomControls;
using QuickJump2022.Data;
using QuickJump2022.Models;
using QuickJump2022.Options;
using QuickJump2022.Tools;
using MessageBox = System.Windows.Forms.MessageBox;

namespace QuickJump2022.Forms;

public class SearchForm : Form
{
	public List<ProjectItem> DocFileNames;

	public List<CodeItem> CodeItems;

	public Enums.ESearchType SearchType;

	private IContainer components;

	private TextBox txtSearch;

	private Panel pnlStatus;

	private Label lblSolutionName;

	private Label lblCountValue;

	private Label lblCount;

	private CustomListBox lstItems;

	public SearchForm(Enums.ESearchType type)
	{
		InitializeComponent();
		ThreadHelper.ThrowIfNotOnUIThread(".ctor");
		SearchType = type;
	}

	private async void SearchForm_Load(object sender, EventArgs e) {
        // TODO:
        // ThreadHelper.ThrowIfNotOnUIThread("SearchForm_Load");
        // _package.JoinableTaskFactory.SwitchToMainThreadAsync();

        pnlStatus.Visible = QuickJumpData.Instance.GeneralOptions.ShowStatusBar;
		base.Width = QuickJumpData.Instance.GeneralOptions.Width;
		CenterToScreen();
		base.Top += QuickJumpData.Instance.GeneralOptions.OffsetTop;
		base.Left += QuickJumpData.Instance.GeneralOptions.OffsetLeft;
		try
		{
			ClearData();
			Document document = ((_DTE)QuickJumpData.Instance.Dte).ActiveWindow.Document;
			if (SearchType == Enums.ESearchType.Files || SearchType == Enums.ESearchType.All)
			{
				DocFileNames = QuickJumpData.Instance.GetDocFilenames();
			}
			if (SearchType == Enums.ESearchType.Methods || SearchType == Enums.ESearchType.All)
			{
                CodeItems = await QuickJumpData.Instance.GetCodeItemsUsingSymbolFinderAsync(document);
                // CodeItems = await QuickJumpData.Instance.GetCodeItemsUsingWorkspaceAsync(document);
                // CodeItems = QuickJumpData.Instance.GetCodeItemsUsingManualCompilation(document);
            }
			txtSearch.Font = QuickJumpData.Instance.GeneralOptions.SearchFont;
			lstItems.ItemHeight = QuickJumpData.Instance.GeneralOptions.ItemFont.Height + 6;
			lblSolutionName.Text = ((_Solution)((_DTE)QuickJumpData.Instance.Dte).Solution).FullName;
			RefreshList();
			if (lstItems.Items.Count > 0)
			{
				lstItems.SelectedIndex = 0;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString());
		}
	}

	private void RefreshList()
	{
		ThreadHelper.ThrowIfNotOnUIThread("RefreshList");
		try
		{
			List<ListItemBase> objectList = new List<ListItemBase>(2048);
			string str = txtSearch.Text;
			lstItems.Items.Clear();
			if (SearchType == Enums.ESearchType.Files || SearchType == Enums.ESearchType.All)
			{
				foreach (ProjectItem doc in DocFileNames)
				{
					if (Utilities.Filter(doc.Name, str))
					{
						objectList.Add(new ListItemFile(doc, str));
					}
				}
			}
			if (SearchType == Enums.ESearchType.Methods || SearchType == Enums.ESearchType.All)
			{
				foreach (CodeItem item in CodeItems)
				{
					if (Utilities.Filter(item.Name, str))
					{
						objectList.Add(new ListItemCSharp(item, str));
					}
				}
			}

			// Apply fuzzy search scoring to improve relevance
			if (!string.IsNullOrEmpty(str))
			{
				// Update weights based on fuzzy search scores
				foreach (var item in objectList)
				{
					var fuzzyScore = FuzzySearch.ScoreFuzzy(item.Name, str);
					item.Weight = fuzzyScore.Score;
				}

				// Sort by fuzzy score first, then by the configured sort type
				objectList.Sort((a, b) =>
				{
					// Primary sort: fuzzy score (higher is better)
					if (a.Weight != b.Weight)
					{
						return b.Weight.CompareTo(a.Weight);
					}

					// Secondary sort: configured sort type
					var sortType = SearchType switch
					{
						Enums.ESearchType.Files => QuickJumpData.Instance.GeneralOptions.FileSortType,
						Enums.ESearchType.Methods => QuickJumpData.Instance.GeneralOptions.CSharpSortType,
						_ => QuickJumpData.Instance.GeneralOptions.MixedSortType,
					};

					return GetSortComparison(a, b, sortType);
				});
			}
			else
			{
				// No search query, use normal sorting
				SortObjects(objectList, SearchType switch
				{
					Enums.ESearchType.Files => QuickJumpData.Instance.GeneralOptions.FileSortType,
					Enums.ESearchType.Methods => QuickJumpData.Instance.GeneralOptions.CSharpSortType,
					_ => QuickJumpData.Instance.GeneralOptions.MixedSortType,
				});
			}

			ListBox.ObjectCollection items = lstItems.Items;
			object[] items2 = objectList.ToArray();
			items.AddRange(items2);
			lblCountValue.Text = $"{lstItems.Items.Count}";
			base.Height = Utilities.Clamp(lstItems.Items.Count * lstItems.ItemHeight + 56, 100, QuickJumpData.Instance.GeneralOptions.MaxHeight);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString());
		}
	}

	private void SortObjects(List<ListItemBase> objectList, Enums.SortType sortType)
	{
		switch (sortType)
		{
		case Enums.SortType.Alphabetical:
			objectList.Sort(Sort.Alphabetical);
			break;
		case Enums.SortType.AlphabeticalReverse:
			objectList.Sort(Sort.AlphabeticalReverse);
			break;
		case Enums.SortType.LineNumber:
			objectList.Sort(Sort.LineNumber);
			break;
		case Enums.SortType.LineNumberReverse:
			objectList.Sort(Sort.LineNumberReverse);
			break;
		case Enums.SortType.Weight:
			objectList.Sort(Sort.Weight);
			break;
		case Enums.SortType.WeightReverse:
			objectList.Sort(Sort.WeightReverse);
			break;
		}
	}

	private int GetSortComparison(ListItemBase a, ListItemBase b, Enums.SortType sortType)
	{
		switch (sortType)
		{
		case Enums.SortType.Alphabetical:
			return Sort.Alphabetical(a, b);
		case Enums.SortType.AlphabeticalReverse:
			return Sort.AlphabeticalReverse(a, b);
		case Enums.SortType.LineNumber:
			return Sort.LineNumber(a, b);
		case Enums.SortType.LineNumberReverse:
			return Sort.LineNumberReverse(a, b);
		case Enums.SortType.Weight:
			return Sort.Weight(a, b);
		case Enums.SortType.WeightReverse:
			return Sort.WeightReverse(a, b);
		case Enums.SortType.Fuzzy:
			return Sort.Fuzzy(a, b);
		case Enums.SortType.FuzzyReverse:
			return Sort.FuzzyReverse(a, b);
		default:
			return Sort.Alphabetical(a, b);
		}
	}

	private void ClearData()
	{
		DocFileNames = null;
		CodeItems = null;
		GC.Collect();
	}

	private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\u001b')
		{
			Close();
			e.Handled = true;
		}
		else if (e.KeyChar == '\r')
		{
			GotoItem();
			Close();
			e.Handled = true;
		}
	}

	private void txtSearch_KeyDown(object sender, KeyEventArgs e)
	{
		if (lstItems.Items.Count <= 0)
		{
			return;
		}
		if (e.KeyCode == Keys.Prior)
		{
			if (lstItems.SelectedIndex >= 10)
			{
				lstItems.SelectedIndex -= 10;
			}
			else
			{
				lstItems.SelectedIndex = 0;
			}
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Next)
		{
			if (lstItems.SelectedIndex < lstItems.Items.Count - 10)
			{
				lstItems.SelectedIndex += 10;
			}
			else
			{
				lstItems.SelectedIndex = lstItems.Items.Count - 1;
			}
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Up)
		{
			if (lstItems.SelectedIndex > 0)
			{
				CustomListBox customListBox = lstItems;
				int selectedIndex = customListBox.SelectedIndex - 1;
				customListBox.SelectedIndex = selectedIndex;
			}
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Down)
		{
			if (lstItems.SelectedIndex < lstItems.Items.Count - 1)
			{
				CustomListBox customListBox2 = lstItems;
				int selectedIndex = customListBox2.SelectedIndex + 1;
				customListBox2.SelectedIndex = selectedIndex;
			}
			e.Handled = true;
		}
	}

	private void GotoItem()
	{
		object selectedItem = lstItems.SelectedItem;
		if (selectedItem is ListItemBase)
		{
			((ListItemBase)selectedItem).Go();
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == (Keys.Back | Keys.Control))
		{
			txtSearch.Text = "";
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void txtSearch_TextChanged(object sender, EventArgs e)
	{
		ThreadHelper.ThrowIfNotOnUIThread("txtSearch_TextChanged");
		RefreshList();
		if (lstItems.Items.Count > 0)
		{
			lstItems.SelectedIndex = 0;
		}
	}

	private void lstItems_KeyUp(object sender, KeyEventArgs e)
	{
		TextBox textBox = txtSearch;
		int selectionStart = textBox.SelectionStart + 1;
		textBox.SelectionStart = selectionStart;
		txtSearch.Focus();
	}

	private void SearchForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		ClearData();
	}

	private void SearchForm_Paint(object sender, PaintEventArgs e)
	{
		// Draw a 1-pixel border around the form
		using (Pen borderPen = new Pen(QuickJumpData.Instance.GeneralOptions.BorderColor, 1f))
		{
			Rectangle borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
			e.Graphics.DrawRectangle(borderPen, borderRect);
		}
	}

	private void lstItems_DrawItem(object sender, DrawItemEventArgs e)
	{
		ThreadHelper.ThrowIfNotOnUIThread("lstItems_DrawItem");
		ListItemBase item = (ListItemBase)lstItems.Items[e.Index];
		GeneralOptionsPage options = QuickJumpData.Instance.GeneralOptions;
		string name = item.Name;
		string rightSide = item.Description ?? "";
		Color itemBackgroundColor = options.FileBackgroundColor;
		Color itemForegroundColor = options.FileForegroundColor;
		Color itemDescriptionForegroundColor = options.FileDescriptionForegroundColor;
		Color itemSelectedBackgroundColor = options.FileSelectedBackgroundColor;
		Color itemSelectedForegroundColor = options.FileSelectedForegroundColor;
		Color itemSelectedDescriptionForegroundColor = options.FileSelectedDescriptionForegroundColor;
		Font itemFont = options.ItemFont;
		int iconSpace = 24;
		if (e.Bounds.Height < 20 || !options.ShowIcons)
		{
			iconSpace = 0;
		}
		if (item is ListItemCSharp)
		{
			rightSide = $"{item.Description}:{item.Line}";
			itemBackgroundColor = options.CodeBackgroundColor;
			itemForegroundColor = options.CodeForegroundColor;
			itemDescriptionForegroundColor = options.CodeDescriptionForegroundColor;
			itemSelectedBackgroundColor = options.CodeSelectedBackgroundColor;
			itemSelectedForegroundColor = options.CodeSelectedForegroundColor;
			itemSelectedDescriptionForegroundColor = options.CodeSelectedDescriptionForegroundColor;
		}
		SizeF nameSize = e.Graphics.MeasureString(name, itemFont);
		SizeF descriptionSize = e.Graphics.MeasureString(rightSide, itemFont);
		if (nameSize.Width < (float)(e.Bounds.Width - 50))
		{
			while (nameSize.Width + descriptionSize.Width > (float)(e.Bounds.Width - (iconSpace + 6)))
			{
				rightSide = "..." + rightSide.Substring(4);
				descriptionSize = e.Graphics.MeasureString(rightSide, itemFont);
			}
		}
		else
		{
			rightSide = string.Empty;
			while (nameSize.Width > (float)(e.Bounds.Width - 50))
			{
				name = name.Substring(0, name.Length - 5) + "...";
				nameSize = e.Graphics.MeasureString(name, itemFont);
			}
		}
		if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
		{
			e = new DrawItemEventArgs(e.Graphics, itemFont, e.Bounds, e.Index, e.State ^ DrawItemState.Selected, e.ForeColor, itemSelectedBackgroundColor);
			e.DrawBackground();
			e.Graphics.DrawString(name, itemFont, new SolidBrush(itemSelectedForegroundColor), new PointF(e.Bounds.X + iconSpace, e.Bounds.Y + 4));
			e.Graphics.DrawString(rightSide, itemFont, new SolidBrush(itemSelectedDescriptionForegroundColor), new PointF((float)e.Bounds.Width - descriptionSize.Width - 2f, e.Bounds.Y + 4));
		}
		else
		{
			e = new DrawItemEventArgs(e.Graphics, itemFont, e.Bounds, e.Index, e.State, e.ForeColor, itemBackgroundColor);
			e.DrawBackground();
			e.Graphics.DrawString(name, itemFont, new SolidBrush(itemForegroundColor), new PointF(e.Bounds.X + iconSpace, e.Bounds.Y + 4));
			e.Graphics.DrawString(rightSide, itemFont, new SolidBrush(itemDescriptionForegroundColor), new PointF((float)e.Bounds.Width - descriptionSize.Width - 2f, e.Bounds.Y + 4));
		}
		e.Graphics.DrawLine(new Pen(options.ItemSeperatorColor, 1f), e.Bounds.X, e.Bounds.Y, e.Bounds.X + e.Bounds.Width, e.Bounds.Y);
		if (e.Bounds.Height >= 20 && options.ShowIcons)
		{
			int paddingTop = e.Bounds.Height / 2 - 8;
			e.Graphics.DrawIcon(item.IconImage, new Rectangle(e.Bounds.X + 4, e.Bounds.Y + paddingTop, 16, 16));
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.txtSearch = new System.Windows.Forms.TextBox();
		this.pnlStatus = new System.Windows.Forms.Panel();
		this.lblSolutionName = new System.Windows.Forms.Label();
		this.lblCountValue = new System.Windows.Forms.Label();
		this.lblCount = new System.Windows.Forms.Label();
		this.lstItems = new QuickJump2022.CustomControls.CustomListBox();
		this.pnlStatus.SuspendLayout();
		base.SuspendLayout();
		this.txtSearch.BackColor = System.Drawing.SystemColors.WindowFrame;
		this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtSearch.Dock = System.Windows.Forms.DockStyle.Top;
		this.txtSearch.Font = new System.Drawing.Font("Consolas", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, 0);
		this.txtSearch.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.txtSearch.Location = new System.Drawing.Point(0, 0);
		this.txtSearch.Name = "txtSearch";
		this.txtSearch.Size = new System.Drawing.Size(700, 26);
		this.txtSearch.TabIndex = 0;
		this.txtSearch.WordWrap = false;
		this.txtSearch.TextChanged += new System.EventHandler(txtSearch_TextChanged);
		this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(txtSearch_KeyDown);
		this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(txtSearch_KeyPress);
		this.pnlStatus.Controls.Add(this.lblSolutionName);
		this.pnlStatus.Controls.Add(this.lblCountValue);
		this.pnlStatus.Controls.Add(this.lblCount);
		this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.pnlStatus.Location = new System.Drawing.Point(0, 168);
		this.pnlStatus.Name = "pnlStatus";
		this.pnlStatus.Size = new System.Drawing.Size(700, 21);
		this.pnlStatus.TabIndex = 5;
		this.lblSolutionName.BackColor = System.Drawing.SystemColors.ControlText;
		this.lblSolutionName.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblSolutionName.Font = new System.Drawing.Font("Consolas", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, 0);
		this.lblSolutionName.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
		this.lblSolutionName.Location = new System.Drawing.Point(114, 0);
		this.lblSolutionName.Name = "lblSolutionName";
		this.lblSolutionName.Size = new System.Drawing.Size(586, 21);
		this.lblSolutionName.TabIndex = 7;
		this.lblSolutionName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblCountValue.Dock = System.Windows.Forms.DockStyle.Left;
		this.lblCountValue.Font = new System.Drawing.Font("Consolas", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, 0);
		this.lblCountValue.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
		this.lblCountValue.Location = new System.Drawing.Point(64, 0);
		this.lblCountValue.Name = "lblCountValue";
		this.lblCountValue.Size = new System.Drawing.Size(50, 21);
		this.lblCountValue.TabIndex = 6;
		this.lblCountValue.Text = "0";
		this.lblCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lblCount.Dock = System.Windows.Forms.DockStyle.Left;
		this.lblCount.Font = new System.Drawing.Font("Consolas", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, 0);
		this.lblCount.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
		this.lblCount.Location = new System.Drawing.Point(0, 0);
		this.lblCount.Name = "lblCount";
		this.lblCount.Size = new System.Drawing.Size(64, 21);
		this.lblCount.TabIndex = 5;
		this.lblCount.Text = "Results:";
		this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lstItems.BackColor = System.Drawing.Color.DimGray;
		this.lstItems.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstItems.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lstItems.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.lstItems.Font = new System.Drawing.Font("Consolas", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, 0);
		this.lstItems.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.lstItems.FormattingEnabled = true;
		this.lstItems.IntegralHeight = false;
		this.lstItems.ItemHeight = 20;
		this.lstItems.Location = new System.Drawing.Point(0, 26);
		this.lstItems.Name = "lstItems";
		this.lstItems.ShowScrollbar = false;
		this.lstItems.Size = new System.Drawing.Size(700, 142);
		this.lstItems.TabIndex = 6;
		this.lstItems.DrawItem += new System.Windows.Forms.DrawItemEventHandler(lstItems_DrawItem);
		this.lstItems.KeyUp += new System.Windows.Forms.KeyEventHandler(lstItems_KeyUp);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.ControlText;
		base.ClientSize = new System.Drawing.Size(700, 189);
		base.ControlBox = false;
		base.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
		base.Controls.Add(this.lstItems);
		base.Controls.Add(this.pnlStatus);
		base.Controls.Add(this.txtSearch);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "SearchForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(SearchForm_FormClosed);
		base.Load += new System.EventHandler(SearchForm_Load);
		base.Paint += new System.Windows.Forms.PaintEventHandler(SearchForm_Paint);
		this.pnlStatus.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
