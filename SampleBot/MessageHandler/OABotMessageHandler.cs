using OAChatBot.Base.MessageHandler;
using OAChatBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using OAChatBot.Extensions;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace OAChatBot.MessageHandler
{

    public class OaBotMessageHandler : BaseMessageHandler
    {
        public override async Task<Message> OnMessage()
        {
            Message.SetBotPerUserInConversationData("UserInput", Message.Text);

            //if ((string.Compare(Message.Text, "quit", StringComparison.OrdinalIgnoreCase) == 0) ||
            //    (string.Compare(Message.Text, "exit", StringComparison.OrdinalIgnoreCase) == 0))
            //{
            //    return await Conversation.SendAsync(Message, () => new BbBaseDialog());
            //}

            VersionManager(Message);

            var message = await Conversation.SendAsync(Message, MakeRoot);
            await message.LogMessage();

            return message;
        }

        private void VersionManager(Message message)
        {

            string currentBotVersion = "2.0";
            string botVersionKey = "BotVersion";

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
            {
                var botData = scope.Resolve<IBotData>();

                //botData.PerUserInConversationData.RemoveValue(DialogModule.BlobKey);

                string version = string.Empty;
                if (botData.PerUserInConversationData.TryGetValue(botVersionKey, out version))
                {
                    // remove the dialog stack data if version is different
                    // data migrations can happen here.
                    if (version != currentBotVersion)
                    {
                        botData.PerUserInConversationData.RemoveValue(DialogModule.BlobKey);
                        botData.PerUserInConversationData.SetValue(botVersionKey, currentBotVersion);
                    }
                }
                else
                {
                    botData.PerUserInConversationData.SetValue(botVersionKey, currentBotVersion);
                }
            }
        }

        internal static IDialog<string> MakeRoot()
        {
            return Chain.From(() => new OAConversationLuisDialog());
        }

        public override Task<Message> Ping()
        {
            throw new NotImplementedException();
        }

        public override Task<Message> DeleteUserData()
        {
            throw new NotImplementedException();
        }

        public override async Task<Message> BotAddedToConversation()
        {
            return await Task.Run(() => Message.CreateReplyMessage("BotAddedToConversation"));
        }

        public override Task<Message> BotRemovedFromConversation()
        {
            throw new NotImplementedException();
        }

        public override async Task<Message> UserAddedToConversation()
        {
            return await Task.Run(() => Message.CreateReplyMessage("UserAddedToConversation"));
        }

        public override async Task<Message> UserRemovedFromConversation()
        {
            return await Task.Run(() => Message.CreateReplyMessage("UserRemovedFromConversation"));
        }

        public override async Task<Message> EndOfConversation()
        {
            return await Task.Run(() => Message.CreateReplyMessage("EndOfConversation"));
        }
    }

}