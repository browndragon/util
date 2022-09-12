using System;
using System.Collections;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Actions/Sink")]
    public class ActionsSink : ScriptableObject
    {
        public static ActionsSink main { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartRun()
        {
            Debug.Log($"RuntimeInitialize ran!");
            Coroutines.StartCoroutine(main.Run());
        }

        public ActionsHead[] UpdateHeads;
        readonly Disposes.All unsubscribe = new();
        public bool Cancel;
        public bool IsRunning;

        void OnEnable()
        {
            main = this;
            Cancel = false;
            if (UpdateHeads != null) foreach (ActionsHead head in UpdateHeads) unsubscribe.Add(head.Subscribe(OnAction));
        }
        void OnDisable()
        {
            Cancel = true;
            unsubscribe.Dispose();
        }
        void OnAction(Action action)
        {
            if (Cancel) return;
            action();
        }
        IEnumerable Run()
        {
            IsRunning = true;
            while (!Cancel)
            {
                bool poppedAny = false;
                if (UpdateHeads != null) foreach (ActionsHead head in UpdateHeads)
                    {
                        while (!Cancel && head.Pop()) poppedAny = true;
                        if (Cancel) break;
                    }
                if (Cancel) break;
                if (!poppedAny) yield return null;
            }
            IsRunning = false;
        }
    }
}