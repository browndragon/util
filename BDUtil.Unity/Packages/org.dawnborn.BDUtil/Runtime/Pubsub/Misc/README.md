# Unity/Pubsub/Misc

These are related to events & messages, but don't actually use the scriptableobject topics :->

* CollisionNInvoker turns the SendMessage events into UnityEvents. They publish the other body (on the theory that whatever you use to listen can remember "this"). That might not be wise; further study needed.
* OnLifecycle has the same idea for the SendMessage lifecycle events (currently Start/OnDestroy & OnEnable/OnDisable). This somewhat overlaps Ticker, but not completely; Ticker is a standalone object with its own drumbeat (so its OnEnable isnt your object's OnEnable...) .
