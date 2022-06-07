using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil
{
    [Tooltip("A pool is a (semantic-neutral) supplier of a mapping between GetInstanceId->GameObject")]
    public abstract class BasePool : MonoBehaviour
    {
        [SerializeField, SuppressMessage("IDE", "IDE0044")]
        protected FKeySet<int, GameObject> Objects = new(go => go.GetInstanceID());
        public IReadOnlyDictionary<int, GameObject> GameObjects => Objects;

        public abstract void EnableMe(GameObject me);
        public abstract void DisableMe(GameObject me);

        public bool TryPop(out GameObject gameObject)
        {
            foreach (GameObject v in Objects)
            {
                Objects.Remove(v);
                gameObject = v;
                return true;
            }
            gameObject = default;
            return false;
        }
    }
}