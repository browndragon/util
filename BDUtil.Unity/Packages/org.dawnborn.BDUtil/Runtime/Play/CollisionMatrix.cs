using System;
using System.Text;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Play
{
    [Serializable]
    public struct CollisionMatrix
    {
        [SerializeField]
        internal Blocks.B32<LayerMask> LayerMasks;
        public override string ToString()
        {
            StringBuilder builder = new();
            for (int i = 0; i < LayerMasks.Count; ++i) builder.Append($"{i}: {(int)LayerMasks[i]:x8}\n");
            return builder.ToString();
        }
        public bool this[int layerFrom, int layerTo]
        {
            get
            {
                if (layerTo < layerFrom) (layerFrom, layerTo) = (layerTo, layerFrom);
                if (layerTo < 0 || layerFrom >= LayerMasks.Count) throw new NotSupportedException($"{layerFrom}<->{layerTo}");
                return (LayerMasks[layerFrom] & (1 << layerTo)) != 0;
            }
            set
            {
                if (layerFrom >= LayerMasks.Count) throw new NotSupportedException($"{layerFrom}<->{layerTo}");
                if (layerTo >= LayerMasks.Count) throw new NotSupportedException($"{layerFrom}<->{layerTo}");
                if (value)
                {
                    LayerMasks[layerFrom] |= 1 << layerTo;
                    LayerMasks[layerTo] |= 1 << layerFrom;
                }
                else
                {
                    LayerMasks[layerFrom] &= ~(1 << layerTo);
                    LayerMasks[layerTo] &= ~(1 << layerFrom);
                }
            }
        }
        public LayerMask this[int layerFrom]
        {
            get => LayerMasks[layerFrom];
            set
            {
                for (int i = 0; i < LayerMasks.Count; ++i)
                {
                    this[layerFrom, i] = (value & (1 << i)) != 0;
                }
            }
        }

        public LayerMask GetRHS(int layerFrom)
        {
            // return this[layerFrom];
            LayerMask got = default;
            for (int i = layerFrom; i < LayerMasks.Count; ++i)
            {
                if (this[layerFrom, i]) got |= 1 << i;
            }
            // Enable "everything" by setting the invisible low order bits.
            if (got != 0) for (int i = 0; i < layerFrom; ++i) got |= 1 << i;
            return got;
        }
        /// Take the bottom N-1 bits from `this`, and the top 32-N bits from `mask`.
        /// This does break "everything", since it's not 0xFF..FF, but instead 0xFF..00 (or whatever).
        /// Deal.
        public void SetRHS(int layerFrom, int mask)
        {
            for (int i = layerFrom; i < LayerMasks.Count; ++i)
            {
                this[layerFrom, i] = 0 != (mask & (1 << i));
            }
        }

        public void SetFromPhysics2D()
        {
            for (int i = 0; i < LayerMasks.Count; ++i)
            {
                LayerMasks[i] = Physics2D.GetLayerCollisionMask(i);
            }
        }
        public void SetToPhysics2D()
        {
            for (int i = 0; i < LayerMasks.Count; ++i)
            {
                Physics2D.SetLayerCollisionMask(i, LayerMasks[i]);
            }
        }
    }
}