using UnityEngine;

namespace UGameCore.Utilities
{
    public static class AnimationCurveUtils
    {
        public static AnimationCurve LinearUpward() => AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public static AnimationCurve LinearDownward() => AnimationCurve.Linear(0f, 1f, 1f, 0f);

        public static AnimationCurve EaseInOutDownward() => AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        public static AnimationCurve LogarithmicDownward(float middlePoint = 0.1f)
        {
            // this is not a perfect logarithmic curve, because I don't know how to properly
            // assign tangents/weights

            Keyframe[] keyframes = new Keyframe[]
            {
                new(0f, 1f, 0f, 0f, 1f, 1f),
                new(middlePoint, middlePoint, -(1f - middlePoint), -(1f - middlePoint), 0f, 0f),
                new(1f, 0f, -10f * middlePoint, 0f, 1f, 1f),
            };

            AnimationCurve curve = new(keyframes);

            //for (int i = 0; i < keyframes.Length; i++)
            //    curve.SmoothTangents(i, 0f);

            return curve;
        }

        public static AnimationCurve ConstantThenDownward(float constantValue, float timeWhenConstantValueFinishes)
        {
            Keyframe[] keyframes = new Keyframe[]
            {
                new(0f, constantValue),
                new(timeWhenConstantValueFinishes, constantValue),
                new(1f, 0f),
            };

            AnimationCurve curve = new(keyframes);
            return curve;
        }

        public static AnimationCurve MaxThenDownward(float timeWhenConstantValueFinishes)
            => ConstantThenDownward(1f, timeWhenConstantValueFinishes);
    }
}
