using System;
using System.Collections;
using UnityEngine;

namespace Buck
{
    public static class StaticFunctions
    {
        #region Invoke Inline
        /// <summary>
        /// 
        /// this.Invoke(MyFunctionNoParams, 1f);
        /// this.Invoke(() => MyFunctionParams(0, false), 1f);
        /// this.Invoke(()=>Debug.Log("Lambdas also work"), 1f);
        /// 
        /// </summary>
        /// <param name="mb"></param>
        /// <param name="f"></param>
        /// <param name="delay"></param>
        public static void Invoke(this MonoBehaviour mb, Action f, float delay)
        {
            mb.StartCoroutine(InvokeRoutine(f, delay));
        }

        private static IEnumerator InvokeRoutine(System.Action f, float delay)
        {
            yield return new WaitForSeconds(delay);
            f();
        }
        #endregion

        /// <summary>
        /// Returns normalized psuedo-random value 0-1 based on perlin-noise
        /// </summary>
        public static float SeedRandom(float seed, float offset)
        {
            return Mathf.PerlinNoise(seed * seed, offset) % 1;
        }
    }
}
