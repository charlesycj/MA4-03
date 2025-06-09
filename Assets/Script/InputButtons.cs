using Fusion;
using UnityEngine;

public enum InputButtons
{
    None = 0,
    LeftMouse = 1 << 0,
    RightMouse = 1 << 1,
}

public struct NetworkInputData : INetworkInput
{
    public NetworkButtons Buttons;
    
    public Vector3 MousePosition;
}