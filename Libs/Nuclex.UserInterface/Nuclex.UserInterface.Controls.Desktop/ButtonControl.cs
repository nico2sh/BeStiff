using System;

namespace Nuclex.UserInterface.Controls.Desktop;

/// <summary>Pushable button that can initiate an action</summary>
public class ButtonControl : PressableControl
{
	/// <summary>Text that will be shown on the button</summary>
	public string Text;

	/// <summary>Will be triggered when the button is pressed</summary>
	public event EventHandler Pressed;

	/// <summary>Called when the button is pressed</summary>
	protected override void OnPressed()
	{
		if (this.Pressed != null)
		{
			this.Pressed(this, EventArgs.Empty);
		}
	}
}
