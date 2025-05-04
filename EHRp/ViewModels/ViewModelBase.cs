﻿﻿﻿﻿﻿﻿using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using EHRp.Models;

namespace EHRp.ViewModels
{
    /// <summary>
    /// Base class for all view models in the application.
    /// </summary>
    public class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Gets the UI thread dispatcher.
        /// </summary>
        protected Dispatcher Dispatcher => Dispatcher.UIThread;
        
        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        public User? CurrentUser { get; set; }
        
        /// <summary>
        /// Performs cleanup when the view model is unloaded.
        /// </summary>
        public virtual void Cleanup()
        {
            // Base implementation does nothing
        }
        
        /// <summary>
        /// Executes an action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        protected void ExecuteOnUIThread(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.Post(action);
            }
        }
        
        /// <summary>
        /// Executes an action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected Task ExecuteOnUIThreadAsync(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }
            else
            {
                var operation = Dispatcher.InvokeAsync(action);
                return operation.GetTask();
            }
        }
    }
}
