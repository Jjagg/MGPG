using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace MGPG.IdeTemplateWriters
{
    public class VfsDirectory : VfsFileSystemEntry
    {
        public List<VfsFileSystemEntry> Entries { get; }

        public VfsDirectory()
        {
            Entries = new List<VfsFileSystemEntry>();
        }

        public void Add(VfsFile file)
        {
            Debug.Assert(file.FilePath.Substring(0, FilePath.Length) == FilePath);
            var subPath = file.FilePath.Substring(FilePath.Length);
            var separatorIndex = subPath.IndexOfAny(new[] { '/', '\\' });
            if (separatorIndex == -1)
                Entries.Add(file);
            else
            {
                var nextDir = subPath.Substring(0, separatorIndex + 1);
                var subDir = (VfsDirectory) Entries.FirstOrDefault(e => e.Name == nextDir);
                if (subDir == null)
                {
                    subDir = new VfsDirectory();
                    subDir.FilePath = Path.Combine(FilePath, nextDir);
                    Entries.Add(subDir);
                }
                subDir.Add(file);
            }
        }

        public override void Write(string root, Action<string, FileEntry> writeFileEntry)
        {
            Directory.CreateDirectory(Path.Combine(root, FilePath));
            foreach (var e in Entries)
                e.Write(root, writeFileEntry);
        }
    }

    public class VfsFile : VfsFileSystemEntry
    {
        public FileEntry FileEntry { get; }

        public VfsFile(FileEntry fe)
        {
            FileEntry = fe;
        }

        public override void Write(string root, Action<string, FileEntry> writeFileEntry)
        {
            writeFileEntry(Path.Combine(root, FilePath), FileEntry);
        }
    }

    public abstract class VfsFileSystemEntry
    {
        public string FilePath { get; set; }
        public VfsDirectory Parent { get; set; }

        public bool HasParent => Parent != null;
        public string Name => Path.GetFileName(FilePath);

        public abstract void Write(string root, Action<string, FileEntry> writeFileEntry);
    }
}