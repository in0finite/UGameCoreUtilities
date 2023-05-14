using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class AnimationExtensions
    {
        public static void Update(this Animation animation, float deltaTime)
        {
            foreach (AnimationState animationState in animation)
            {
                if (!animationState.enabled)
                    continue;

                animationState.Update(deltaTime);
            }

            animation.Sample();
        }

        public static void Update(this AnimationState animationState, float deltaTime)
        {
            float newTime = (animationState.time + deltaTime * animationState.speed + animationState.length) % animationState.length;
            if (float.IsFinite(newTime))
                animationState.time = newTime;
        }

        public static IEnumerable<AnimationState> GetAnimationStates(this Animation animation)
        {
            foreach (AnimationState animationState in animation)
                yield return animationState;
        }

        public static float GetTimePerc(this AnimationState state)
        {
            return state.time / state.length;
        }

        public static void SetTimePerc(this AnimationState state, float perc)
        {
            state.time = state.length * perc;
        }
    }
}
