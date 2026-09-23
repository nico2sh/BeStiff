using System;

namespace EasyStorage
{
	public struct GetFilesCompletedEventArgs
	{
		public Exception Error { get; private set; }

		public object UserState { get; private set; }

		public string[] Result { get; private set; }

		public GetFilesCompletedEventArgs(Exception error, string[] result, object userState)
		{
			this = default(GetFilesCompletedEventArgs);
			Error = error;
			Result = result;
			UserState = userState;
		}
	}
}
