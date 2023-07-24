// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Interface for use internal flash storage space.
    /// </summary>
    public interface IFileStorage
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
        /// <returns>True if file was successfully written. False otherwise.</returns>
        public bool WriteFile(string fileName, string content);

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
        /// <returns>File contents or NULL if file is empty or doesn't exist.</returns>
        public string ReadFile(string fileName);

        /// <summary>
        /// Renames a file in the file storage.
        /// </summary>
        /// <param name="oldFileName">The old file name.</param>
        /// <param name="newFileName">The new file name.</param>
        /// <returns>True if success.</returns>
        public bool RenameFile(string oldFileName, string newFileName);
    }
}
