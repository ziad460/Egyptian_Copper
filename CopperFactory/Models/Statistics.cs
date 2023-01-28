using System.ComponentModel.DataAnnotations.Schema;

namespace CopperFactory.Models
{
    public class Statistics
    {
        public int ID { get; set; }

        public int FactoryID { get; set; }
        public double Factory_Production_Percentage { get; set; }
        public double Factory_Sales_Percentage { get; set; }
        public double Factory_Orders_Percentage { get; set; }

        [ForeignKey("FactoryID")]
        public virtual Factory Factory { get; set; }
    }
}
