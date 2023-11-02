namespace Warudo.Plugins.StreamDeck {
    public interface IStreamDeckEventHandler {
        
        public void OnStreamDeckTrigger(string receiverName);
        
        public void OnStreamDeckToggle(string receiverName);

        public void OnStreamDeckMessage(string receiverName, string message);
        
    }
}
