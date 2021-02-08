using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreHostingEnvironment : Core.Hosting.IHostingEnvironment
    {
        private readonly ISet<Uri> _applicationUrls = new HashSet<Uri>();
        private readonly IOptionsMonitor<HostingSettings> _hostingSettings;
        private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private string _localTempPath;

        public AspNetCoreHostingEnvironment(
            IOptionsMonitor<HostingSettings> hostingSettings,
            IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
            IWebHostEnvironment webHostEnvironment)
        {
            _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
            _webRoutingSettings = webRoutingSettings ?? throw new ArgumentNullException(nameof(webRoutingSettings));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));

            SiteName = webHostEnvironment.ApplicationName;
            ApplicationId = AppDomain.CurrentDomain.Id.ToString();
            ApplicationPhysicalPath = webHostEnvironment.ContentRootPath;
        }

        /// <inheritdoc/>
        public bool IsHosted { get; } = true;

        /// <inheritdoc/>
        public Uri ApplicationMainUrl { get; private set; }

        /// <inheritdoc/>
        public string SiteName { get; }

        /// <inheritdoc/>
        public string ApplicationId { get; }

        /// <inheritdoc/>
        public string ApplicationPhysicalPath { get; }

        // TODO how to find this, This is a server thing, not application thing.
        public string ApplicationVirtualPath => _hostingSettings.CurrentValue.ApplicationVirtualPath?.EnsureStartsWith('/') ?? "/";

        /// <inheritdoc/>
        public bool IsDebugMode => _hostingSettings.CurrentValue.Debug;

        public Version IISVersion { get; }

        public string LocalTempPath
        {
            get
            {
                if (_localTempPath != null)
                {
                    return _localTempPath;
                }

                switch (_hostingSettings.CurrentValue.LocalTempStorageLocation)
                {
                    case LocalTempStorage.EnvironmentTemp:

                        // environment temp is unique, we need a folder per site

                        // use a hash
                        // combine site name and application id
                        // site name is a Guid on Cloud
                        // application id is eg /LM/W3SVC/123456/ROOT
                        // the combination is unique on one server
                        // and, if a site moves from worker A to B and then back to A...
                        // hopefully it gets a new Guid or new application id?
                        string hashString = SiteName + "::" + ApplicationId;
                        string hash = hashString.GenerateHash();
                        string siteTemp = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData", hash);

                        return _localTempPath = siteTemp;

                    default:

                        return _localTempPath = MapPathContentRoot(Core.Constants.SystemDirectories.TempData);
                }
            }
        }

        /// <inheritdoc/>
        public string MapPathWebRoot(string path) => MapPath(_webHostEnvironment.WebRootPath, path);

        /// <inheritdoc/>
        public string MapPathContentRoot(string path) => MapPath(_webHostEnvironment.ContentRootPath, path);

        private string MapPath(string root, string path)
        {
            var newPath = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            // TODO: This is a temporary error because we switched from IOHelper.MapPath to HostingEnvironment.MapPathXXX
            // IOHelper would check if the path passed in started with the root, and not prepend the root again if it did,
            // however if you are requesting a path be mapped, it should always assume the path is relative to the root, not
            // absolute in the file system.  This error will help us find and fix improper uses, and should be removed once
            // all those uses have been found and fixed
            if (newPath.StartsWith(root))
            {
                throw new ArgumentException("The path appears to already be fully qualified.  Please remove the call to MapPath");
            }

            return Path.Combine(root, newPath.TrimStart('~', '/', '\\'));
        }

        /// <inheritdoc/>
        public string ToAbsolute(string virtualPath)
        {
            if (!virtualPath.StartsWith("~/") && !virtualPath.StartsWith("/"))
            {
                throw new InvalidOperationException($"The value {virtualPath} for parameter {nameof(virtualPath)} must start with ~/ or /");
            }

            // will occur if it starts with "/"
            if (Uri.IsWellFormedUriString(virtualPath, UriKind.Absolute))
            {
                return virtualPath;
            }

            string fullPath = ApplicationVirtualPath.EnsureEndsWith('/') + virtualPath.TrimStart('~', '/');

            return fullPath;
        }

        public void EnsureApplicationMainUrl(Uri currentApplicationUrl)
        {
            // Fixme: This causes problems with site swap on azure because azure pre-warms a site by calling into `localhost` and when it does that
            // it changes the URL to `localhost:80` which actually doesn't work for pinging itself, it only works internally in Azure. The ironic part
            // about this is that this is here specifically for the slot swap scenario https://issues.umbraco.org/issue/U4-10626

            // see U4-10626 - in some cases we want to reset the application url
            // (this is a simplified version of what was in 7.x)
            // note: should this be optional? is it expensive?

            if (currentApplicationUrl == null)
            {
                return;
            }

            if (!(_webRoutingSettings.CurrentValue.UmbracoApplicationUrl is null))
            {
                return;
            }

            var change = currentApplicationUrl != null && !_applicationUrls.Contains(currentApplicationUrl);
            if (change)
            {
                _applicationUrls.Add(currentApplicationUrl);

                ApplicationMainUrl = currentApplicationUrl;
            }
        }
    }


}
