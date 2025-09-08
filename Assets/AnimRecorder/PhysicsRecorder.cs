using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class PhysicsRecorder : MonoBehaviour
{
    public Transform target;
    public bool IsRecording { get; private set; }

    private readonly List<Vector3> _positions = new();
    private readonly List<Quaternion> _rotations = new();
    private readonly List<float> _times = new();

    public IReadOnlyList<Vector3> Positions => _positions;
    public IReadOnlyList<Quaternion> Rotations => _rotations;
    public IReadOnlyList<float> Times => _times;

    public void StartRecording()
    {
        if (target == null) target = transform;
        _positions.Clear();
        _rotations.Clear();
        _times.Clear();
        IsRecording = true;
        Sample();
    }

    public void StopRecording()
    {
        IsRecording = false;
    }

    void FixedUpdate()
    {
        if (IsRecording) Sample();
    }

    private void Sample()
    {
        _positions.Add(target.localPosition);
        _rotations.Add(target.localRotation.normalized);
        _times.Add(Time.fixedTime);
    }
}