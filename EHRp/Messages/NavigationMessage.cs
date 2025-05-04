using CommunityToolkit.Mvvm.Messaging.Messages;
using EHRp.ViewModels;

namespace EHRp.Messages
{
    /// <summary>
    /// Message sent when navigation occurs
    /// </summary>
    public class NavigationMessage : ValueChangedMessage<ViewModelBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationMessage"/> class
        /// </summary>
        /// <param name="viewModel">The view model to navigate to</param>
        public NavigationMessage(ViewModelBase viewModel) : base(viewModel)
        {
        }
    }
}