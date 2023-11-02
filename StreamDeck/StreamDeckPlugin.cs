using Warudo.Core.Attributes;
using Warudo.Core.Plugins;
using Warudo.Plugins.StreamDeck.Nodes;

namespace Warudo.Plugins.StreamDeck {
    [PluginType(
        Id = "Warudo.StreamDeck",
        Name = "STREAM_DECK",
        Description = "STREAM_DECK_DESCRIPTION",
        Author = "Hakuya Labs",
        Version = "WARUDO_VERSION",
        NodeTypes = new[] {
            typeof(OnStreamDeckTriggerNode),
            typeof(OnStreamDeckToggleNode),
            typeof(OnStreamDeckMessageNode)
        })]
    public class StreamDeckPlugin : Plugin {
        
        public StreamDeckEventController EventController { get; } = new();

        protected override void OnDestroy() {
            base.OnDestroy();
            EventController.Dispose();
        }
        
    }
}
