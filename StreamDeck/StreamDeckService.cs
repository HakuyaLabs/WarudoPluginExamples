using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Warudo.Core.Server;
using Warudo.Core.Utils;
using Warudo.Plugins.StreamDeck.Nodes;

namespace Warudo.Plugins.StreamDeck {
    public class StreamDeckService : WebSocketService {
        
        public StreamDeckEventController Parent { get; set; }

        protected override async UniTask<bool> HandleAction(string action, JObject data) {
            await UniTask.SwitchToMainThread();
            switch (action) {
                case "trigger":
                    Parent.OnStreamDeckTrigger(data["receiverName"].Value<string>());
                    return true;
                case "toggle":
                    Parent.OnStreamDeckToggle(data["receiverName"].Value<string>());
                    return true;
                case "message":
                    if (!data.ContainsKey("message")) return false;
                    Parent.OnStreamDeckMessage(data["receiverName"].Value<string>(), data["message"].Value<string>());
                    return true;
                case "getReceivers":
                    GetReceivers(data["type"].Value<string>());
                    return true;
                case "getToggles":
                    GetToggles();
                    return true;
            }
            return false;
        }

        private List<string> GetReceiverNames<T>() where T : StreamDeckNode {
            return Warudo.Core.Context.OpenedScene.GetGraphs().Values.SelectMany(it => it.GetNodes().Values).OfType<T>().Select(it => it.ReceiverName).Where(it => !it.IsNullOrWhiteSpace()).Distinct().ToList();
        }

        protected void GetReceivers(string type) {
            Respond("getReceivers", type switch {
                "trigger" => GetReceiverNames<OnStreamDeckTriggerNode>(),
                "toggle" => GetReceiverNames<OnStreamDeckToggleNode>(),
                "message" => GetReceiverNames<OnStreamDeckMessageNode>(),
                _ => new List<string>()
            });
        }

        protected void GetToggles() {
            Respond("getToggles", Warudo.Core.Context.OpenedScene.GetGraphs().Values.SelectMany(it => it.GetNodes().Values).OfType<OnStreamDeckToggleNode>()
                .Select(it => (it.ReceiverName, it.ToggleState)).Where(it => !it.ReceiverName.IsNullOrWhiteSpace()).Distinct().ToDictionary(it => it.ReceiverName, it => it.ToggleState));
        }
        
        public void NotifyToggle(string receiverName, bool state, bool isResponse) {
            Broadcast("toggle", new {receiverName, state, isResponse});
        }

    }
}
