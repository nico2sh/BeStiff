using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nuclex.Input;
using Nuclex.Support.Collections;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>List showing a sequence of items</summary>
public class ListControl : Control, IFocusable
{
	/// <summary>
	///   Row locator through which the list can detect which row the mouse has
	///   been pressed down on
	/// </summary>
	private IListRowLocator listRowLocator;

	/// <summary>Last known Y coordinate of the mouse</summary>
	private float mouseY;

	/// <summary>How the list lets the user select from its items</summary>
	private ListSelectionMode selectionMode;

	/// <summary>Items contained in the list</summary>
	private ObservableCollection<string> items;

	/// <summary>Items currently selected in the list</summary>
	private ObservableCollection<int> selectedItems;

	/// <summary>Slider the lists uses to scroll through its items</summary>
	private VerticalSliderControl slider;

	/// <summary>How the user can select items in the list</summary>
	public ListSelectionMode SelectionMode
	{
		get
		{
			return selectionMode;
		}
		set
		{
			selectionMode = value;
		}
	}

	/// <summary>Slider the list uses to scroll through its items</summary>
	public VerticalSliderControl Slider => slider;

	/// <summary>Items being displayed in the list</summary>
	public IList<string> Items => items;

	/// <summary>Indices of the items current selected in the list</summary>
	public IList<int> SelectedItems => selectedItems;

	/// <summary>Whether the control can currently obtain the input focus</summary>
	bool IFocusable.CanGetFocus => true;

	/// <summary>
	///   Can be set by renderers to enable selection of list items by mouse
	/// </summary>
	public IListRowLocator ListRowLocator
	{
		get
		{
			return listRowLocator;
		}
		set
		{
			if (value != listRowLocator)
			{
				listRowLocator = value;
				updateSlider();
			}
		}
	}

	/// <summary>Triggered when the selected items in list have changed</summary>
	public event EventHandler SelectionChanged;

	/// <summary>Initializes a new list box control</summary>
	public ListControl()
	{
		items = new ObservableCollection<string>();
		items.Cleared += itemsCleared;
		items.ItemAdded += itemAdded;
		items.ItemRemoved += itemRemoved;
		selectedItems = new ObservableCollection<int>();
		selectedItems.Cleared += selectionCleared;
		selectedItems.ItemAdded += selectionAdded;
		selectedItems.ItemRemoved += selectionRemoved;
		slider = new VerticalSliderControl();
		slider.Bounds = new UniRectangle(new UniScalar(1f, -20f), new UniScalar(0f, 0f), new UniScalar(0f, 20f), new UniScalar(1f, 0f));
		base.Children.Add(slider);
	}

	/// <summary>Called when a mouse button has been pressed down</summary>
	/// <param name="button">Index of the button that has been pressed</param>
	/// <remarks>
	///   If this method states that a mouse press is processed by returning
	///   true, that means the control did something with it and the mouse press
	///   should not be acted upon by any other listener.
	/// </remarks>
	protected override void OnMousePressed(MouseButtons button)
	{
		if (listRowLocator != null)
		{
			int row = listRowLocator.GetRow(GetAbsoluteBounds(), slider.ThumbPosition, items.Count, mouseY);
			if (row >= 0 && row < items.Count)
			{
				OnRowClicked(row);
			}
		}
	}

	/// <summary>Called when the user has clicked on a row in the list</summary>
	/// <param name="row">Row the user has clicked on</param>
	/// <remarks>
	///   The default behavior of the list control in multi select mode is to
	///   toggle items that are clicked between selected and unselected. If you
	///   need different behavior (for example, dragging a selected region or
	///   selecting sequences of items by holding the shift key), you can override
	///   this method and handle the selection behavior yourself.
	/// </remarks>
	protected virtual void OnRowClicked(int row)
	{
		switch (selectionMode)
		{
		case ListSelectionMode.Single:
			if (selectedItems.Count == 1)
			{
				if (selectedItems[0] == row)
				{
					break;
				}
				selectedItems[0] = row;
			}
			else
			{
				selectedItems.Clear();
				selectedItems.Add(row);
			}
			OnSelectionChanged();
			break;
		case ListSelectionMode.Multi:
			if (!selectedItems.Remove(row))
			{
				selectedItems.Add(row);
			}
			OnSelectionChanged();
			break;
		case ListSelectionMode.None:
			break;
		}
	}

