using CLM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Localization;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Printing;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace CopperFactory.Models
{
    public enum Function
    {
        Edit = 0,
        Add = 1,
        Remove = 2,
    }
    [Table("Zone")]
    public class Zone:BaseEntity
    {
        [Required(ErrorMessage ="This field is required")]
        public string Arabic_Name { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string English_Name { get; set; }
        public virtual ICollection<Factory>? Factories { get; set; }
    }
    [Table("Product")]
    public class Product:BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]
        public string Arabic_Name { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string English_Name { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public int Factory_ID { get; set; }
        public double Inventory_Total_Amount { get; set; }
        public double Out_Inventory_Total_Amount { get; set; }

        [ForeignKey("Factory_ID")]
        public virtual Factory? Factory { get; set; }
        public virtual ICollection<Inventory_IN>? Inventory_INs { get; set; }
        public virtual ICollection<Inventory_Out>? Inventory_Outs { get; set; }
        public virtual ICollection<OrderDetails>? OrderDetails { get; set; }
        public virtual ICollection<Forcasting>? Forcastings { get; set; }
        public virtual ICollection<Production>? Productions { get; set; }
    }
    [Table("Customer")]
    public class Customer:BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Address { get; set; }
        [DataType(DataType.PhoneNumber), Required(ErrorMessage = "This field is required") , RegularExpression("^1[0-2,5]{1}[0-9]{8}$" , ErrorMessage ="The phone number is invalid be sure it start with 1")]
        public long PhoneNumber { get; set; }
        
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Inventory_Out>? Inventory_Outs { get; set; }
    }
    [Table("Order")]
    public class Order:BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]
        public int Customer_ID { get; set; }

        [DataType(DataType.Date) , Required(ErrorMessage = "This field is required")]
        public DateTime Start_Date { get; set; }

        [Remote(action: "CheckEndDate", controller: "WorkOrder", AdditionalFields = "Start_Date", ErrorMessage = "End date mustn't be befor start date")]
        [DataType(DataType.Date) , Required(ErrorMessage = "This field is required")]
        public DateTime End_Date { get; set; }
        public int Factory_ID { get; set; }
        public bool OrderStatus { get; set; }
        public double Total_Products_Quantity { get; set; }

        [ForeignKey("Factory_ID")]
        public virtual Factory? Factory { get; set; }
        [ForeignKey("Customer_ID")]
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<OrderDetails>? OrderDetails { get; set; }
        public virtual ICollection<Inventory_Out>? Inventory_Outs { get; set; }
    }
    [Table("OrderDetails")]
    public class OrderDetails:BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]
        public int Order_ID { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public int Product_ID { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public double value { get; set; }
        public bool Delivery_Status { get; set; }

        [ForeignKey("Order_ID")]
        public virtual Order? Order { get; set; }
        [ForeignKey("Product_ID")]
        public virtual Product? Product { get; set; }
    }
    
    [Table("Factory")]
    public class Factory:BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]   
        public string English_Name { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Arabic_Name { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public double Max_Capacity { get; set; }
        public int Zone_ID { get; set; }

        [ForeignKey("Zone_ID")]
        public virtual Zone? Zone { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
        public virtual Statistics? StaticFunctions { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
    
    [Table("Production")]
    public class Production:BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]
        public int Product_ID { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public double Quantity_Producted { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "This field is required")]
        public DateTime Production_Date { get; set; }

        [ForeignKey("Product_ID")]
        public virtual Product? Product { get; set; }
    }
    
    [Table("Forcasting")]
    public class Forcasting :BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]
        public int Product_ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public double Quantity_Forcasted { get; set; }
        
        public double Quantity_Forcasted_PerDay { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "This field is required")]
        public DateTime Forcasting_Date { get; set; }

        [ForeignKey("Product_ID")]
        public virtual Product? Product { get; set; }
    }
    
    [Table("Inventory_IN")]
    public class Inventory_IN:BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]
        public int Product_ID { get; set;}

        [Required(ErrorMessage = "This field is required")]
        public double Quantity_Received { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.Date)]
        public DateTime DateTime { get; set; }
        
        [ForeignKey("Product_ID")]
        public virtual Product? Product { get; set; }
    }
    [Table("Inventory_Out")]
    public class Inventory_Out:BaseEntity
    {
        [Required(ErrorMessage = "This field is required")]
        public int Product_ID { get; set; }
        public int? Order_ID { get; set; }
        public int? Customer_ID { get; set; }

        [Remote(action: "CheckDateTime", controller: "Inventory", AdditionalFields = "Order_ID", ErrorMessage = "The adding date to this order must be after order start date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "This field is required")]
        public DateTime DateTime { get; set; }

        [Remote(action: "CheckQuantitySold", controller: "Inventory" , AdditionalFields = "Product_ID", ErrorMessage = "The amount of this product in inventory does not fit this sold quantity")]
        [Required(ErrorMessage = "This field is required")]
        public double Quantity_Sold { get; set; }

        [ForeignKey("Product_ID")]
        public virtual Product? Product { get; set; }
        [ForeignKey("Order_ID")]
        public virtual Order? Order { get; set; }
        [ForeignKey("Customer_ID")]
        public virtual Customer? Customer { get; set; }
    }
}
