// <copyright file="IsolatedLoadContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Represents an isolated assembly load context for loading plugins or assemblies with specific dependencies.
/// </summary>
class IsolatedLoadContext(string pluginPath, ILoggerFactory loggerFactory) : AssemblyLoadContext
{
    static readonly HashSet<string> SharedAssemblies = new()
    {
        // For in-process mode
        "Microsoft.Azure.Functions.Extensions",
        "Microsoft.Extensions.Configuration.EnvironmentVariables",

        // For isolated mode
        "Microsoft.Azure.Functions.Worker.Extensions.ServiceBus",
        "Microsoft.Extensions.Configuration.Abstractions",
        "Microsoft.Extensions.Configuration.Json",
        "Microsoft.Extensions.DependencyInjection.Abstractions",
        "Microsoft.Extensions.Hosting.Abstractions",
        "Microsoft.Extensions.Logging.Abstractions",
    };

    readonly AssemblyDependencyResolver resolver = new(pluginPath);

    #pragma warning disable CA2213 // Disposable fields should be disposed
    readonly ILoggerFactory loggerFactory = loggerFactory;  // loggerFactory is a singleton, so it doesn't need to be disposed
    #pragma warning restore CA2213

    /// <summary>
    /// Loads an assembly into the isolated context.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to load.</param>
    /// <returns>The loaded assembly, or null if the assembly is shared or cannot be resolved.</returns>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (SharedAssemblies.Contains(assemblyName.Name!))
        {
            return null;
        }

        string? assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            loggerFactory.CreateLogger("T.Green0").LogDebug($"{assemblyName.Name}:{assemblyName.Version} Loading assembly from path {assemblyPath}");
            return LoadFromAssemblyPath(assemblyPath);
        }

        loggerFactory.CreateLogger("T.Red0").LogDebug($"{assemblyName.Name}:{assemblyName.Version} Could not resolve assembly to a path");
        return null;
    }

    /// <summary>
    /// Loads an unmanaged DLL into the isolated context.
    /// </summary>
    /// <param name="unmanagedDllName">The name of the unmanaged DLL to load.</param>
    /// <returns>A pointer to the loaded DLL, or IntPtr.Zero if the DLL cannot be resolved.</returns>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Loads an assembly from the specified path using an isolated load context.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly to load.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <returns>The loaded assembly.</returns>
    public static Assembly LoadAssembly(string assemblyPath, ILoggerFactory loggerFactory)
    {
        return loadedAssemblies.GetOrAdd(assemblyPath, path =>
        {
            var loadContext = new IsolatedLoadContext(path, loggerFactory);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
        });
    }

    /// <summary>
    /// A dictionary to cache loaded assemblies by their paths.
    /// </summary>
    static readonly ConcurrentDictionary<string, Assembly> loadedAssemblies = new();
}
