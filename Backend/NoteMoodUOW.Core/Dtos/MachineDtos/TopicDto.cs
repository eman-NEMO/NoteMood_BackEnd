using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.MachineDtos
{
    public class TopicDto
    {
        public string TopicName { get; set; }
        public int Frequency { get; set; }
        public double Percentage { get; set; }
        public List<SentimentPercentageDto> Sentiments { get; set; }
    }
}
