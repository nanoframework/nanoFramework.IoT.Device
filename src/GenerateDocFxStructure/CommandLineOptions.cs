// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CommandLine;
using System.Collections.Generic;

namespace GenerateDocFxStructure
{
    /// <summary>
    /// Class for command line options.
    /// </summary>
    public class CommandlineOptions
    {
        /// <summary>
        /// Gets or sets the destination folder.
        /// </summary>
        [Option('d', "destination", Required = true, HelpText = "Folder containing the destination folder.")]
        public string DestinationFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source folder.
        /// </summary>
        [Option('s', "source", Required = true, HelpText = "Folder containing the source folder.")]
        public string SourceFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination media folder.
        /// </summary>
        [Option('m', "media", Required = true, HelpText = "Folder containing the destination media folder.")]
        public string MediaFolder { get; set; } = string.Empty;

        [Option('i', "ignore", Required = false, Separator = ',', HelpText = "Folder to ignore in building the docs, separate with ','.")]
        public IEnumerable<string> IgnoreFolders { get; set; }

        /// <summary>
        /// Gets or sets the main repository.
        /// </summary>
        [Option('r', "repo", Required = false, HelpText = "The main repository with the branch to target pointing on the folder used as a source. Default value is 'https://github.com/nanoFramework/nanoFramework.IoT.Device/tree/develop/devices'.")]
        public string Repo { get; set; } = "https://github.com/nanoFramework/nanoFramework.IoT.Device/tree/develop/devices";

        /// <summary>
        /// Gets or sets a value indicating whether verbose information is shown in the output.
        /// </summary>
        [Option('v', "verbose", Required = false, HelpText = "Show verbose messages.")]
        public bool Verbose { get; set; }
    }
}
