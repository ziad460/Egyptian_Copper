using System.ComponentModel.DataAnnotations;

namespace CopperFactory.ViewModels
{
    public class InventoryVM
    {
        public int ID { get; set; }
        public int? Product_Id { get; set; }
        public int? Model_ID { get; set; }
        public DateTime Date { get; set; }
        public bool DayStatus { get; set; }
        public double Value_Stored { get; set; }
        public double productionValue { get; set; }
        public double DidNotStore { get; set; }
    }
}
