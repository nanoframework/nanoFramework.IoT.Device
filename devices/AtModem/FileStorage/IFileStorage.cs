// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.AtModem.FileStorage;

namespace Iot.Device.AtModem
{
    /// <summary>
    /// Interface for use internal flash storage space.
    /// </summary>
    public interface IFileStorage : IDisposable
    {
        /// <summary>
        /// Deletes a file from the file storage.
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <returns>True if the file was successfully deleted. False otherwise.</returns>
        public bool DeleteFile(string fileName);

        /// <summary>
        /// Writes content to a storage file.
        /// Content must be plain string without \r \n chars.
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <param name="content">File content.</param>
        /// <param name="createMode">File creation mode.</param>
        /// <returns>True if file was successfully written. False otherwise.</returns>
        public bool WriteFile(string fileName, string content, CreateMode createMode = CreateMode.Override);

        /// <summary>
        /// Writes content to a storage file.
        /// Content must be plain string without \r \n chars.
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <param name="content">File content.</param>
        /// <param name="createMode">File creation mode.</param>
        /// <returns>True if file was successfully written. False otherwise.</returns>
        public bool WriteFile(string fileName, byte[] content, CreateMode createMode = CreateMode.Override);

        /// <summary>
        /// Returns available storage space.
        /// </summary>
        /// <returns>File available storage space in bytes or -1 if size couldn't be determined.</returns>
        public int GetAvailableStorage();

        /// <summary>
        /// Returns the file size in bytes.
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <returns>File size in bytes or -1 if size couldn't be read or doesn't exist.</returns>
        public int GetFileSize(string fileName);

        /// <summary>
        /// Read a file from the file storage.
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <param name="position">Position to read from.</param>
        /// <returns>File contents or NULL if file is empty or doesn't exist.</returns>
        public string ReadFile(string fileName, int position = 0);

        /// <summary>
        /// Read a file from the file storage.
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <param name="content">File contents as a span of bytes.</param>
        /// <param name="position">Position to read from.</param>
        /// <returns>File contents or NULL if file is empty or doesn't exist.</returns>
        public bool ReadFile(string fileName, ref byte[] content, int position = 0);

        /// <summary>
        /// Renames a file in the file storage.
        /// </summary>
        /// <param name="oldFileName">The old file name.</param>
        /// <param name="newFileName">The new file name.</param>
        /// <returns>True if success.</returns>
        public bool RenameFile(string oldFileName, string newFileName);

        /// <summary>
        /// Delete a directory from the file storage.
        /// </summary>
        /// <param name="directoryName">The name of the directory.</param>
        /// <returns>True if success.</returns>
        public bool DeleteDirectory(string directoryName);

        /// <summary>
        /// Create a directory in the file storage.
        /// </summary>
        /// <param name="directoryName">The name of the directory.</param>
        /// <returns>True if success.</returns>
        public bool CreateDirectory(string directoryName);

        /// <summary>
        /// List the files in a directory.
        /// </summary>
        /// <param name="directoryName">The name of the directory.</param>
        /// <returns>The list of files and directories.</returns>
        public string[] ListDirectory(string directoryName);

        /// <summary>
        /// Gets a value indicating whether the file storage supports directories.
        /// </summary>
        public bool HasDirectorySupport { get; }
    }
}
