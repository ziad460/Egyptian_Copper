using CopperFactory.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CopperFactory.ViewModels
{
    public class ProductDataVM
    {
        public int Product_Id { get; set; }
        public int? Model_ID { get; set; }
        public DateTime Day { get; set; }
        public bool DayStatus { get; set; }
        [Required]        
        public double Value { get; set; }
    }
}
