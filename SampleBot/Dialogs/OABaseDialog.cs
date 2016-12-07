using OAChatBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;
using OAChatBot.Extensions;

namespace OAChatBot.Dialogs
{
    [Serializable]
    public class OaBaseDialog : IDialog<string>
    {
        private const string OperationErrorMsg = "Error in performing the operation. Please try after some time.";

        private const string WelcomeMsg = "Hi!! {0} {1}.\n I can help you to: \n " +
                                          "* Order items from BB \n E.g.: type *Can I get 5 kgs of Ashirvad atta?* \n " +
                                          "* Repeat the previous ordered item. E.g. type **add again**  \n " +
                                          "* Type **status** to know your order status. \n " +
                                          "* Type **offers** to know the available offers. \n " +
                                          "* Type **return** to return back the purchased item. \n ";// +
                                           // "* Type **quit or exit** to exit from chat.";
        private const string QuitMsg = "Bye {0}. Thanks for using OAChatBot.";
        private bool _hasQuit = false;

        public async Task StartAsync(IDialogContext context)
        {
            var userName = "";

            try
            {
                var msg = context.MakeMessage();
                userName = msg.To.Name;

                var userCntx = new UserContext()
                {
                    UserId = userName
                };
                context.UserData.SetValue("userContext", userCntx);
            }
            catch (Exception)
            { }

            if (_hasQuit)
                _hasQuit = false;
            else
                await context.PostAsyncCustom(string.Format(WelcomeMsg, GetWishBasedOnTime(), userName));

            context.Wait(MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<Message> result)
        {
            try
            {
                var message = await result;
                context.PerUserInConversationData.SetValue("UserInput", message.Text);

                context.Call(new OAConversationLuisDialog(), ConversationComplete);
            }
            catch (Exception ex)
            {
                await context.PostAsyncCustom(OperationErrorMsg);
                context.Wait(MessageReceived);
            }
        }

        private async Task ConversationComplete(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var retResult = await result;

                if (retResult == null)
                {
                    await context.PostAsyncCustom(OperationErrorMsg);
                    context.Wait(MessageReceived);
                }
                else if (string.Compare(retResult, "quit", 0) == 0)
                {
                    UserContext userCntx = null;
                    context.UserData.TryGetValue("userContext", out userCntx);
                    await context.PostAsyncCustom(string.Format(QuitMsg, (userCntx != null) ? userCntx.UserId : ""));
                    //context.Done("quit");
                    context.Wait(MessageReceived);
                    _hasQuit = true;
                }
                else {
                    context.Wait(MessageReceived);
                }
            }
            catch (Exception ex)
            {
                await context.PostAsyncCustom(OperationErrorMsg);
                context.Wait(MessageReceived);
            }
        }

        private string GetWishBasedOnTime()
        {
            if (DateTime.Now.Hour < 12)
            {
                return "Good Morning";
            }
            return DateTime.Now.Hour < 17 ? "Good Afternoon" : "Good Evening";
        }
    }
}