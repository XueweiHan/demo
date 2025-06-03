// ---------------------------------------------------------------------------
// <copyright file="FunctionRunnerBuilderExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------
#nullable enable
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

// If your project is using package Microsoft.Azure.Functions.Extensions (in-process mode)
// copy this file to your project and update the namespace
namespace Copy_This_File_To_Your_Project_And_Update_The_Namespace_To_Your_Project_Namespace;

/// <summary>
/// Extensions for the FunctionRunnerBuilder.
/// </summary>
static class FunctionRunnerBuilderExtensions
{
    /// <summary>
    /// Get the context from the builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The FunctionsHostBuilderContext.</returns>
    public static FunctionsHostBuilderContext GetContext(this IFunctionsHostBuilder builder)
    {
        return FunctionRunnerHostBuilderContext ?? FunctionsBuilderExtensions.GetContext(builder);
    }

    /// <summary>
    /// Get the context from the builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The FunctionsHostBuilderContext.</returns>
    public static FunctionsHostBuilderContext GetContext(this IFunctionsConfigurationBuilder builder)
    {
        return FunctionRunnerHostBuilderContext ?? FunctionsBuilderExtensions.GetContext(builder);
    }

    static FunctionsHostBuilderContext? FunctionRunnerHostBuilderContext { get; set; }
}
