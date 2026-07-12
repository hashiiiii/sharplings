using System.Collections;
using UnityEngine;

namespace Contrasts.CoroutinesAsync
{
    // Contrasts: 04_coroutines_async -- Before (C# 9, today's Unity default).
    // See README.md in this folder for the full EN/JA explanation.
    //
    // The classic Unity idiom: an IEnumerator coroutine, started with
    // StartCoroutine, yielding WaitForSeconds to pause between steps.
    public class CoroutinesAsyncBefore : MonoBehaviour
    {
        [ContextMenu("Run")]
        private void Run()
        {
            StartCoroutine(CountdownRoutine(3));
        }

        private IEnumerator CountdownRoutine(int seconds)
        {
            for (int i = seconds; i > 0; i--)
            {
                Debug.Log($"countdown: {i}");
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("liftoff");
        }
    }
}
