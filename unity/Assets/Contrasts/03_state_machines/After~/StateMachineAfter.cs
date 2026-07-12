using UnityEngine;

namespace Contrasts.StateMachines
{
    // Contrasts: 03_state_machines -- After. Unlike most of these six
    // topics, nothing here strictly needs Unity 6.8: tuple patterns in a
    // switch expression are C# 8 (already legal at Unity's own default
    // LangVersion 9), and list patterns are C# 11 -- reachable today via
    // Stage 1's `-langversion:preview` (`docs/unity-lab-setup.md`), just
    // not at Unity's plain C# 9 default. See README.md.
    //
    // NOTE: this file lives in an "After~" folder. The trailing "~" makes
    // it invisible to Unity's asset pipeline, so it is never imported or
    // compiled by the project as it stands today -- kept out of
    // `Assets/Contrasts` regardless, for consistency with the other five
    // topics and to keep this folder dependency/langversion-free by
    // design.
    //
    // Mirrors Before/StateMachineBefore.cs behavior 1:1 -- same
    // Debug.Log lines for the same transitions -- so the two files diff
    // cleanly.
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

    public class StateMachineAfter : MonoBehaviour
    {
        private PlayerState _state = PlayerState.Idle;

        [ContextMenu("Run")]
        private void Run()
        {
            Fire(PlayerTrigger.Move);
            Fire(PlayerTrigger.Jump);
            Fire(PlayerTrigger.Land);
            Fire(PlayerTrigger.Stop);

            var inputs = new[] { PlayerTrigger.Move, PlayerTrigger.Jump, PlayerTrigger.Land };
            Debug.Log($"is dash-jump combo: {IsDashJumpCombo(inputs)}");
        }

        // Switch expression over a (state, trigger) tuple: the whole
        // nested-switch tree from Before collapses to one expression, and
        // an unhandled combination now falls out of `_ => _state` instead
        // of needing every branch to remember to `break`.
        private void Fire(PlayerTrigger trigger)
        {
            var next = (_state, trigger) switch
            {
                (PlayerState.Idle, PlayerTrigger.Move) => PlayerState.Running,
                (PlayerState.Running, PlayerTrigger.Jump) => PlayerState.Jumping,
                (PlayerState.Running, PlayerTrigger.Stop) => PlayerState.Idle,
                (PlayerState.Jumping, PlayerTrigger.Land) => PlayerState.Idle,
                _ => _state,
            };

            Debug.Log($"{_state} + {trigger} -> {next}");
            _state = next;
        }

        // List pattern: "starts with Move, then Jump, then anything" reads
        // directly as a shape, instead of manual length/index checks.
        private static bool IsDashJumpCombo(PlayerTrigger[] inputs) =>
            inputs is [PlayerTrigger.Move, PlayerTrigger.Jump, ..];
    }
}
