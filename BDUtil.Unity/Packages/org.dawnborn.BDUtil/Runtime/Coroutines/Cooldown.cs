using System;
using UnityEngine;

namespace BDUtil.Math
{
    public static class Cooldowns
    {
        public static Cooldown.Heats GetNextHeat(this Cooldown.Heats thiz) => thiz switch
        {
            Cooldown.Heats.Cold => Cooldown.Heats.Warming,
            Cooldown.Heats.Warming => Cooldown.Heats.Hot,
            Cooldown.Heats.Hot => Cooldown.Heats.Cooling,
            Cooldown.Heats.Cooling => Cooldown.Heats.Cold,
            _ => throw thiz.BadValue(),
        };
    }
    /// Generic support for a phased state which goes through cold, warm, hot, and cool states, any of which might be of 0 or infinite length.
    /// The idea is that you can set it to Warm or to Cool, and it will cycle through to Hot or Cold.
    [Serializable]
    public struct Cooldown
    {
        // Length of time to spend in each non-cold (which is default) state.
        // You could also call pause/restart in specific states if exit from a state requires additional logic.
        // Use Warm/Rewarm to go from None->Pre or Cool to go from Pre/Live->Post.
        public float Warming, Hot, Cooling;
        public float Cold => float.PositiveInfinity;
        [field: SerializeField] public Heats PeekHeat { get; private set; }
        /// ++ whenever the temperature goes hot.
        [field: SerializeField] public int Count { get; private set; }
        public Delay Timer;
        public int ResetCount()
        {
            GetCurrentHeat();
            int hadCount = Count;
            Count = 0;
            return hadCount;
        }

        public Cooldown(float warming, float hot, float cooling, Clock clock = default)
        {
            Warming = warming;
            Hot = hot;
            Cooling = cooling;
            PeekHeat = Heats.Cold;
            Count = 0;
            Timer = clock.StoppedDelayOf(float.PositiveInfinity);
        }
        public void Reset()
        {
            PeekHeat = Heats.Cold;
            Count = 0;
            Timer.Length = float.PositiveInfinity;
            Timer.Stop();
        }
        public enum Heats
        {
            Cold = default,
            Warming,
            Hot,
            Cooling,
        };
        public bool IsHot => GetCurrentHeat() == Heats.Hot;
        public bool IsCold => GetCurrentHeat() == Heats.Cold;
        public static implicit operator bool(Cooldown thiz) => thiz.IsHot;
        public Delay.Tick HeatTick => Timer.Ratio;
        public void PauseHeat() => Timer.Stop();
        public void RestartHeat() => Timer.Reset();
        public void NextHeat() => SetHeat(GetCurrentHeat().GetNextHeat());
        public void ExtendHeat(float time) => Timer.Length += time;

        // Drive this engine towards hot. This respects warming and cooling periods.
        // Depending on params, you can skip cooling (moving straight to warming) or extend the hot period.
        public void Warm(bool skipCooling = false, bool restartHot = false)
        {
            switch (GetCurrentHeat())
            {
                case Heats.Warming: return;
                case Heats.Hot: if (restartHot) RestartHeat(); return;
                case Heats.Cooling: if (!skipCooling) return; break;
            }
            SetHeat(Heats.Warming);
        }
        // Drives a warming or hot engine immediately into cooling (or by param, warming->cold since it hadn't hit hot yet).
        public void Cool(bool skipWarmingToCold = false)
        {
            switch (GetCurrentHeat())
            {
                case Heats.Cold:
                case Heats.Cooling: return;
                case Heats.Warming: SetHeat(skipWarmingToCold ? Heats.Cold : Heats.Cooling); return;
            }
            SetHeat(Heats.Cooling);
        }
        // Advances the time to "now" and returns the heat.
        // If there were multiple Hots, Count will get incremented.
        public Heats GetCurrentHeat()
        {
            while (Timer.Ratio.IsOver)
            {
                float end = Timer.End;
                SetHeat(PeekHeat.GetNextHeat());
                Timer.Start = end;
            }
            return PeekHeat;
        }
        /// Unconditionally sets us to heat & sets the proper timer.
        public void SetHeat(Heats heat)
        {
            Heats was = PeekHeat;
            Timer.Reset((PeekHeat = heat) switch
            {
                Heats.Cold => float.PositiveInfinity,
                Heats.Warming => Warming,
                Heats.Hot => Hot,
                Heats.Cooling => Cooling,
                _ => throw heat.BadValue(),
            });
            if (PeekHeat == Heats.Hot && was != Heats.Hot) Count++;
        }
    }
}
