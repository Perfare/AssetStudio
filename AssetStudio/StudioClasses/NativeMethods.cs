using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AssetStudio.StudioClasses
{
	// we needed a very fast select all for asset list view
	// source: https://stackoverflow.com/a/1118396
	public static class NativeMethods
	{
		private const int LVM_FIRST = 0x1000;
		private const int LVM_SETITEMSTATE = LVM_FIRST + 43;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct LVITEM
		{
			public int mask;
			public int iItem;
			public int iSubItem;
			public int state;
			public int stateMask;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pszText;

			public int cchTextMax;
			public int iImage;
			public IntPtr lParam;
			public int iIndent;
			public int iGroupId;
			public int cColumns;
			public IntPtr puColumns;
		};

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);

		/// <summary>
		/// Select all rows on the given listview
		/// </summary>
		/// <param name="list">The listview whose items are to be selected</param>
		public static void SelectAllItems(ListView list)
		{
			NativeMethods.SetItemState(list, -1, 2, 2);
		}

		/// <summary>
		/// Deselect all rows on the given listview
		/// </summary>
		/// <param name="list">The listview whose items are to be deselected</param>
		public static void DeselectAllItems(ListView list)
		{
			NativeMethods.SetItemState(list, -1, 2, 0);
		}

		/// <summary>
		/// Set the item state on the given item
		/// </summary>
		/// <param name="list">The listview whose item's state is to be changed</param>
		/// <param name="itemIndex">The index of the item to be changed</param>
		/// <param name="mask">Which bits of the value are to be set?</param>
		/// <param name="value">The value to be set</param>
		public static void SetItemState(ListView list, int itemIndex, int mask, int value)
		{
			LVITEM lvItem = new LVITEM();
			lvItem.stateMask = mask;
			lvItem.state = value;
			SendMessageLVItem(list.Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);
		}
	}
}
