using System;
using UnityEngine;

namespace Major.World {
    public class CustomTriggerArea : TriggerArea {
        public event Action<GameObject> onBegin = (sender) => { };
        public event Action<GameObject> onEnd = (sender) => { };

        protected override void OnTriggerAreaBegin(GameObject sender) => onBegin.Invoke(sender);
        protected override void OnTriggerAreaEnd(GameObject sender) => onEnd.Invoke(sender);
    }
}