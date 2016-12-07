using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace OAChatBot.Models
{
    [Serializable]
    public class BbConversation
    {
        public string Conversationid { get; set; }
        public string Userinput { get; set; }
        public string Botoutput { get; set; }
        public string Userid { get; set; }
        public DateTime? Createddt { get; set; }
        public DateTime? Updateddt { get; set; }
    }
}