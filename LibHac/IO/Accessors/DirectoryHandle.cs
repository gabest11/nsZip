﻿using System;

namespace LibHac.IO.Accessors
{
    public struct DirectoryHandle : IDisposable
    {
        internal readonly DirectoryAccessor Directory;

        internal DirectoryHandle(DirectoryAccessor directory)
        {
            Directory = directory;
        }

        public int GetId() => Directory.GetHashCode();

        public void Dispose()
        {
            Directory.Parent.FsManager.CloseDirectory(this);
        }
    }
}
