using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;

namespace EasyStorage
{
	/// <summary>
	/// File-system based replacement for the original EasyStorage SaveDevice,
	/// which relied on the XNA StorageDevice/GamerServices APIs that MonoGame
	/// does not provide. Containers map to directories under the user's
	/// application data folder.
	/// </summary>
	public abstract class SaveDevice : IGameComponent, IUpdateable, IAsyncSaveDevice, ISaveDevice
	{
		private static readonly string RootDirectory = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"BeStiff");

		private int pendingOperations;
		private bool enabled = true;
		private int updateOrder;
		private bool isReady;
		private bool deviceSelectedPending;

		public bool IsBusy => pendingOperations > 0;

		public bool IsReady => isReady;

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				if (enabled != value)
				{
					enabled = value;
					EnabledChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public int UpdateOrder
		{
			get { return updateOrder; }
			set
			{
				if (updateOrder != value)
				{
					updateOrder = value;
					UpdateOrderChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public event SaveCompletedEventHandler SaveCompleted;
		public event LoadCompletedEventHandler LoadCompleted;
		public event DeleteCompletedEventHandler DeleteCompleted;
		public event FileExistsCompletedEventHandler FileExistsCompleted;
		public event GetFilesCompletedEventHandler GetFilesCompleted;
		public event EventHandler<EventArgs> DeviceSelected;
		public event EventHandler<SaveDeviceEventArgs> DeviceSelectorCanceled;
		public event EventHandler<SaveDevicePromptEventArgs> DeviceReselectPromptClosed;
		public event EventHandler<SaveDeviceEventArgs> DeviceDisconnected;
		public event EventHandler<EventArgs> EnabledChanged;
		public event EventHandler<EventArgs> UpdateOrderChanged;

		public virtual void Initialize()
		{
		}

		/// <summary>
		/// The original implementation showed an asynchronous device selector.
		/// Here the local disk is always available, but callers subscribe to
		/// DeviceSelected after calling this, so the event is raised from the
		/// next Update instead of synchronously.
		/// </summary>
		public void PromptForDevice()
		{
			Directory.CreateDirectory(RootDirectory);
			isReady = true;
			deviceSelectedPending = true;
		}

		public void Update(GameTime gameTime)
		{
			if (deviceSelectedPending)
			{
				deviceSelectedPending = false;
				DeviceSelected?.Invoke(this, EventArgs.Empty);
			}
		}

		private static string ContainerPath(string containerName)
		{
			string path = Path.Combine(RootDirectory, containerName);
			Directory.CreateDirectory(path);
			return path;
		}

		private static string FilePath(string containerName, string fileName)
		{
			return Path.Combine(ContainerPath(containerName), fileName);
		}

		/// <summary>
		/// Serializes file operations across all devices (they share one root
		/// directory). Without it, overlapping SaveAsync calls on the same file
		/// fail with a sharing violation and the later save is silently lost.
		/// </summary>
		private static readonly object FileLock = new object();

		public void Save(string containerName, string fileName, FileAction saveAction)
		{
			lock (FileLock)
			{
				// Write to a temporary file and swap it in, so a failure halfway
				// through never leaves a truncated save behind.
				string path = FilePath(containerName, fileName);
				string tempPath = path + ".tmp";
				try
				{
					using (FileStream stream = File.Create(tempPath))
					{
						saveAction(stream);
					}
					File.Move(tempPath, path, overwrite: true);
				}
				finally
				{
					if (File.Exists(tempPath))
					{
						File.Delete(tempPath);
					}
				}
			}
		}

		public void Load(string containerName, string fileName, FileAction loadAction)
		{
			lock (FileLock)
			{
				using (FileStream stream = File.OpenRead(FilePath(containerName, fileName)))
				{
					loadAction(stream);
				}
			}
		}

		public void Delete(string containerName, string fileName)
		{
			lock (FileLock)
			{
				string path = FilePath(containerName, fileName);
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
		}

		public bool FileExists(string containerName, string fileName)
		{
			return File.Exists(FilePath(containerName, fileName));
		}

		public string[] GetFiles(string containerName)
		{
			return GetFiles(containerName, "*");
		}

		public string[] GetFiles(string containerName, string pattern)
		{
			string[] files = Directory.GetFiles(ContainerPath(containerName), pattern);
			for (int i = 0; i < files.Length; i++)
			{
				files[i] = Path.GetFileName(files[i]);
			}
			return files;
		}

		private void RunAsync(Action work)
		{
			Interlocked.Increment(ref pendingOperations);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					work();
				}
				finally
				{
					Interlocked.Decrement(ref pendingOperations);
				}
			});
		}

		public void SaveAsync(string containerName, string fileName, FileAction saveAction)
		{
			SaveAsync(containerName, fileName, saveAction, null);
		}

		public void SaveAsync(string containerName, string fileName, FileAction saveAction, object userState)
		{
			RunAsync(() =>
			{
				Exception error = null;
				try { Save(containerName, fileName, saveAction); }
				catch (Exception e) { error = e; }
				SaveCompleted?.Invoke(this, new FileActionCompletedEventArgs(error, userState));
			});
		}

		public void LoadAsync(string containerName, string fileName, FileAction loadAction)
		{
			LoadAsync(containerName, fileName, loadAction, null);
		}

		public void LoadAsync(string containerName, string fileName, FileAction loadAction, object userState)
		{
			RunAsync(() =>
			{
				Exception error = null;
				try { Load(containerName, fileName, loadAction); }
				catch (Exception e) { error = e; }
				LoadCompleted?.Invoke(this, new FileActionCompletedEventArgs(error, userState));
			});
		}

		public void DeleteAsync(string containerName, string fileName)
		{
			DeleteAsync(containerName, fileName, null);
		}

		public void DeleteAsync(string containerName, string fileName, object userState)
		{
			RunAsync(() =>
			{
				Exception error = null;
				try { Delete(containerName, fileName); }
				catch (Exception e) { error = e; }
				DeleteCompleted?.Invoke(this, new FileActionCompletedEventArgs(error, userState));
			});
		}

		public void FileExistsAsync(string containerName, string fileName)
		{
			FileExistsAsync(containerName, fileName, null);
		}

		public void FileExistsAsync(string containerName, string fileName, object userState)
		{
			RunAsync(() =>
			{
				Exception error = null;
				bool result = false;
				try { result = FileExists(containerName, fileName); }
				catch (Exception e) { error = e; }
				FileExistsCompleted?.Invoke(this, new FileExistsCompletedEventArgs(error, result, userState));
			});
		}

		public void GetFilesAsync(string containerName)
		{
			GetFilesAsync(containerName, "*", null);
		}

		public void GetFilesAsync(string containerName, object userState)
		{
			GetFilesAsync(containerName, "*", userState);
		}

		public void GetFilesAsync(string containerName, string pattern)
		{
			GetFilesAsync(containerName, pattern, null);
		}

		public void GetFilesAsync(string containerName, string pattern, object userState)
		{
			RunAsync(() =>
			{
				Exception error = null;
				string[] result = null;
				try { result = GetFiles(containerName, pattern); }
				catch (Exception e) { error = e; }
				GetFilesCompleted?.Invoke(this, new GetFilesCompletedEventArgs(error, result, userState));
			});
		}
	}

	public sealed class SharedSaveDevice : SaveDevice
	{
	}

	public sealed class PlayerSaveDevice : SaveDevice
	{
		public PlayerIndex Player { get; private set; }

		public PlayerSaveDevice(PlayerIndex player)
		{
			Player = player;
		}
	}
}
