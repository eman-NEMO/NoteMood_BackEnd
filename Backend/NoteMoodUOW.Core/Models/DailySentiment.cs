using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    public class DailySentiment
    {
        public DateOnly Date { get; set; }
        public string ApplicationUserId { get; set; }

        public string Sentiment { get; set; }

        public double Percentage { get; set; }
        public ApplicationUser? User { get; set; }



    }
}
