// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using System;

namespace Microsoft.AspNetCore.SpaServices.VueCli
{
    /// <summary>
    /// Extension methods for enabling Vue cli middleware support.
    /// </summary>
    public static class VueCliMiddlewareExtensions
    {
        /// <summary>
        /// Handles requests by passing them through to an instance of the vue-cli-service server.
        /// This means you can always serve up-to-date CLI-built resources without having
        /// to run the vue-cli-service server manually.
        ///
        /// This feature should only be used in development. For production deployments, be
        /// sure not to enable the vue-cli-service server.
        /// </summary>
        /// <param name="spaBuilder">The <see cref="ISpaBuilder"/>.</param>
        /// <param name="npmScript">The name of the script in your package.json file that launches the vue-cli-service server.</param>
        public static void UseVueCli(
            this ISpaBuilder spaBuilder,
            string npmScript)
        {
            if (spaBuilder == null)
            {
                throw new ArgumentNullException(nameof(spaBuilder));
            }

            var spaOptions = spaBuilder.Options;

            if (string.IsNullOrEmpty(spaOptions.SourcePath))
            {
                throw new InvalidOperationException($"To use {nameof(UseVueCli)}, you must supply a non-empty value for the {nameof(SpaOptions.SourcePath)} property of {nameof(SpaOptions)} when calling {nameof(SpaApplicationBuilderExtensions.UseSpa)}.");
            }

            VueCliMiddleware.Attach(spaBuilder, npmScript);
        }
    }
}
