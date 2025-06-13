using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    public class Sentiment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public  ICollection<Entry> Entries { get; set; }
        public virtual ICollection<EntitySentiment> EntitySentiments { get; set; }
        public virtual ICollection<Entry> Entries { get; set; }
        public virtual ICollection<TopicSentiment> TopicSentiments { get; set; }
    }
}
