// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Model
{
    /// <summary>
    /// Command of the interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets name of the command in the interface.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets display name of the command.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute" /> class.
        /// </summary>
        /// <param name="name">Optional name of the command in the interface. If not provided method name will be used.</param>
        /// <param name="displayName">Optional name of the command in the interface.</param>
        public CommandAttribute(string? name, string? displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute" /> class.
        /// </summary>
        /// <param name="name">Optional name of the command in the interface. If not provided method name will be used.</param>
        public CommandAttribute(string? name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute" /> class.
        /// </summary>
        public CommandAttribute()
        {
        }
    }
}
