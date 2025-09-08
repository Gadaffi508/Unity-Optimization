using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IInputService _input;
    private void Awake()
    {
        _input = new UnityInputService();
    }
    private void Update()
    {
        if (_input.JumpPressed()) Jump();
        if (_input.DashPressed()) Dash();
    }
    private void Jump() => Debug.Log("Player Jumped!");
    private void Dash() => Debug.Log("Player Dashed!");
}