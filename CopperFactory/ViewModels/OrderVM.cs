using CopperFactory.Models;

namespace CopperFactory.ViewModels
{
    public class OrderVM
    {
        public int Product_ID { get; set; }
        public int Customer_ID { get; set; }
        public double OrderValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool OrderStatue { get; set; }
    }
}
