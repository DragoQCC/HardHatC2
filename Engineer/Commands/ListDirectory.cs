using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
	public class ListDirectory : EngineerCommand
	{
		public override string Name => "ls" ;
	
		public override async Task Execute(EngineerTask task)
		{
			if (!task.Arguments.TryGetValue("/path", out string path)) // if it fails to get a value from this argument then it will use the current path
			{
				path = Directory.GetCurrentDirectory();
            }
            try
            {
                string output = GetDirectoryListing(path).ToString();
                Tasking.FillTaskResults(output, task,EngTaskStatus.Complete);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults("error: " + ex.Message,task,EngTaskStatus.Failed);
            }
        }
		public static SharpSploitResultList<FileSystemEntryResult> GetDirectoryListing(string Path)
		{
            
			SharpSploitResultList<FileSystemEntryResult> results = new SharpSploitResultList<FileSystemEntryResult>();
			foreach (string dir in Directory.GetDirectories(Path))
			{
				DirectoryInfo dirInfo = new DirectoryInfo(dir);
				results.Add(new FileSystemEntryResult
				{
					Name = dirInfo.FullName,
					Length = 0,
					CreationTimeUtc = dirInfo.CreationTimeUtc,
					LastAccessTimeUtc = dirInfo.LastAccessTimeUtc,
					LastWriteTimeUtc = dirInfo.LastWriteTimeUtc
				});
			}
			foreach (string file in Directory.GetFiles(Path))
			{
				FileInfo fileInfo = new FileInfo(file);
				results.Add(new FileSystemEntryResult
				{
					Name = fileInfo.FullName,
					Length = fileInfo.Length,
					CreationTimeUtc = fileInfo.CreationTimeUtc,
					LastAccessTimeUtc = fileInfo.LastAccessTimeUtc,
					LastWriteTimeUtc = fileInfo.LastWriteTimeUtc
				});
			}
			return results;
		}
	}

	public sealed class FileSystemEntryResult : SharpSploitResult
	{
		public string Name { get; set; } = "";
		public long Length { get; set; } = 0;
		public DateTime CreationTimeUtc { get; set; } = new DateTime();
		public DateTime LastAccessTimeUtc { get; set; } = new DateTime();
		public DateTime LastWriteTimeUtc { get; set; } = new DateTime();
		protected internal override IList<SharpSploitResultProperty> ResultProperties
		{
			get
			{
				return new List<SharpSploitResultProperty>
					{
						new SharpSploitResultProperty
						{
							Name = "Name",
							Value = this.Name
						},
						new SharpSploitResultProperty
						{
							Name = "Length",
							Value = this.Length
						},
						new SharpSploitResultProperty
						{
							Name = "CreationTimeUtc",
							Value = this.CreationTimeUtc
						},
						new SharpSploitResultProperty
						{
							Name = "LastAccessTimeUtc",
							Value = this.LastAccessTimeUtc
						},
						new SharpSploitResultProperty
						{
							Name = "LastWriteTimeUtc",
							Value = this.LastWriteTimeUtc
						}
					};
			}
		}
	}
}
