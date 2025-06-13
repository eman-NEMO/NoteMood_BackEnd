using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.MachineDtos
{
    public class DailySentimentDto
    {
        public DateOnly Date { get; set; }
        public string Sentiment { get; set; }
    }
}