	/// <summary>Called when the mouse wheel has been rotated</summary>
	/// <param name="ticks">Number of ticks that the mouse wheel has been rotated</param>
	protected override void OnMouseWheel(float ticks)
	{
		if (listRowLocator != null)
		{
			RectangleF absoluteBounds = GetAbsoluteBounds();
			float num = items.Count;
			float num2 = absoluteBounds.Height / listRowLocator.GetRowHeight(absoluteBounds);
			float num3 = num - num2;
			slider.ThumbPosition -= 1f / num3 * ticks;
			slider.ThumbPosition = MathHelper.Clamp(slider.ThumbPosition, 0f, 1f);
		}
	}

	/// <summary>Called when the mouse position is updated</summary>
	/// <param name="x">X coordinate of the mouse cursor on the control</param>
	/// <param name="y">Y coordinate of the mouse cursor on the control</param>
	protected override void OnMouseMoved(float x, float y)
	{
		mouseY = y;
	}

	/// <summary>Called when the selected items in the list have changed</summary>
	protected virtual void OnSelectionChanged()
	{
		if (this.SelectionChanged != null)
		{
			this.SelectionChanged(this, EventArgs.Empty);
		}
	}

	/// <summary>Called when an item is removed from the items list</summary>
	/// <param name="sender">List the item has been removed from</param>
	/// <param name="arguments">Contains the item that has been removed</param>
	private void itemRemoved(object sender, ItemEventArgs<string> arguments)
	{
		updateSlider();
	}

	/// <summary>Called when an item is added to the items list</summary>
	/// <param name="sender">List the item has been added to</param>
	/// <param name="arguments">Contains the item that has been added</param>
	private void itemAdded(object sender, ItemEventArgs<string> arguments)
	{
		updateSlider();
	}

	/// <summary>Called when the items list is about to clear itself</summary>
	/// <param name="sender">Items list that is about to clear itself</param>
	/// <param name="arguments">Not used</param>
	private void itemsCleared(object sender, EventArgs arguments)
	{
		updateSlider();
	}

	/// <summary>Called when an entry is added to the list of selected items</summary>
	/// <param name="sender">List to which an item was added to</param>
	/// <param name="arguments">Contains the added item</param>
	private void selectionAdded(object sender, ItemEventArgs<int> arguments)
	{
		OnSelectionChanged();
	}

	/// <summary>
	///   Called when an entry is removed from the list of selected items
	/// </summary>
	/// <param name="sender">List from which an item was removed</param>
	/// <param name="arguments">Contains the removed item</param>
	private void selectionRemoved(object sender, ItemEventArgs<int> arguments)
	{
		OnSelectionChanged();
	}

	/// <summary>Called when the selected items list is about to clear itself</summary>
	/// <param name="sender">List that is about to clear itself</param>
	/// <param name="arguments">Not Used</param>
	private void selectionCleared(object sender, EventArgs arguments)
	{
		OnSelectionChanged();
	}

	/// <summary>Updates the size and position of the list's slider</summary>
	private void updateSlider()
	{
		if (base.Screen != null && listRowLocator != null)
		{
			RectangleF absoluteBounds = GetAbsoluteBounds();
			float num = items.Count;
			float num2 = absoluteBounds.Height / listRowLocator.GetRowHeight(absoluteBounds);
			slider.ThumbSize = Math.Min(1f, num2 / num);
		}
	}
}
