using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Core.Localization;

namespace Warudo.Plugins.StreamDeck.Nodes {
    public abstract class StreamDeckNode : Node, IStreamDeckEventHandler {

        [Markdown(order: -1000)]
        [HiddenIf(nameof(HideStatus))]
        public string Status;

        protected bool HideStatus() => Status == null;
        
        [DataInput(order: -999)]
        [Label("RECEIVER_NAME")]
        public string ReceiverName;
        
        protected StreamDeckEventController EventController => Context.PluginManager.GetPlugin<StreamDeckPlugin>().EventController;

        protected override async void OnCreate() {
            base.OnCreate();
            if (!await EventController.Subscribe(this)) {
                Status = "FAILED_TO_START_STREAM_DECK_SERVICE".Localized();
                BroadcastDataInput(nameof(Status));
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            EventController.Unsubscribe(this);
        }

        public virtual void OnStreamDeckTrigger(string receiverName) {
        }
        
        public virtual void OnStreamDeckToggle(string receiverName) {
        }
        
        public virtual void OnStreamDeckMessage(string receiverName, string message) {
        }
    }
}
