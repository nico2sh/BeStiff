using System.Text;

namespace Nuclex.Support.Collections;

/// <summary>An pair of a priority and an item</summary>
public struct PriorityItemPair<PriorityType, ItemType>
{
	/// <summary>Priority assigned to this priority / item pair</summary>
	public PriorityType Priority;

	/// <summary>Item contained in this priority / item pair</summary>
	public ItemType Item;

	/// <summary>Initializes a new priority / item pair</summary>
	/// <param name="priority">Priority of the item in the pair</param>
	/// <param name="item">Item to be stored in the pair</param>
	public PriorityItemPair(PriorityType priority, ItemType item)
	{
		Priority = priority;
		Item = item;
	}

	/// <summary>Converts the priority / item pair into a string</summary>
	/// <returns>A string describing the priority / item pair</returns>
	public override string ToString()
	{
		int num = 4;
		string text = Priority.ToString();
		if (text != null)
		{
			num += text.Length;
		}
		else
		{
			text = string.Empty;
		}
		string text2 = Item.ToString();
		if (text2 != null)
		{
			num += text2.Length;
		}
		else
		{
			text2 = string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		stringBuilder.Append('[');
		stringBuilder.Append(text);
		stringBuilder.Append(", ");
		stringBuilder.Append(text2);
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}
}
