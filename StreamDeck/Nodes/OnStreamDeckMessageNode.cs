using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace Warudo.Plugins.StreamDeck.Nodes {
    [NodeType(Id = "cc63b0b0-194d-4d74-9e52-2531ae89fd22", Title = "ON_STREAM_DECK_MESSAGE", Category = "STREAM_DECK")]
    public class OnStreamDeckMessageNode : StreamDeckNode {

        [DataOutput]
        [Label("MESSAGE")]
        public string Message() => lastMessage;
        
        [FlowOutput]
        public Continuation Exit;
        
        public override void OnStreamDeckMessage(string receiverName, string message) {
            if (receiverName != ReceiverName) return;
            lastMessage = message;
            InvokeFlow(nameof(Exit));
        }
        
        private string lastMessage;

    }
}
