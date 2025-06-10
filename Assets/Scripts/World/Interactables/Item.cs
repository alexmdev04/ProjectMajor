using UnityEngine;
using Unity.Logging;
using System;

namespace Major.World {
    [RequireComponent(typeof(Rigidbody))]
    public class Item : Interactable {
        public enum PickupType {
            None,
            Instant,
            Carry
        }
        public PickupType pickupType;

        public enum ItemType {
            None,
        }

        private Rigidbody rb;
        private ItemSlot itemSlot;


        public Vector3 position {
            get { return rb.position; }
            set { rb.position = value; }
        }

        private void Awake() {
            rb = GetComponent<Rigidbody>();
        }

        public bool isCarried { get; private set; }

        public override void Interact(Player sender, Action callback = null) {
            switch (pickupType) {
                case PickupType.None: {
                        Log.Warning("[WorldItem] Interacted with None type WorldItem");
                        break;
                    }
                case PickupType.Instant: {
                        // add item type to inventory and destroy world item
                        Destroy(gameObject);
                        break;
                    }
                case PickupType.Carry: {
                        if (isCarried) { return; }
                        if (itemSlot) {
                            itemSlot.Release();
                            itemSlot = null;
                        }
                        sender.SetCarriedItem(this);
                        break;
                    }
            }
        }

        public void SetCarriedState(bool state) {
            if (isCarried && state) {
                return;
            }

            isCarried = state;
            rb.constraints = state ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
            rb.isKinematic = state;
            rb.detectCollisions = !state;
        }

        public void SetSlotted(ItemSlot slottedInto, Vector3 position, Quaternion rotation) {
            rb.isKinematic = true;
            itemSlot = slottedInto;
            rb.position = position;
            rb.rotation = rotation;
        }
        
        // Test whether the object can move towards the target, e.g. prevents objects going through walls.
        // Get the point on the object's collider faces, from the direction of the movement from the center of the object to the target position
        // public bool TryMoveTowards(Vector3 target) {

        // }
    }
}