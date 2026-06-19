namespace IMS.Helpers;

/// <summary>
/// Interface for pages that need explicit initialization of their ViewModel.
/// Used by MainLayout to trigger data loading without relying on fragile reflection.
/// </summary>
public interface IInitializablePage
{
    /// <summary>
    /// Initializes the page's ViewModel by calling its Initialize method.
    /// This replaces the fragile reflection-based TriggerPageOnAppearing approach.
    /// </summary>
    void Initialize();
}
