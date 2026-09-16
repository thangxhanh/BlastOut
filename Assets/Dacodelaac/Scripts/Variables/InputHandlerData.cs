using System;
using Lean.Touch;
using UnityEngine;

namespace Dacodelaac.Variables
{
    public class InputHandlerData
    {
        public LeanFinger Finger { get; set; }
        public bool IsActive => Finger != null;
        public bool Stopped { get; set; }
        public bool Down => IsActive && Finger.Down;
        public bool Up => IsActive && Finger.Up;
        public bool Drag => IsActive && !Finger.StartedOverGui && Finger.Set;
        public bool Tap => IsActive && Finger.Tap;
        public Vector3 GetWorldPosition(float distance) => IsActive ? Finger.GetWorldPosition(distance) : Vector3.zero;
        public Vector2 Direction { get; set; }
    }
}