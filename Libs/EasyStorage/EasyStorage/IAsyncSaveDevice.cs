namespace EasyStorage
{
	public interface IAsyncSaveDevice : ISaveDevice
	{
		bool IsBusy { get; }

		event SaveCompletedEventHandler SaveCompleted;

		event LoadCompletedEventHandler LoadCompleted;

		event DeleteCompletedEventHandler DeleteCompleted;

		event FileExistsCompletedEventHandler FileExistsCompleted;

		event GetFilesCompletedEventHandler GetFilesCompleted;

		void SaveAsync(string containerName, string fileName, FileAction saveAction);

		void SaveAsync(string containerName, string fileName, FileAction saveAction, object userState);

		void LoadAsync(string containerName, string fileName, FileAction loadAction);

		void LoadAsync(string containerName, string fileName, FileAction loadAction, object userState);

		void DeleteAsync(string containerName, string fileName);

		void DeleteAsync(string containerName, string fileName, object userState);

		void FileExistsAsync(string containerName, string fileName);

		void FileExistsAsync(string containerName, string fileName, object userState);

		void GetFilesAsync(string containerName);

		void GetFilesAsync(string containerName, object userState);

		void GetFilesAsync(string containerName, string pattern);

		void GetFilesAsync(string containerName, string pattern, object userState);
	}
}
