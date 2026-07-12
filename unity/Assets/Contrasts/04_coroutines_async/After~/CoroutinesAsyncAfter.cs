using UnityEngine;

namespace Contrasts.CoroutinesAsync
{
    // Contrasts: 04_coroutines_async -- After. Unlike most of these six
    // topics, the blocker here is not a C# language version at all:
    // `Awaitable` is a Unity 6 **API** (UnityEngine.CoreModule), and
    // Unity 6000.7.0a2 already ships it. See README.md for why this
    // still lives in After~.
    //
    // NOTE: this file lives in an "After~" folder. The trailing "~" makes
    // it invisible to Unity's asset pipeline, so it is never imported or
    // compiled by the project as it stands today.
    //
    // Mirrors Before/CoroutinesAsyncBefore.cs behavior 1:1 -- same
    // Debug.Log lines, same countdown -- so the two files diff cleanly.
    public class CoroutinesAsyncAfter : MonoBehaviour
    {
        [ContextMenu("Run")]
        private void Run()
        {
            // Fire-and-forget from a non-async context, same as
            // StartCoroutine(...) does for the IEnumerator version.
            _ = CountdownAsync(3);
        }

        // `async Awaitable` replaces `IEnumerator` + StartCoroutine: this
        // reads as an ordinary async method (exceptions propagate through
        // await, you can return values, `await` composes with other
        // Awaitables/Tasks), instead of a state machine driven externally
        // by the coroutine scheduler.
        private async Awaitable CountdownAsync(int seconds)
        {
            for (int i = seconds; i > 0; i--)
            {
                Debug.Log($"countdown: {i}");
                await Awaitable.WaitForSecondsAsync(1f);
            }

            Debug.Log("liftoff");
        }
    }
}
