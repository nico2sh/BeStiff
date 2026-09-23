using System;

namespace EasyStorage
{
	public sealed class SaveDevicePromptEventArgs : EventArgs
	{
		public bool ShowDeviceSelector { get; internal set; }
	}
}
