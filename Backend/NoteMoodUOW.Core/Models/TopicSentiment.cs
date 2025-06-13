using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    public class TopicSentiment
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        public virtual Topic Topic { get; set; }
        public int SentimentId { get; set; }
        public virtual Sentiment Sentiment { get; set; }
        public int EntryId { get; set; }
        public virtual Entry Entry { get; set; }
    }
}
