using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AspectId { get; set; }
        public virtual Aspect Aspect { get; set; }
        public virtual ICollection<EntitySentiment> EntitySentiments { get; set; }
    }
}
