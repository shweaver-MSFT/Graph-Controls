﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Basic configuration for talking to the Graph.
    /// </summary>
    public class WindowsConfig : IGraphConfig
    {
        /// <summary>
        /// Gets or sets the Client ID (the unique application (client) ID assigned to your app by Azure AD when the app was registered).
        /// </summary>
        /// <remarks>
        /// For details about how to register an app and get a client ID,
        /// see the <a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app">Register an app quick start</a>.
        /// </remarks>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the list of Scopes (permissions) to request on initial login.
        /// </summary>
        /// <remarks>
        /// This list can be modified by controls which require specific scopes to function.
        /// This will aid in requesting all scopes required by controls used before login is initiated, if using the LoginButton.
        /// </remarks>
        public ScopeSet Scopes { get; set; } = new ScopeSet { "User.Read", "User.ReadBasic.All" };
    }
}