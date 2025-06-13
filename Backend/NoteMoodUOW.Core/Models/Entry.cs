using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    public class Entry
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly Time { get; set; }
        //[ForeignKey("SentimentId")]
        public int SentimentId { get; set; }

        //public string? OverallSentiment { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual Sentiment Sentiment { get; set; }
        public virtual ICollection<EntitySentiment> EntitySentiments { get; set; }
        public virtual ICollection<TopicSentiment> TopicSentiments { get; set; }

    }
}
