//using Microsoft.Bot.Builder.Dialogs;
//using OAChatBot.Luis;
//using Microsoft.Bot.Connector;

//namespace OAChatBot.Dialogs
//{
//    public class OADialogManager
//    {
//        //private const string PreviousIntent = "PreviousIntent";

//        public static IDialog<object> GetBBDialog(Message message, LuisResponse luisResponse)
//        {
//            if (luisResponse == null)
//            {
//               // message.CreateReplyMessage("I am a simple echo dialog with a counter!Reset my counter by typing \"reset\"!");
//                return null;
//            }

//            Intent winner = luisResponse.Winner();

//            if (winner.Name.Equals("AddToCart") || winner.Name.Equals("RepeatOrder"))
//            {
//                //message.SetBotUserData(PreviousIntent, winner.Name);
//                return new BBConversationDialog(luisResponse);
//            }

//            //message.CreateReplyMessage("I am a simple echo dialog with a counter!Reset my counter by typing \"reset\"!");
//            return null;
//        }



//        //public static readonly async IDialog<string> dialogs =
//        //    Chain
//        //        .PostToChain()
//        //         .Select(msg => msg.Text)
//        //        .Switch(
//        //            new Case<string, IDialog<object>>((txt) =>
//        //            {
//        //                _luisClient = LuisClient.Create();
//        //                _luisResponse = await _luisClient.SendQuery(txt);
//        //                var winner = _luisResponse.Winner();

//        //                return (winner == null || winner.IsNone()) && (winner.Name.Equals("CreateOrder"));

//        //            },
//        //            (ctx, msg) =>
//        //            {
//        //                return Chain.From(() => new BBConversationDialog(_luisResponse));
//        //            }

//        //    )

//        //new DefaultCase<Message, IDialog<object>>((ctx, msg) => { return new BBConversationDialog(luisResponse); ; }));


//        //return Chain.PostToChain()
//        //    .Switch(

//        //        Chain.Case((msg) => { return true; }, (cntx) => { new BBConversationDialog(); })
//        //    );




//    }

//}