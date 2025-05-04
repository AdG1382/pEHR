namespace EHRp.ViewModels
{
    /// <summary>
    /// Interface for view models that need to be aware of navigation
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Called when the view model is navigated to
        /// </summary>
        /// <param name="parameter">The parameter passed during navigation</param>
        void OnNavigatedTo(object? parameter);
        
        /// <summary>
        /// Called when the view model is navigated from
        /// </summary>
        void OnNavigatedFrom();
    }
}