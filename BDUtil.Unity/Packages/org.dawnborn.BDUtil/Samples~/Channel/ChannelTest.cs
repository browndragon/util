// using System.Collections;
// using System.Collections.Generic;
// using BDUtil.Pubsub;
// using UnityEngine;

// namespace BDUtil
// {
//     public class ChannelTest : MonoBehaviour
//     {
//         public ATSO OnCreate;
//         public ATSO OnDestroy;
//         new Camera camera;
//         public ATListener Proto;
//         void Awake() => camera = Camera.main;
//         void Update()
//         {
//             if (!Input.GetMouseButtonUp(0)) return;
//             Vector2 point = camera.ScreenToWorldPoint(Input.mousePosition);
//             Collider2D atPoint = Physics2D.OverlapPoint(point);
//             if (atPoint != null)
//             {
//                 Destroy(atPoint.gameObject);
//                 OnDestroy.Value.Invoke();
//                 return;
//             }
//             OnCreate.Value.Invoke();
//             Instantiate(Proto, camera.ScreenToWorldPoint(Input.mousePosition).WithZ(0f), Quaternion.identity);
//         }
//     }
// }