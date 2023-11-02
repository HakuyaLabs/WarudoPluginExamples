using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace Warudo.Plugins.StreamDeck.Nodes {
    [NodeType(Id = "25fea368-e2e4-486b-91ee-aab4350b39f5", Title = "ON_STREAM_DECK_TRIGGER", Category = "STREAM_DECK")]
    public class OnStreamDeckTriggerNode : StreamDeckNode {
        
        [FlowOutput]
        public Continuation Exit;

        public override void OnStreamDeckTrigger(string receiverName) {
            if (receiverName != ReceiverName) return;
            InvokeFlow(nameof(Exit));
        }

    }
}
