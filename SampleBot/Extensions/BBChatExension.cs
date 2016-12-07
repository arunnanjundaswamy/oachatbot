using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using OAChatBot.Models;
using OAChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Resource;
using Microsoft.Bot.Connector;

namespace OAChatBot.Extensions
{
    public static class BbChatExension
    {
        public static string GetWishBasedOnTime(this DateTime dateTime)
        {
            DateTime ist = TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

            if (ist.Hour < 12)
            {
                return "Good Morning";
            }
            return ist.Hour < 17 ? "Good Afternoon" : "Good Evening";
        }

        public static DateTime GetIst(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        }

        public static async Task PostAsyncCustom(this IDialogContext context, string botMessage)
        {
            try
            {
                var cntxMessage = context.MakeMessage();
                var userInput = "";
                context.PerUserInConversationData.TryGetValue("UserInput", out userInput);

                var botConv = new BbConversation()
                {
                    Userid = cntxMessage.To.Name,
                    Conversationid = cntxMessage.ConversationId,
                    Botoutput = botMessage,
                    Userinput = userInput,
                    Createddt = DateTime.Now,
                    Updateddt = DateTime.Now
                };

                await ServiceHandler.TraceMessage(botConv);

                await context?.PostAsync(botMessage);
            }
            catch (Exception)
            {
                await context?.PostAsync(botMessage);
            }
        }

        public static async Task LogMessage(this Message message)
        {
            try
            {
                if (message == null) return;

                var cntxMessage = message;
                var userInput = message.GetBotPerUserInConversationData<string>("UserInput");

                if (userInput == null) return;

                var botConv = new BbConversation()
                {
                    Userid = cntxMessage.To.Name,
                    Conversationid = cntxMessage.ConversationId,
                    Botoutput = message.Text,
                    Userinput = userInput,
                    Createddt = DateTime.Now,
                    Updateddt = DateTime.Now
                };

                await ServiceHandler.TraceMessage(botConv);

            }
            catch (Exception)
            {
            }
        }

        public static bool ContainsYes(this string yesString)
        {
            return (Resources.MatchYes.SplitList().Any((txt)=>txt.ToLower().Contains(yesString.ToLower())));
        }

        public static bool ContainsNo(this string noString)
        {
            return (Resources.MatchNo.SplitList().Any((txt) => txt.ToLower().Contains(noString.ToLower())));
        }

        public static bool TryFindType(this LuisResult luisResult, string typeName, out EntityRecommendation recommendation)
        {
            try
            {
                recommendation = null;

                if (luisResult == null || typeName == null || luisResult.Entities == null)
                {
                    return false;
                }

                foreach (var entityRecommendation in luisResult.Entities)
                {
                    if (String.Compare(entityRecommendation.Type, typeName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        recommendation = entityRecommendation;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                recommendation = null;
                return false;
            }
            return false;
        }
    }
}