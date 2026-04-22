using System;

namespace EasySave.Model.Storage
{
	/// <summary>
	/// The metadata of a file used by the storage
	/// </summary>
	public class FileMetadata
	{
		public bool Exists { get; }
		public long Size { get; }
		public DateTime LastModified { get; }

		public FileMetadata(bool exists, long size, DateTime lastModified)
		{
			Exists = exists;
			Size = size;
			LastModified = lastModified;
		}
	}
}
