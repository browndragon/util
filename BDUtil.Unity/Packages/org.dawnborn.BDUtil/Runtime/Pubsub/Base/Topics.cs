using System.Collections;

namespace BDUtil.Pubsub
{
    public static class Topics
    {
        public static float GetFloatOrDefault(this IObjectTopic thiz, float @default = default) => thiz switch
        {
            null => @default,
            ITopic<float> t => t.Value,
            ITopic<int> t => t.Value,
            ITopic<bool> t => t.Value ? 1f : 0f,
            ITopic<Lock> t => t.Value ? 1f : 0f,
            ICollectionTopic t => ((ICollection)t.Collection).Count,
            _ => thiz.Value switch
            {
                null => 0f,
                float f => f,
                int f => f,
                bool f => f ? 1f : 0f,
                Lock f => f ? 1f : 0f,
                ICollection f => f.Count,
                _ => 1f,
            },
        };
        public static bool IsValuePositive(this IObjectTopic thiz)
        => thiz.GetFloatOrDefault(float.NegativeInfinity) > 0f;
    }
}