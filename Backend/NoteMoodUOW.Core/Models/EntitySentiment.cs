using NoteMoodUOW.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    public class EntitySentiment
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public virtual Entity Entity { get; set; }
        public int SentimentId { get; set; }
        public virtual Sentiment Sentiment { get; set; }
        public int EntryId { get; set; }
        public virtual Entry Entry { get; set; }
    }
}
