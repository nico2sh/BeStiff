using System;
using System.Collections.ObjectModel;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>Control displaying an exclusive choice the user can select</summary>
/// <remarks>
///   The choice control is equivalent to a radio button - if more than one
///   choice control is on a dialog, only one can be selected at a time.
///   To have several choice groups on a dialog, use panels to group them.
/// </remarks>
public class ChoiceControl : PressableControl
{
	/// <summary>Text that will be shown on the button</summary>
	public string Text;

	/// <summary>Whether the choice is currently selected</summary>
	public bool Selected;

	/// <summary>Will be triggered when the choice is changed</summary>
	public event EventHandler Changed;

	/// <summary>Called when the button is pressed</summary>
	protected override void OnPressed()
	{
		if (!Selected)
		{
			Selected = true;
			unselectSiblings();
			OnChanged();
		}
	}

	/// <summary>Triggers the changed event</summary>
	protected virtual void OnChanged()
	{
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}

	/// <summary>Disables all sibling choices on the same level</summary>
	private void unselectSiblings()
	{
		if (base.Parent == null)
		{
			return;
		}
		Collection<Control> collection = base.Parent.Children;
		for (int i = 0; i < collection.Count; i++)
		{
			if (collection[i] is ChoiceControl choiceControl && choiceControl != this && choiceControl.Selected)
			{
				choiceControl.Selected = false;
				choiceControl.OnChanged();
			}
		}
	}
}
