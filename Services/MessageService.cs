using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GrpcMessageService.Services
{
    public class MessageService : MessageService.MessageServiceBase
    {
        private readonly ILogger<MessageService> _logger;
        private readonly IConnectionMultiplexer _redis;

        public MessageService(ILogger<MessageService> logger, IConnectionMultiplexer redis)
        {
            _logger = logger;
            _redis = redis;
        }

        public override async Task<Empty> SendMessage(MessageRequest request, ServerCallContext context)
        {
            var db = _redis.GetDatabase();
            await db.ListRightPushAsync("messages", request.Message);
            return new Empty();
        }

        public override async Task<MessageList> GetAllMessages(Empty request, ServerCallContext context)
        {
            var db = _redis.GetDatabase();
            var messages = await db.ListRangeAsync("messages");
            var response = new MessageList();
            response.Messages.AddRange(messages.Select(m => new Message { Content = m }));
            return response;
        }
    }
}

