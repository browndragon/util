using UnityEngine;

namespace BDUtil.Play
{
    public class OnState : StateMachineBehaviour
    {
        public interface IEnter
        {
            void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        }
        public interface IExit
        {
            void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        }
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (IEnter value in animator.GetComponents<IEnter>()) value.OnStateEnter(animator, stateInfo, layerIndex);
        }
        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (IExit value in animator.GetComponents<IExit>()) value.OnStateExit(animator, stateInfo, layerIndex);
        }
    }
}