// ---------------------------------------------------------------------------
// <copyright file="FunctionRunnerHostExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------
using Microsoft.Extensions.Hosting;

// If your project is using package Microsoft.Azure.Functions.Worker (isolated mode)
// copy this file to your project and update the namespace
namespace Copy_This_File_To_Your_Project_And_Update_The_Namespace_To_Your_Project_Namespace;

/// <summary>
/// Provides extension methods for running an <see cref="IHost"/> in the context of Azure Functions.
/// </summary>
static class FunctionRunnerHostExtensions
{
    /// <summary>
    /// Runs the specified <see cref="IHost"/> instance.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> instance to run.</param>
    public static void Run(this IHost host)
    {
        (FunctionRunnerHostBuilder != null ? (Action<IHost>)(h => { }) : HostingAbstractionsHostExtensions.Run)(host);
    }

    /// <summary>
    /// Configures the worker to use the ASP.NET Core integration, enabling advanced HTTP features.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
    public static IHostBuilder ConfigureFunctionsWebApplication(this IHostBuilder builder)
    {
        return FunctionRunnerHostBuilder ?? FunctionsHostBuilderExtensions.ConfigureFunctionsWebApplication(builder);
    }

    static IHostBuilder? FunctionRunnerHostBuilder { get; set; }
}
