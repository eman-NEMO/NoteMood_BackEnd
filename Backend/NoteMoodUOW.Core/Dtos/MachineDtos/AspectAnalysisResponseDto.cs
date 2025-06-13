using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.MachineDtos
{
    public class AspectAnalysisResponseDto
    {
        public Dictionary<string, AspectCategoryDto> aspects { get; set; }
    }
}
