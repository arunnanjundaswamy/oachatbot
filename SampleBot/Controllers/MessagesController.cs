using System.Threading.Tasks;
using System.Web.Http;
using OAChatBot.MessageHandler;
using Microsoft.Bot.Connector;

namespace OAChatBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly OaBotMessageHandler _messageHandler;

        public MessagesController()
        {
            _messageHandler = new OaBotMessageHandler();
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message incomingMessage)
        {
            return await _messageHandler.Act(incomingMessage);
        }

    }
}