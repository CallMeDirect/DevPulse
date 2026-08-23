using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DevPulse.ViewModels;

namespace DevPulse;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    #region Public methods

    /// <summary>Creates the view whose name corresponds to the supplied view-model type.</summary>
    /// <param name="param">The view model for which a view should be created.</param>
    /// <returns>The matching view, a fallback message, or <see langword="null"/> for null input.</returns>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        
        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>Determines whether this locator can build a view for the supplied object.</summary>
    /// <param name="data">The candidate data object.</param>
    /// <returns><see langword="true"/> for DevPulse view models; otherwise, <see langword="false"/>.</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    #endregion
}
