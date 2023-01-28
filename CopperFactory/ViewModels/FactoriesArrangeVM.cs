using CopperFactory.Models;

namespace CopperFactory.ViewModels
{
    public enum Status
    {
        UP = 0,
        Down = 1,
        NoChange = 2,
    }
    public class FactoriesArrangeVM
    {
        public Factory Factory { get; set; }
        public double ProductionVolume { get; set; }
        public Status Status { get; set; }

    }
}
