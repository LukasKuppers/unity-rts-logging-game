
public interface ISelectableObject
{
    /// <summary>
    /// Gets the control mode for this object
    /// </summary>
    public IPlayerControlMode GetPlayerControlMode();

    /// <summary>
    /// Called when the object is first selected
    /// </summary>
    public void OnSelect();

    /// <summary>
    /// Called when the object is deselected
    /// </summary>
    public void OnDeselect();
}
