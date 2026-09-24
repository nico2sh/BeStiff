namespace Nuclex.UserInterface.Controls;

/// <summary>Control that draws a block of text</summary>
public class LabelControl : Control
{
	/// <summary>Text to be rendered in the control's frame</summary>
	public string Text;

	/// <summary>Initializes a new label control with an empty string</summary>
	public LabelControl()
		: this(string.Empty)
	{
	}

	/// <summary>Initializes a new label control</summary>
	/// <param name="text">Text to be printed at the location of the label control</param>
	public LabelControl(string text)
	{
		Text = text;
	}
}
