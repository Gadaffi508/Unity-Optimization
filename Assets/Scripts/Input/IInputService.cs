using UnityEngine;

public interface IInputService
{
    bool JumpPressed();
    bool DashPressed();
    bool CancelPressed();
}

public class UnityInputService : IInputService
{
    public bool JumpPressed() => Input.GetKeyDown(KeyCode.Space);
    public bool DashPressed() => Input.GetKeyDown(KeyCode.LeftShift);
    public bool CancelPressed() => Input.GetKeyDown(KeyCode.Escape);
}