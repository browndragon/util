using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Math
{
    public static class Bresenham
    {

        /// Returns all points from thiz->end using Bresenham's per http://members.chello.at/~easyfilter/bresenham.html .
        // Produces a rasterization where 0 <= dRise <= 1 wrt dRun==1; you can use Octant to map any octant into this space.
        // Includes both endpoints (0,0) and (totalRun,totalRise)
        public struct RiseRun : IEnumerable<int>, IEnumerator<int>
        {
            public int Run;  // The major axis (incremented by 1 each step). This is also just the execution index.
            public int Rise;  // The minor axis (incremented by 1 on error accumulation).
            public int Current => Rise;
            object IEnumerator.Current => Current;

            public int totalRise;  // Also, error from each step.
            public int totalRun;  // Also, error reduction from each correction.
            public int err;
            public RiseRun(int totalRise, int totalRun)
            {
                this = default;
                this.totalRise = totalRise;
                this.totalRun = totalRun;
                Reset();
            }
            public void Reset()
            {
                Run = -1;
                Rise = 0;
                err = 3 * totalRise / 2;  // totalRise to cover the initial MoveNext to comply with iteration reqs.
            }
            public bool MoveNext()
            {
                Run++;
                err -= totalRise;
                if (err < 0)
                {
                    Rise++;
                    err += totalRun;
                }
                return Run <= totalRun;
            }
            public bool Skip(int count)
            {
                Run += count;
                err -= count * totalRise;
                if (err < 0)
                {
                    int adjustSteps = System.Math.DivRem(-err, totalRun, out int rem);
                    Rise += adjustSteps;
                    if (rem == 0) err = 0;
                    else
                    {
                        Rise += 1;
                        err = totalRun - rem;
                    }
                }
                return Run <= totalRun;
            }
            public bool SkipTo(int run) => Skip(run - Run);
            public RiseRun GetEnumerator() => this;
            IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void Dispose() { }
        }
        public struct Rasterizer : IEnumerable<Vector2Int>, IEnumerator<Vector2Int>
        {
            public Vector2Int Offset;
            public Vector2Int current;
            public Octant Octant;
            public RiseRun RiseRun;
            public Rasterizer(Vector2Int offset, Octant octant, RiseRun riseRun)
            {
                current = Offset = offset;
                Octant = octant;
                RiseRun = riseRun;
            }
            public Rasterizer GetEnumerator() => this;
            IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public Vector2Int Current => current;
            object IEnumerator.Current => Current;
            public bool MoveNext()
            {
                bool success = RiseRun.MoveNext();
                current = Offset + Octant.AsVector(RiseRun.Run, RiseRun.Rise);
                return success;
            }
            public void Reset()
            {
                RiseRun.Reset();
                current = Offset;
            }
            public void Dispose() { }
        }

        /// Returns all points from thiz->end (both included) using Bresenham's per http://members.chello.at/~easyfilter/bresenham.html .
        public static Rasterizer Rasterized(this Vector2Int thiz, Vector2Int end)
        {
            Vector2Int delta = end - thiz;
            Octant octant = delta.BestOctant();
            octant.AsMajorMinor(delta, out int major, out int minor);
            RiseRun riseRun = new(minor, major);
            return new(thiz, octant, riseRun);
        }
    }
}