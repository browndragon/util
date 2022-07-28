using System;

namespace BDUtil.Math
{
    /// Transforms an error value into an output adjustment via a https://en.wikipedia.org/wiki/PID_controller .
    /// This is particularly helpful for combining something like a tween with physics,
    /// where the tween sets a position or velocity, while the output of the PID is the requested force
    /// to be applied to the body to accomplish that goal.
    [Serializable]
    public struct PID
    {
        // Construction note: PID<Vector2> (eg) depends _out_ on BDEaseUnity defining V2Arith,
        // and so in terms of initialization order, we need to try to not grab a reference to that class
        // before they've had a chance to reference Arith.Initialize() & UnityArith.Initialize().
        // Since this is likely a field on a monobehaviour and will use our own static Default, that means
        // waiting to define Arith until, say, their class' init time.
        // Values kind of arbitrarily chosen in V2 space with a known object optimizing for diagonal convergence time.
        public readonly static PID Default = new(7f, 100f, .075f, 120f, float.PositiveInfinity);
        /// The overall gain for this PID controller.
        public float Gain;
        /// The time the integral term takes to correct for error.
        public float ITime;
        /// The time the derivative term takes to correct for error.
        public float DTime;
        /// Input error clamp
        public float MaxIn;
        /// Output response clamp
        public float MaxOut;

        public PID(float gain, float iTime, float dTime, float maxIn, float maxOut)
        {
            Gain = gain;
            ITime = iTime;
            DTime = dTime;
            MaxIn = maxIn;
            MaxOut = maxOut;
        }

        /// dT: Timestep (usually just Time.fixedDeltaTime).
        /// Error: target - actual ("offset" in, say, position).
        /// Might be capped (with implications for cumulativeError)!
        /// LastError: Maintained at error; used to calculate the derivative.
        /// CumulativeError: Internally maintained sum of observed errors scaled by time.
        /// Returns: The output variable (weighted by error, delta, and cumulative error).
        public T Apply<T>(float dT, T error, ref T lastError, ref T cumulativeError)
        {
            IArith<T> arith = Arith<T>.Default;
            error = arith.Clamp(error, MaxIn);

            // Antiwindup:
            // If error-dot-last is negative, then we need to turn >90deg; enough of a change to wipe cumulative error.
            if (arith.Dot(error, lastError) <= 0f) cumulativeError = default;

            // Derivative: Scale the change in error by DTime/dT.
            T dFactor = arith.Difference(error, lastError);
            dFactor = dT != 0f ? arith.Scale(DTime / dT, dFactor) : default;
            lastError = error;
            // Integral: update the cumulative error by error*dT; scale by ITime.
            cumulativeError = arith.Add(cumulativeError, arith.Scale(dT, error));
            T iFactor = ITime != 0f ? arith.Scale(1f / ITime, cumulativeError) : default;

            error = arith.Add(error, dFactor);
            error = arith.Add(error, iFactor);
            T res = arith.Scale(Gain, error);

            res = arith.Clamp(res, MaxOut);
            return res;
        }

        public struct State<T>
        {
            public T Error;
            public T LastError;
            public T CumulativeError;
            public T Output;
        }
        public void Apply<T>(float dT, ref State<T> state)
        => state.Output = Apply(dT, state.Error, ref state.LastError, ref state.CumulativeError);
    }
}