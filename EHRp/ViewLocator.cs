using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using EHRp.ViewModels;

namespace EHRp;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        
        var viewModelName = param.GetType().Name;
        var viewModelNamespace = param.GetType().Namespace;
        
        if (viewModelName.EndsWith("ViewModel"))
        {
            // Extract the base name (without "ViewModel")
            var baseName = viewModelName.Substring(0, viewModelName.Length - "ViewModel".Length);
            
            // Try to find the view in different locations
            Type? viewType = null;
            
            // First try: Same namespace structure but with .Views instead of .ViewModels
            if (viewModelNamespace != null)
            {
                var viewNamespace = viewModelNamespace.Replace("ViewModels", "Views");
                var fullViewName = $"{viewNamespace}.{baseName}View";
                viewType = Type.GetType(fullViewName);
            }
            
            // Second try: Look in specific folders based on the view name
            if (viewType == null)
            {
                var possibleNamespaces = new[]
                {
                    $"EHRp.Views.{baseName}",
                    "EHRp.Views"
                };
                
                foreach (var ns in possibleNamespaces)
                {
                    var fullViewName = $"{ns}.{baseName}View";
                    viewType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.FullName == fullViewName);
                    
                    if (viewType != null)
                        break;
                }
            }
            
            // If we found a view type, create an instance
            if (viewType != null)
            {
                return (Control)Activator.CreateInstance(viewType)!;
            }
        }
        
        // If we couldn't find a matching view, show a message
        return new TextBlock { Text = $"View not found for: {param.GetType().FullName}" };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
