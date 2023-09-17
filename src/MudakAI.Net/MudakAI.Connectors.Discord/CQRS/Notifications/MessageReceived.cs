using Discord.WebSocket;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MudakAI.Connectors.Discord.CQRS.Notifications
{
    public sealed class MessageReceivedNotification : INotification
    {
        public MessageReceivedNotification(SocketMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public SocketMessage Message { get; }
    }

    public abstract class MessageReceivedNotificationHandlerBase : INotificationHandler<MessageReceivedNotification>
    {
        public abstract Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken);
    }
}
