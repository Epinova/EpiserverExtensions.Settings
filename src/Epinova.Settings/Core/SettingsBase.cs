﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsBase.cs" company="none">
//      Copyright © 2019 Linus Ekström, Jeroen Stemerdink.
//      Permission is hereby granted, free of charge, to any person obtaining a copy
//      of this software and associated documentation files (the "Software"), to deal
//      in the Software without restriction, including without limitation the rights
//      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//      copies of the Software, and to permit persons to whom the Software is
//      furnished to do so, subject to the following conditions:
// 
//      The above copyright notice and this permission notice shall be included in all
//      copies or substantial portions of the Software.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//      SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Epinova.Settings.Core
{
    using EPiServer.Core;
    using System;

    /// <summary>
    /// Class SettingsBase.
    /// Implements the <see cref="EPiServer.Core.BasicContent" />
    /// Implements the <see cref="EPiServer.Core.IVersionable" />
    /// </summary>
    /// <seealso cref="EPiServer.Core.BasicContent" />
    /// <seealso cref="EPiServer.Core.IVersionable" />
    public class SettingsBase : BasicContent, IVersionable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this item is in pending publish state.
        /// </summary>
        /// <value><c>true</c> if this instance is in pending publish state; otherwise, <c>false</c>.</value>
        public bool IsPendingPublish { get; set; }

        /// <summary>
        /// Gets or sets the start publish date for this item.
        /// </summary>
        /// <value>The start publish.</value>
        public DateTime? StartPublish { get; set; }

        /// <summary>
        /// Gets or sets the version status of this item.
        /// </summary>
        /// <value>The status.</value>
        public VersionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the stop publish date for this item.
        /// </summary>
        /// <value>The stop publish.</value>
        public DateTime? StopPublish { get; set; }
    }
}