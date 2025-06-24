using System;
using UnityEngine;

namespace Major.World {
    public class CustomTrigger : Trigger {
        public event Action<GameObject> onBegin = (sender) => { };
        public event Action<GameObject> onEnd = (sender) => { };

        protected override void OnTriggerBegin(GameObject sender) => onBegin.Invoke(sender);
        protected override void OnTriggerEnd(GameObject sender) => onEnd.Invoke(sender);
    }
}