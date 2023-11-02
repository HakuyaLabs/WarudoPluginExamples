using System;
using System.Linq;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace Warudo.Plugins.StreamDeck.Nodes {
    [NodeType(Id = "8128e51d-cc23-4167-9402-6b5e55b97766", Title = "ON_STREAM_DECK_TOGGLE", Category = "STREAM_DECK")]
    public class OnStreamDeckToggleNode : StreamDeckNode {

        [DataInput]
        [Label("TOGGLE_STATE")]
        public bool ToggleState;
        
        [FlowOutput]
        [Label("IF_YES")]
        public Continuation IfYes;
        
        [FlowOutput]
        [Label("IF_NO")]
        public Continuation IfNo;
        
        public override void OnStreamDeckToggle(string receiverName) {
            if (receiverName != ReceiverName) return;
            try {
                ToggleState = !ToggleState;
                BroadcastDataInput(nameof(ToggleState));
                EventController.NotifyToggle(ReceiverName, ToggleState, true);
                InvokeFlow(ToggleState ? nameof(IfYes) : nameof(IfNo));
            } catch (Exception e) {
                Debug.Log(e);
            }
        }

        protected override void OnCreate() {
            base.OnCreate();
            Watch(nameof(ReceiverName), EnsureTogglesConsistency);
            Watch(nameof(ToggleState), () => {
                EventController.NotifyToggle(ReceiverName, ToggleState);
                EnsureTogglesConsistency();
            });
        }

        private void EnsureTogglesConsistency() {
            // Make sure all other toggles with the same receiver name have the same toggle state
            var toggles = Context.OpenedScene.GetGraphs().Values.SelectMany(it => it.GetNodes().Values).OfType<OnStreamDeckToggleNode>()
                .Where(it => it.ReceiverName == ReceiverName && it != this);
            foreach (var toggle in toggles) {
                toggle.ToggleState = ToggleState;
                toggle.BroadcastDataInput(nameof(ToggleState));
            }
        }

    }
}
