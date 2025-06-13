using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    public class Aspect
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public virtual ICollection<Entity> Entities { get; set; }

    }
}
