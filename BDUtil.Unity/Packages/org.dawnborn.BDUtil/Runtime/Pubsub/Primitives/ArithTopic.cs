using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    /// [RPN](https://en.wikipedia.org/wiki/Reverse_Polish_notation) calculator.
    /// Per example, """3 − 4 + 5 in conventional notation would be written 3 4 − 5 +"""
    /// so note the stack order for the binops that care (-, /, %, ^)!
    /// Since Predicate checks for positivity, you can get GTEQ (etc) behaviour using subtraction.
    /// Collections' value is their length; boolean (or convertible to boolean...) are 1f/0f.
    [CreateAssetMenu(menuName = "BDUtil/Prim/ArithTopic")]
    public class ArithTopic : Topic<float>
    {
        public enum Instruction
        {
            Value = default,
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulus,
            Power,
            /// Sends all negative values positive, all positive values negative, and 0 positive.
            BoolFlip,
        }
        [Serializable]
        public struct AST
        {
            [Tooltip("Values are only used if this is Instruction.Value")]
            public Instruction Instruction;
            [Tooltip("If Instruction.Value: value coerced to float is stored.")]
            public ObjectTopic Topic;
            [Tooltip("If Instruction.Value && Topic == null: value is stored.")]
            public float Constant;

            public void Evaluate(List<float> state)
            {
                float a, b;
                switch (Instruction)
                {
                    case Instruction.Value: state.Add(Topic.GetFloatOrDefault(Constant)); break;
                    case Instruction.Add: state.Add(state.PopBack() + state.PopBack()); break;
                    case Instruction.Subtract: state.Add(-state.PopBack() + state.PopBack()); break;
                    case Instruction.Multiply: state.Add(state.PopBack() * state.PopBack()); break;
                    case Instruction.Divide: state.Add(1f / state.PopBack() * state.PopBack()); break;
                    case Instruction.Modulus:
                        (b, a) = (state.PopBack(), state.PopBack());
                        state.Add(a % b);
                        break;
                    case Instruction.Power:
                        (b, a) = (state.PopBack(), state.PopBack());
                        state.Add(Mathf.Pow(a, b));
                        break;
                }
            }
        }
        [Tooltip("Reverse polish notation; implemented using a standard list of floats to store values.")]
        [SerializeField] AST[] asts;
        readonly List<float> scratch;
        [SerializeField] float value;
        public override float Value
        {
            get => value;
            set
            {
                float was = this.value;
                this.value = value;
                if (was != value) Publish();
            }
        }
        readonly Disposes.All unsubscribe = new();
        void OnValidate()
        {
            unsubscribe.Dispose();
            Resubscribe();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            Resubscribe();
        }
        protected override void OnDisable()
        {
            unsubscribe.Dispose();
            base.OnDisable();
        }
        void Resubscribe()
        {
            HashSet<Topic> subscribed = new();
            if (asts != null) foreach (AST ast in asts)
                {
                    if (ast.Topic == null || !subscribed.Add(ast.Topic)) continue;
                    unsubscribe.Add(ast.Topic.Subscribe(Reevaluate));
                }
        }
        void Reevaluate()
        {
            scratch.Clear();
            if (asts != null) foreach (AST ast in asts) ast.Evaluate(scratch);
            Value = scratch.PopBack();
        }
    }
}