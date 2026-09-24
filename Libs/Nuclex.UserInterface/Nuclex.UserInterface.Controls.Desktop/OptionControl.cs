using System;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>Control displaying an option the user can toggle on and off</summary>
public class OptionControl : PressableControl
{
	/// <summary>Text that will be shown on the button</summary>
	public string Text;

	/// <summary>Whether the option is currently selected</summary>
	public bool Selected;

	/// <summary>Will be triggered when the choice is changed</summary>
	public event EventHandler Changed;

	/// <summary>Called when the button is pressed</summary>
	protected override void OnPressed()
	{
		Selected = !Selected;
		OnChanged();
	}

	/// <summary>Triggers the changed event</summary>
	protected virtual void OnChanged()
	{
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}
}
