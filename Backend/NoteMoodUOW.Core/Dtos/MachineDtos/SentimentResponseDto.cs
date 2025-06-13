using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.MachineDtos
{
    public class SentimentResponseDto
    {
        public Dictionary<string, AspectAnalysisResponseDto> aspects { get; set; }
    }
}
