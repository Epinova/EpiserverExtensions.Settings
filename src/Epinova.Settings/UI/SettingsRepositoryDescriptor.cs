﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsRepositoryDescriptor.cs" company="none">
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

namespace Epinova.Settings.UI
{
    using Epinova.Settings.Core;
    using EPiServer.Cms.Shell.UI.CompositeViews.Internal;
    using EPiServer.Core;
    using EPiServer.Framework.Localization;
    using EPiServer.ServiceLocation;
    using EPiServer.Shell;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class SettingsRepositoryDescriptor.
    /// Implements the <see cref="EPiServer.Shell.ContentRepositoryDescriptorBase" />
    /// </summary>
    /// <seealso cref="EPiServer.Shell.ContentRepositoryDescriptorBase" />
    [ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
    public class SettingsRepositoryDescriptor : ContentRepositoryDescriptorBase
    {
        /// <summary>
        /// Gets the repository key.
        /// </summary>
        /// <value>The repository key.</value>
        public static string RepositoryKey => "dynamiccontent";

        /// <summary>
        /// Gets the contained types.
        /// </summary>
        /// <value>The contained types.</value>
        public override IEnumerable<Type> ContainedTypes => new[] { typeof(ContentFolder), typeof(SettingsBase) };

        /// <summary>
        /// Gets the creatable types.
        /// </summary>
        /// <value>The creatable types.</value>
        public override IEnumerable<Type> CreatableTypes => new[] { typeof(SettingsBase) };

        /// <summary>
        /// Gets the custom navigation widget.
        /// </summary>
        /// <value>The custom navigation widget.</value>
        public override string CustomNavigationWidget => "epi-cms/component/PageNavigationTree";

        /// <summary>
        /// Gets the custom select title.
        /// </summary>
        /// <value>The custom select title.</value>
        public override string CustomSelectTitle => LocalizationService.Current.GetString("/contentrepositories/settings/customselecttitle");

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public override string Key => RepositoryKey;

        /// <summary>
        /// Gets the main navigation types.
        /// </summary>
        /// <value>The main navigation types.</value>
        public override IEnumerable<Type> MainNavigationTypes => new[] { typeof(ContentFolder) };

        /// <summary>
        /// Gets the main views.
        /// </summary>
        /// <value>The main views.</value>
        public override IEnumerable<string> MainViews => new[] { HomeView.ViewName };

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name => LocalizationService.Current.GetString("/contentrepositories/settings/name");

        /// <summary>
        /// Gets the roots.
        /// </summary>
        /// <value>The roots.</value>
        public override IEnumerable<ContentReference> Roots => new[] { Settings.Service.SettingsRoot };

        /// <summary>
        /// Gets the search area.
        /// </summary>
        /// <value>The search area.</value>
        public override string SearchArea => SettingsSearchProvider.SearchArea;

        /// <summary>
        /// Gets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public override int SortOrder => 1100;

        private Injected<ISettingsService> Settings { get; set; }
    }
}