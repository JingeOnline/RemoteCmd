
using Microsoft.AspNetCore.SignalR;

namespace SignalrServer
{
    public class UseInputService : BackgroundService
    {
        private readonly IHubContext<MessageHub> _hubContext;
        public UseInputService(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => {
                while (true)
                {
                    string input = Console.ReadLine();
                    _hubContext.Clients.All.SendAsync("ClientReceiveMessage", input);
                }
            });
            return Task.CompletedTask;       
        }
    }
}
