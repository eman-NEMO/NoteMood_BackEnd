using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.MachineDtos
{
    public class AspectDto
    {
        public string AspectName { get; set; }
        public List<EntityDto> Entities { get; set; }
    }
}
