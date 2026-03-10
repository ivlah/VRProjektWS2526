/// <summary>
/// Simple interface to standardize initialization order in Room1.
/// Any Manager/Service that wants to be initialized by Room1_BackendRoot
/// can implement this interface.
/// </summary>
public interface IRoom1Initializable
{
    /// <summary>
    /// Defines initialization priority.
    /// Lower numbers initialize earlier (e.g., 0 before 100).
    /// Use this to ensure deterministic startup order.
    /// </summary>
    int InitOrder { get; }

    /// <summary>
    /// Called exactly once by the Room1 backend bootstrap chain.
    /// </summary>
    void Initialize(Room1BackendRoot root);
}