namespace EasyStorage
{
	public interface ISaveDevice
	{
		bool IsReady { get; }

		void Save(string containerName, string fileName, FileAction saveAction);

		void Load(string containerName, string fileName, FileAction loadAction);

		void Delete(string containerName, string fileName);

		bool FileExists(string containerName, string fileName);

		string[] GetFiles(string containerName);

		string[] GetFiles(string containerName, string pattern);
	}
}
