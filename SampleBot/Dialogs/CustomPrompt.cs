//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Threading.Tasks;
//using OAChatBot.Extensions;
//using OAChatBot.Luis;
//using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Builder.Internals.Fibers;
//using Microsoft.Bot.Connector;

//namespace OAChatBot.Dialogs
//{
//    [Serializable]
//    public class CustomPromptDialog
//    {
//        public static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, IEnumerable<T> options,
//            string prompt, string retry = null, int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto)
//        {
//            var child = new CustomPromptChoice<T>(options, prompt, retry, attempts);
//            context.Call<T>(child, resume);
//        }
//    }

//    [Serializable]
//    public class CustomPromptChoice<T> : CustomPrompt<T, T>
//    {
//        public CustomPromptChoice(IEnumerable<T> options, string prompt, string retry, int attempts, PromptStyle promptStyle = PromptStyle.Auto)
//            : this(new PromptOptions<T>(prompt, retry, options: options.ToList(), attempts: attempts, promptStyler: new PromptStyler(promptStyle)))
//        {
//        }

//        public CustomPromptChoice(PromptOptions<T> promptOptions)
//            : base(promptOptions)
//        {
//            SetField.CheckNull(nameof(promptOptions.Options), promptOptions.Options);
//        }


//        protected async override Task<bool> TryParse(Message message)
//        {
//            if (!string.IsNullOrWhiteSpace(message.Text))
//            {
//                string applicationId = ConfigurationManager.AppSettings["LuisBBCommonApplicationId"];

//                var luisClient = LuisClient.Create(applicationId);
//                var luisResponse = await luisClient.SendQuery(message.Text);

//                Intent intent = null;

//                if (luisResponse.TryFindIntent("RefundOption", out intent))
//                {
//                    var entity = luisResponse.Entities.FirstOrDefault();
//                    result = this.promptOptions.Options.FirstOrDefault((opt) => opt.ToString().Contains(entity != null ? entity.Value : ""));
//                    return true;
//                }
//            }

//            result = default(T);
//            return false;
//        }
//    }

//    [Serializable]
//    public class CustomPrompt<T, U> : IDialog<T>
//    {
//        protected readonly PromptOptions<U> promptOptions;

//        public CustomPrompt(PromptOptions<U> promptOptions)
//        {
//            SetField.NotNull(out this.promptOptions, nameof(promptOptions), promptOptions);

//        }

//        async Task IDialog<T>.StartAsync(IDialogContext context)
//        {
//            await context.PostAsync(this.MakePrompt(context, promptOptions.Prompt, promptOptions.Options));
//            context.Wait(MessageReceived);
//        }

//        private async Task MessageReceived(IDialogContext context, IAwaitable<Message> message)
//        {
//            T result;
//            if (this.TryParse(await message, out result))
//            {
//                context.Done(result);
//            }
//            else
//            {
//                --promptOptions.Attempts;
//                if (promptOptions.Attempts > 0)
//                {
//                    await context.PostAsync(this.MakePrompt(context, promptOptions.Retry ?? promptOptions.DefaultRetry, promptOptions.Options));
//                    context.Wait(MessageReceived);
//                }
//                else
//                {
//                    //too many attempts, throw.
//                    await context.PostAsync(this.MakePrompt(context, promptOptions.TooManyAttempts));
//                    throw new Exception(promptOptions.TooManyAttempts);
//                }
//            }
//        }

//        protected virtual async Task<T> Parse(Message message, out T result)
//        {
//            return await Task.FromResult(default(T));
//        }

//        protected virtual Message MakePrompt(IDialogContext context, string prompt, IList<U> options = null)
//        {
//            var msg = context.MakeMessage();
//            if (options != null && options.Count > 0)
//            {
//                promptOptions.PromptStyler.Apply(ref msg, prompt, options);
//            }
//            else
//            {
//                promptOptions.PromptStyler.Apply(ref msg, prompt);
//            }
//            return msg;
//        }
//    }
//}