using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.MachineDtos
{
    public class DailySentimentFreqDto
    {
        public int Count { get; set; }
        public Double Percentage { get; set; }
        public string Sentiment { get; set; }
        
    }
}
