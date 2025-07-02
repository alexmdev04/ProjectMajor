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
        public ItemSlot itemSlot { get; private set; }
        public bool recentlySlotted;
        [SerializeField] private string prompt = "Pickup";
        [SerializeField] private string slottedPrompt = "Unslot";
        [SerializeField] private GameObject triggerObject;
        private LayerMask triggerLayer;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
            triggerLayer = LayerMask.NameToLayer("Trigger");
            if (!triggerObject) { Log.Warning("[Item] " + name + " has no trigger object assigned."); }
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

            //if (triggerObject) { triggerObject.layer = state ? 0 : triggerLayer; }
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

        private void OnDestroy() {
            if (itemSlot) {
                itemSlot.Release(false);
            }
        }

        public override string GetPrompt() {
            return itemSlot ? slottedPrompt : prompt;
        }

    }
}