using System;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/PredicateTopic")]
    [Tooltip("Considers positive integers & floats (& locks, & non-null, etc) true.")]
    public class PredicateTopic : Topic<bool>
    {
        [Serializable]
        protected struct PredicateSubscriber : Subscriber.ISubscribe
        {
            public AST ast;

            public Action Subscribe(Subscriber thiz)
            {
                throw new NotImplementedException();
            }
        }
        public enum Predicate
        {
            [Tooltip("AKA AND")] All,
            [Tooltip("AKA OR")] Any,
            [Tooltip("AKA NOR")] None,
            [Tooltip("AKA XOR")] Not,
            [Tooltip("AKA NAND")] NotAll,
        }
        [Serializable]
        public struct AST
        {
            public Predicate Predicate;
            [Expandable] public ObjectTopic[] Topics;
            public void Subscribe(Action onUpdate, Disposes.All unsubscribe)
            {
                if (Topics != null) foreach (ObjectTopic topic in Topics) unsubscribe.Add(topic.Subscribe(onUpdate));
            }
            public bool Evaluate() => Predicate switch
            {
                Predicate.All => EvaluateAll(),
                Predicate.Any => EvaluateAny(),
                Predicate.None => !EvaluateAny(),
                Predicate.Not => EvaluateXor(),
                Predicate.NotAll => !EvaluateAll(),
                _ => throw new NotImplementedException($"Unhandled {Predicate}"),
            };
            bool EvaluateXor()
            {
                bool value = true;
                if (Topics != null) foreach (ObjectTopic topic in Topics) value ^= topic.IsValuePositive();
                return value;
            }
            bool EvaluateAll()
            {
                bool value = true;
                if (Topics != null) foreach (ObjectTopic topic in Topics) if (!(value &= topic.IsValuePositive())) break;
                return value;
            }
            bool EvaluateAny()
            {
                bool value = false;
                if (Topics != null) foreach (ObjectTopic topic in Topics) if (value |= topic.IsValuePositive()) break;
                return value;
            }
        }
        [SerializeField] AST ast;
        [SerializeField] bool value;
        public override bool Value
        {
            get => value;
            set
            {
                bool was = this.value;
                this.value = value;
                if (was ^ value) Publish();
            }
        }
        readonly Disposes.All unsubscribe = new();
        void OnValidate()
        {
            unsubscribe.Dispose();
            ast.Subscribe(Reevaluate, unsubscribe);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            ast.Subscribe(Reevaluate, unsubscribe);
        }
        protected override void OnDisable()
        {
            unsubscribe.Dispose();
            base.OnDisable();
        }
        void Reevaluate() => Value = ast.Evaluate();
    }
}