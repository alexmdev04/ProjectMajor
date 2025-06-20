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

        public Rigidbody rb { get; private set; }
        private ItemSlot itemSlot;
        public bool recentlySlotted;

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
                        if (isCarried) {
                            break;
                        }
                        if (itemSlot) {
                            itemSlot.Release();
                            itemSlot = null;
                            break;
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
            rb.useGravity = !state;
        }

        public void OnSlotted(ItemSlot slottedInto) {
            itemSlot = slottedInto;
            Player.instance.DropCarriedItem();
            SetSlottedState(true);
        }

        public void OnUnslotted() {
            itemSlot = null;
            // Player.instance.SetCarriedItem(item);
            SetSlottedState(false);
        }

        private void SetSlottedState(bool state) {
            rb.isKinematic = state;
            recentlySlotted = state;
        }
    }
}