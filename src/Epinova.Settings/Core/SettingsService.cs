﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsService.cs" company="none">
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
    using EPiServer;
    using EPiServer.Core;
    using EPiServer.DataAbstraction;
    using EPiServer.Framework.TypeScanner;
    using EPiServer.Logging;
    using EPiServer.Web;
    using EPiServer.Web.Routing;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Class SettingsService.
    /// Implements the <see cref="ISettingsService" />
    /// </summary>
    /// <seealso cref="ISettingsService" />
    public class SettingsService : ISettingsService, IDisposable
    {
        /// <summary>
        /// The global settings root name
        /// </summary>
        public const string GlobalSettingsRootName = "Global Settings Root";

        /// <summary>
        /// The settings root name
        /// </summary>
        public const string SettingsRootName = "Settings Root";

        /// <summary>
        /// The ancestor references loader
        /// </summary>
        private readonly AncestorReferencesLoader ancestorReferencesLoader;

        /// <summary>
        /// The content repository
        /// </summary>
        private readonly IContentRepository contentRepository;

        /// <summary>
        /// The content root service
        /// </summary>
        private readonly ContentRootService contentRootService;

        /// <summary>
        /// The content type repository
        /// </summary>
        private readonly IContentTypeRepository contentTypeRepository;

        /// <summary>
        /// The logger instance
        /// </summary>
        private readonly ILogger log = LogManager.GetLogger();

        /// <summary>
        /// The cache lock
        /// </summary>
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        /// The type scanner lookup
        /// </summary>
        private readonly ITypeScannerLookup typeScannerLookup;

        /// <summary><c>true</c> when this instance is already disposed off; <c>false</c> to when not.</summary>
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        /// <param name="contentRepository">The content repository.</param>
        /// <param name="contentRootService">The content root service.</param>
        /// <param name="typeScannerLookup">The type scanner lookup.</param>
        /// <param name="contentTypeRepository">The content type repository.</param>
        /// <param name="ancestorReferencesLoader">The ancestor references loader.</param>
        public SettingsService(
            IContentRepository contentRepository,
            ContentRootService contentRootService,
            ITypeScannerLookup typeScannerLookup,
            IContentTypeRepository contentTypeRepository,
            AncestorReferencesLoader ancestorReferencesLoader)
        {
            this.contentRepository = contentRepository;
            this.contentRootService = contentRootService;
            this.typeScannerLookup = typeScannerLookup;
            this.contentTypeRepository = contentTypeRepository;
            this.ancestorReferencesLoader = ancestorReferencesLoader;

            GlobalSettings = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Gets the global settings.
        /// </summary>
        /// <value>The global settings.</value>
        public Dictionary<Type, object> GlobalSettings { get; }

        /// <summary>
        /// Gets or sets the global settings root.
        /// </summary>
        /// <value>The global settings root.</value>
        public ContentReference GlobalSettingsRoot { get; set; }

        /// <summary>
        /// Gets or sets the settings root.
        /// </summary>
        /// <value>The settings root.</value>
        public ContentReference SettingsRoot { get; set; }

        /// <summary>
        /// Gets the global settings root unique identifier
        /// </summary>
        private Guid GlobalSettingsRootGuid { get; } = new Guid("98ed413d-d7b5-4fbf-92a6-120d850fe61a");

        /// <summary>
        /// Gets the settings root unique identifier
        /// </summary>
        private Guid SettingsRootGuid { get; } = new Guid("98ed413d-d7b5-4fbf-92a6-120d850fe61c");

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if(disposed)
            {
                return;
            }

            Dispose(true);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <typeparam name="T">The settings type</typeparam>
        /// <returns>An instance of <typeparamref name="T"/> </returns>
        /// <exception cref="T:System.Threading.LockRecursionException">The current thread cannot acquire the write lock when it holds the read lock.-or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire the read lock when it already holds the read lock. -or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire the read lock when it already holds the write lock. -or-The recursion number would exceed the capacity of the counter. This limit is so large that applications should never encounter this exception.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.ReaderWriterLockSlim" /> object has been disposed.</exception>
        /// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in read mode.</exception>
        public T GetSettings<T>()
        {
            readerWriterLock.EnterReadLock();

            try
            {
                if(GlobalSettings.ContainsKey(typeof(T)))
                {
                    return (T)GlobalSettings[typeof(T)];
                }
            }
            catch(KeyNotFoundException keyNotFoundException)
            {
                log.Error($"[Settings] {keyNotFoundException.Message}", exception: keyNotFoundException);
            }
            catch(ArgumentNullException argumentNullException)
            {
                log.Error($"[Settings] {argumentNullException.Message}", exception: argumentNullException);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }

            return default;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <typeparam name="T">The settings type</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>An instance of <typeparamref name="T"/> </returns>
        /// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in read mode.</exception>
        /// <exception cref="T:System.Threading.LockRecursionException">The current thread cannot acquire the write lock when it holds the read lock.-or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire the read lock when it already holds the read lock. -or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire the read lock when it already holds the write lock. -or-The recursion number would exceed the capacity of the counter. This limit is so large that applications should never encounter this exception.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.ReaderWriterLockSlim" /> object has been disposed.</exception>
        public T GetSettings<T>(IContent content)
            where T : IContent
        {
            if(content == null)
            {
                return default;
            }

            T settingsFromContent = TryGetSettingsFromContent<T>(content: content);

            if(settingsFromContent != null)
            {
                return settingsFromContent;
            }

            IEnumerable<ContentReference> ancestors =
                ancestorReferencesLoader.GetAncestors(contentLink: content.ContentLink);

            foreach(ContentReference parentReference in ancestors)
            {

                if(!contentRepository.TryGet(contentLink: parentReference, content: out IContent parentContent))
                {
                    continue;
                }

                settingsFromContent = TryGetSettingsFromContent<T>(content: parentContent);

                if(settingsFromContent != null)
                {
                    return settingsFromContent;
                }
            }

            return GetSettings<T>();
        }

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">If the rootname is already registered with another contentRootId.</exception>
        public void InitSettings()
        {
            try
            {
                contentRootService.Register<ContentFolder>(
                    rootName: GlobalSettingsRootName,
                    contentRootId: GlobalSettingsRootGuid,
                    parent: SiteDefinition.Current.GlobalAssetsRoot);
                GlobalSettingsRoot = contentRootService.Get(rootName: GlobalSettingsRootName);
            }
            catch(NotSupportedException notSupportedException)
            {
                log.Error($"[Settings] {notSupportedException.Message}", exception: notSupportedException);
                throw;
            }

            try
            {
                contentRootService.Register<ContentFolder>(
                    rootName: SettingsRootName,
                    contentRootId: SettingsRootGuid,
                    parent: SiteDefinition.Current.SiteAssetsRoot);
                SettingsRoot = contentRootService.Get(rootName: SettingsRootName);
            }
            catch(NotSupportedException notSupportedException)
            {
                log.Error($"[Settings] {notSupportedException.Message}", exception: notSupportedException);
                throw;
            }

            InitializeContentInstances();
        }

        /// <summary>
        /// Updates the settings.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock in any mode. -or-The current thread has entered read mode, so trying to enter the lock in write mode would create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.ReaderWriterLockSlim" /> object has been disposed.</exception>
        /// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in write mode.</exception>
        public void UpdateSettings(IContent content)
        {
            Type contentType = content.GetOriginalType();

            readerWriterLock.EnterWriteLock();

            try
            {
                if(!GlobalSettings.ContainsKey(key: contentType))
                {
                    return;
                }

                GlobalSettings[key: contentType] = content;
            }
            catch(KeyNotFoundException keyNotFoundException)
            {
                log.Error($"[Settings] {keyNotFoundException.Message}", exception: keyNotFoundException);
            }
            catch(ArgumentNullException argumentNullException)
            {
                log.Error($"[Settings] {argumentNullException.Message}", exception: argumentNullException);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if(disposed)
            {
                return;
            }

            if(readerWriterLock != null)
            {
                readerWriterLock.Dispose();
            }

            disposed = true;

            if(disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Initializes the content instances.
        /// </summary>
        private void InitializeContentInstances()
        {
            Type type = typeof(SettingsContentTypeAttribute);

            IEnumerable<Type> settingsModelTypes = typeScannerLookup.AllTypes.Where(
                t => t.GetCustomAttributes(typeof(SettingsContentTypeAttribute), false).Length > 0);

            List<IContent> existingItems = contentRepository
                .GetChildren<IContent>(contentLink: GlobalSettingsRoot).ToList();

            foreach(Type settingsType in settingsModelTypes)
            {
                SettingsContentTypeAttribute attribute =
                    settingsType.GetCustomAttributes(attributeType: type, false).FirstOrDefault() as
                        SettingsContentTypeAttribute;

                if(attribute == null)
                {
                    continue;
                }

                IContent existingItem = existingItems.FirstOrDefault(
                    i => i.ContentGuid == new Guid(g: attribute.SettingsInstanceGuid));

                if(existingItem == null)
                {
                    ContentType contentType = contentTypeRepository.Load(modelType: settingsType);

                    IContent newSettings = contentRepository.GetDefault<IContent>(
                        parentLink: GlobalSettingsRoot,
                        contentTypeID: contentType.ID);

                    newSettings.Name = attribute.SettingsName;
                    newSettings.ContentGuid = new Guid(g: attribute.SettingsInstanceGuid);

                    contentRepository.Save(
                        content: newSettings,
                        action: EPiServer.DataAccess.SaveAction.Publish,
                        access: EPiServer.Security.AccessLevel.NoAccess);

                    existingItem = newSettings;
                }

                GlobalSettings.Add(existingItem.GetOriginalType(), value: existingItem);
            }
        }

        /// <summary>
        /// Tries to get the settings from content.
        /// </summary>
        /// <typeparam name="T">The settings type</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>An instance of <typeparamref name="T"/> </returns>
        private T TryGetSettingsFromContent<T>(IContent content)
            where T : IContent
        {
            PropertyData property = content.Property[name: typeof(T).Name];

            if(property == null || property.IsNull)
            {
                return default;
            }

            ContentReference reference = property.Value as ContentReference;

            if(reference == null)
            {
                return default;
            }


            contentRepository.TryGet(contentLink: reference, content: out T settingsObject);

            return settingsObject != null ? settingsObject : default;
        }
    }
}