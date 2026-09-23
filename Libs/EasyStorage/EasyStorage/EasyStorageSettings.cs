namespace EasyStorage
{
	/// <summary>
	/// Kept for API compatibility with the original EasyStorage library.
	/// The MonoGame port never shows a storage device selector, so language
	/// settings have no effect.
	/// </summary>
	public static class EasyStorageSettings
	{
		public static void SetSupportedLanguages(params Language[] supportedLanguages)
		{
		}

		public static void ResetSaveDeviceStrings()
		{
		}
	}
}
