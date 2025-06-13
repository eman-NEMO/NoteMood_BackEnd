using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.MachineDtos
{
    public class EntityDto
    {
        public string EntityName { get; set; }
        public List<SentimentPercentageDto> Sentiments { get; set; }
    }
}
