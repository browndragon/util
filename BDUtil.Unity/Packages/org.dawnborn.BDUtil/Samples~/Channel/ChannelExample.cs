using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BDUtil.Channels.Channel), typeof(BDUtil.Channels.Listener))]
public class ChannelExample : MonoBehaviour
{
    public void OnMouseDown()
    {
        // Test: send to the 0th channel.
        GetComponent<BDUtil.Channels.Listener>().Bindings[0].Channels[0].Invoke();
    }
    public void OnMouseUp()
    {
        // Test: send to the 1st channel.
        GetComponent<BDUtil.Channels.Listener>().Bindings[1].Channels[0].Invoke();
    }
    public void OnBeginEvent() => Debug.Log($"BeginEvent");
    public void OnEndEvent() => Debug.Log($"EndEvent");
}
