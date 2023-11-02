using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Utils;
using Warudo.Plugins.Core;
using Warudo.Plugins.StreamDeck.Nodes;
using WebSocketSharp.Server;

namespace Warudo.Plugins.StreamDeck {
    public class StreamDeckEventController {

        private const int ServicePort = 19069;
        private WebSocketServer server;
        private StreamDeckService service;
        
        private readonly HashSet<IStreamDeckEventHandler> handlers = new();

        public async Task<bool> Subscribe(IStreamDeckEventHandler handler) {
            if (server == null) {
                try {
                    var wsUri = WebSocketHelpers.CreateLocalHostUri(ServicePort);
                    await Context.PluginManager.GetPlugin<CorePlugin>().BeforeListenToPort();
                    
                    server = new WebSocketServer(wsUri);
                    server.AddWebSocketService<StreamDeckService>("/", it => {
                        it.Parent = this;
                        service = it;
                    });
                    server.Start();
                    Debug.Log("[StreamDeck] Started service at port " + ServicePort);
                    
                    Context.PluginManager.GetPlugin<CorePlugin>().AfterListenToPort();
                } catch (Exception e) {
                    Log.UserError("[StreamDeck] Failed to start service", e);
                    return false;
                }
            }
            handlers.Add(handler);
            return true;
        }

        public void Unsubscribe(IStreamDeckEventHandler handler) {
            handlers.Remove(handler);
            if (handlers.Count == 0) {
                Dispose();
            }
        }
        
        public void Dispose() {
            handlers.Clear();
            if (server != null) {
                server.Stop();
                Debug.Log("[StreamDeck] Stopped service");
            }
            server = null;
            service = null;
        }
        
        public void OnStreamDeckTrigger(string receiverName) {
            foreach (var handler in handlers) {
                handler.OnStreamDeckTrigger(receiverName);
            }
        }
        
        public void OnStreamDeckToggle(string receiverName) {
            EnsureTogglesConsistency(receiverName);
            foreach (var handler in handlers) {
                handler.OnStreamDeckToggle(receiverName);
            }
        }
        
        private void EnsureTogglesConsistency(string receiverName) {
            // Make sure all other toggles with the same receiver name have the same toggle state
            var toggles = Context.OpenedScene.GetGraphs().Values.SelectMany(it => it.GetNodes().Values).OfType<OnStreamDeckToggleNode>()
                .Where(it => it.ReceiverName == receiverName).ToList();
            if (toggles.Count == 0) return;
            var toggleState = toggles.First().ToggleState;
            foreach (var toggle in toggles) {
                toggle.ToggleState = toggleState;
                toggle.BroadcastDataInput(nameof(toggle.ToggleState));
            }
        }
        
        public void OnStreamDeckMessage(string receiverName, string message) {
            foreach (var handler in handlers) {
                handler.OnStreamDeckMessage(receiverName, message);
            }
        }
        
        public void NotifyToggle(string receiverName, bool state, bool isResponse = false) {
            service?.NotifyToggle(receiverName, state, isResponse);
        }
        
    }
}
