﻿using System.Collections.Generic;
using Improbable.Gdk.Movement;
using Improbable.Gdk.StandardTypes;
using UnityEngine;

public class MyMovementUtils
{
    public interface IMovementProcessor
    {
        Vector3 GetMovement(CharacterController controller, ClientRequest input, int frame, Vector3 velocity,
            Vector3 previous);

        void Clean(int frame);
    }

    public static readonly MovementSettings movementSettings = new MovementSettings
    {
        MovementSpeed = new MovementSpeedSettings
        {
            WalkSpeed = 2f,
            RunSpeed = 3.5f,
            SprintSpeed = 6f
        },
        SprintCooldown = 0.2f,
        Gravity = 25f,
        StartingJumpSpeed = 8f,
        TerminalVelocity = 40f,
        GroundedFallSpeed = 3.5f,
        AirControlModifier = 0.5f,
        InAirDamping = 0.05f
    };

    public class TerminalVelocity : IMovementProcessor
    {
        public Vector3 GetMovement(CharacterController controller, ClientRequest input, int frame, Vector3 velocity,
            Vector3 previous)
        {
            return Vector3.ClampMagnitude(previous, movementSettings.TerminalVelocity);
        }

        public void Clean(int frame)
        {
            // Do nothing, not storing any state that needs cleaning.
        }
    }

    public class Gravity : IMovementProcessor
    {
        public Vector3 GetMovement(CharacterController controller, ClientRequest input, int frame, Vector3 velocity,
            Vector3 previous)
        {
            return previous + Vector3.down * movementSettings.Gravity * FrameLength;
        }

        public void Clean(int frame)
        {
            // Do nothing, not storing any state that needs cleaning.
        }
    }

    public const float FrameLength = 1 / 30f;

    public static void ApplyInput(CharacterController controller, ClientRequest input, int frame, Vector3 velocity,
        IMovementProcessor[] processors)
    {
        var toMove = Vector3.zero;
        for (var i = 0; i < processors.Length; i++)
        {
            toMove = processors[i].GetMovement(controller, input, frame, velocity, toMove);
        }

        ApplyMovement(controller, toMove);
    }

    public static void ApplyMovement(CharacterController controller, Vector3 movement)
    {
        controller.Move(movement * FrameLength);
        controller.transform.position = controller.transform.position.ToIntAbsolute().ToVector3();
    }

    public static bool IsGrounded(CharacterController controller)
    {
        return Physics.CheckSphere(controller.transform.position, 0.1f, LayerMask.GetMask("Default"));
    }

    public static void CleanProcessors(IMovementProcessor[] processors, int frame)
    {
        for (var i = 0; i < processors.Length; i++)
        {
            processors[i].Clean(frame);
        }
    }
}
