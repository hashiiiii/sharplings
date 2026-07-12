using UnityEngine;

namespace Contrasts.StateMachines
{
    // Contrasts: 03_state_machines -- Before (C# 9, today's Unity default).
    // See README.md in this folder for the full EN/JA explanation.
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
    }

    public enum PlayerTrigger
    {
        Move,
        Stop,
        Jump,
        Land,
    }

    public class StateMachineBefore : MonoBehaviour
    {
        private PlayerState _state = PlayerState.Idle;

        [ContextMenu("Run")]
        private void Run()
        {
            Fire(PlayerTrigger.Move);
            Fire(PlayerTrigger.Jump);
            Fire(PlayerTrigger.Land);
            Fire(PlayerTrigger.Stop);

            // Combo detection over a fixed input buffer -- explicit index
            // comparisons, the C# 9 way.
            var inputs = new[] { PlayerTrigger.Move, PlayerTrigger.Jump, PlayerTrigger.Land };
            Debug.Log($"is dash-jump combo: {IsDashJumpCombo(inputs)}");
        }

        // Enum field + switch statements with break: the C# 9 Unity idiom
        // for a small state machine.
        private void Fire(PlayerTrigger trigger)
        {
            PlayerState next;

            switch (_state)
            {
                case PlayerState.Idle:
                    switch (trigger)
                    {
                        case PlayerTrigger.Move:
                            next = PlayerState.Running;
                            break;
                        default:
                            next = _state;
                            break;
                    }

                    break;

                case PlayerState.Running:
                    switch (trigger)
                    {
                        case PlayerTrigger.Jump:
                            next = PlayerState.Jumping;
                            break;
                        case PlayerTrigger.Stop:
                            next = PlayerState.Idle;
                            break;
                        default:
                            next = _state;
                            break;
                    }

                    break;

                case PlayerState.Jumping:
                    switch (trigger)
                    {
                        case PlayerTrigger.Land:
                            next = PlayerState.Idle;
                            break;
                        default:
                            next = _state;
                            break;
                    }

                    break;

                default:
                    next = _state;
                    break;
            }

            Debug.Log($"{_state} + {trigger} -> {next}");
            _state = next;
        }

        private static bool IsDashJumpCombo(PlayerTrigger[] inputs)
        {
            if (inputs.Length < 2)
            {
                return false;
            }

            return inputs[0] == PlayerTrigger.Move && inputs[1] == PlayerTrigger.Jump;
        }
    }
}
