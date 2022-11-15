using UnityEngine;

namespace BDUtil.Screen
{
    /// Usually a serializable struct.
    public interface ISnapshot
    {
        void ReadFrom(GameObject player);
        void ApplyTo(GameObject player);
    }
    public interface ISnapshot<TPlayer> : ISnapshot
    {
        void ReadFrom(TPlayer player);
        void ApplyTo(TPlayer player);
    }
}