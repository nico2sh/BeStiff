namespace EasyStorage
{
	internal enum SaveDevicePromptState
	{
		None,
		ShowSelector,
		PromptForCanceled,
		ForceCanceledReselection,
		PromptForDisconnected,
		ForceDisconnectedReselection
	}
}
