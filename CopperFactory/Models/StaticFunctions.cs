using CopperFactory.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CopperFactory.Models
{
    public class StaticFunctions
    {
        private readonly IUnityOfWork unityOfWork;

        public StaticFunctions(IUnityOfWork unityOfWork)
        {
            this.unityOfWork = unityOfWork;
        }
        public double CalculateSum(List<Production> _production)
        {
            double sum = 0;
            foreach (var item in _production)
            {
                sum += item.Quantity_Producted;
            }
            return sum;
        }

        public async Task<double> Orders_Percentages(DateTime fromDate , DateTime toDate , int id)    //Factory ID
        {
            var StartDate = new DateTime(fromDate.Year, fromDate.Month, 1);
            var EndDate = new DateTime(toDate.Year, toDate.Month, 31);
            double ForcastingSummision = 0;
            double OrdersSummision = 0;

            List<Product>? FactoryProducts = await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id, new[] { "Forcastings" });
            var orders = await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id && x.Start_Date >= fromDate && x.Start_Date < toDate , new[] { "OrderDetails" });
            
            if (FactoryProducts.Count == 0)
                return 0;
            
            ForcastingSummision = FactoryProducts.Sum(x => x.Forcastings.Where(x => x.IsDeleted != true && x.Forcasting_Date >= StartDate && x.Forcasting_Date < EndDate).Sum(x => x.Quantity_Forcasted));
            OrdersSummision = orders.Sum(x => x.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value));

            if (ForcastingSummision == 0)
                ForcastingSummision++;

            return Math.Round((OrdersSummision * 100) / ForcastingSummision);
        }
        public async Task<double> Production_Percentages(DateTime fromDate , DateTime toDate , int id)
        {
            double ProductionSummision = 0;
            double OrdersSummision = 0;

            List<Product>? FactoryProducts = await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id, new[] { "Productions" });
            var orders = await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id && x.Start_Date >= fromDate && x.Start_Date < toDate, new[] { "OrderDetails" });

            if (FactoryProducts == null)
                return 0;

            ProductionSummision = FactoryProducts.Sum(x => x.Productions.Where(x => x.IsDeleted != true && x.Production_Date >= fromDate && x.Production_Date < toDate).Sum(x => x.Quantity_Producted));
            OrdersSummision = orders.Sum(x => x.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value));

            if (OrdersSummision == 0)
                OrdersSummision++;

            return Math.Round((ProductionSummision * 100) / OrdersSummision);
        }

        public async Task<double> Production_Percentages_PerDay(int id)
        {
            double ProductionSummision = 0;
            double ForcastingSummision = 0;

            List<Product>? FactoryProducts = await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id, new[] { "Productions" , "Forcastings" });
            
            if (FactoryProducts == null)
                return 0;

            ProductionSummision = FactoryProducts.Sum(x => x.Productions.Where(x => x.IsDeleted != true && x.Production_Date.ToShortDateString() == DateTime.Now.AddDays(-1).ToShortDateString()).Sum(x => x.Quantity_Producted));
            ForcastingSummision = FactoryProducts.Sum(x => x.Forcastings.Where(x => x.IsDeleted != true && x.Forcasting_Date.Month == DateTime.Now.Month && x.Forcasting_Date.Year == DateTime.Now.Year).Sum(x => x.Quantity_Forcasted_PerDay));

            if (ForcastingSummision == 0)
                ForcastingSummision++;

            return Math.Round((ProductionSummision * 100) / ForcastingSummision);
        }

        public async Task<double> Sales_Percentages(DateTime fromDate , DateTime toDate , int id)
        {
            double OrdersSummision = 0;
            double OutsSummision = 0;

            List<Product>? FactoryProducts = await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id, new[] { "Inventory_Outs" });

            var orders = await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id && x.Start_Date >= fromDate && x.Start_Date < toDate, new[] { "OrderDetails" });
            
            if (FactoryProducts.Count == 0)
                return 0;

            
            OrdersSummision = orders.Sum(x => x.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value));
            OutsSummision = FactoryProducts.Sum(x => x.Inventory_Outs.Where(x => x.IsDeleted != true && x.DateTime >= fromDate && x.DateTime < toDate && x.Order_ID != null).Sum(x => x.Quantity_Sold));

            if (OrdersSummision == 0)
                OrdersSummision++;

            return Math.Round((OutsSummision * 100) / OrdersSummision);
        }

        public async Task<double> AllOrders_Percentages()    //Factory ID
        {
            double ForcastingSummision = 0;
            double OrdersSummision = 0;

            var forcastings = await unityOfWork.Forcasting.FindAllAsync(x => x.IsDeleted != true && x.Forcasting_Date.Month == DateTime.Now.Month && x.Forcasting_Date.Year == DateTime.Now.Year);
            var orders = await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Start_Date.Month == DateTime.Now.Month && x.Start_Date.Year == DateTime.Now.Year, new[] { "OrderDetails" });

            ForcastingSummision = forcastings.Sum(x => x.Quantity_Forcasted);
            OrdersSummision = orders.Sum(x => x.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value));

            if (ForcastingSummision == 0)
                ForcastingSummision++;

            return Math.Round((OrdersSummision * 100) / ForcastingSummision);
        }

        public async Task<double> AllProduction_Percentages()
        {
            double ProductionSummision = 0;
            double OrdersSummision = 0;

            var productions = await unityOfWork.Production.FindAllAsync(x => x.IsDeleted != true && x.Production_Date.Month == DateTime.Now.Month && x.Production_Date.Year == DateTime.Now.Year);
            var orders = await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Start_Date.Month == DateTime.Now.Month && x.Start_Date.Year == DateTime.Now.Year , new[] { "OrderDetails" });

            ProductionSummision = productions.Sum(x => x.Quantity_Producted);
            OrdersSummision = orders.Sum(x => x.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value));

            if (OrdersSummision == 0)
                OrdersSummision++;

            return Math.Round((ProductionSummision * 100) / OrdersSummision);
        }

        public async Task<double> AllProduction_Percentages_PerDay()
        {
            double ProductionSummision = 0;
            double ForcastingSummision = 0;

            var productions = await unityOfWork.Production.FindAllAsync(x => x.IsDeleted != true && x.Production_Date.Year == DateTime.Now.Year
             && x.Production_Date.Month == DateTime.Now.Month
              && x.Production_Date.Day == DateTime.Now.AddDays(-1).Day);

            var forcastings = await unityOfWork.Forcasting.FindAllAsync(x => x.IsDeleted != true && x.Forcasting_Date.Month == DateTime.Now.Month && x.Forcasting_Date.Year == DateTime.Now.Year);

            ProductionSummision = productions.Sum(x => x.Quantity_Producted);
            ForcastingSummision = forcastings.Sum(x => x.Quantity_Forcasted_PerDay);

            if (ForcastingSummision == 0)
                ForcastingSummision++;

            return Math.Round((ProductionSummision * 100) / ForcastingSummision);
        }

        public async Task<double> AllSales_Percentages()
        {
            double OrdersSummision = 0;
            double OutsSummision = 0;

            var outs = await unityOfWork.Inventory_Out.FindAllAsync(x => x.IsDeleted != true && x.DateTime.Month == DateTime.Now.Month && x.DateTime.Year == DateTime.Now.Year && x.Order_ID != null);
            var orders = await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Start_Date.Month == DateTime.Now.Month && x.Start_Date.Year == DateTime.Now.Year, new[] { "OrderDetails" });

            OrdersSummision = orders.Sum(x => x.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value));
            OutsSummision = outs.Sum(x => x.Quantity_Sold);

            if (OrdersSummision == 0)
                OrdersSummision++;

            return Math.Round((OutsSummision * 100) / OrdersSummision);
        }
    }
}
