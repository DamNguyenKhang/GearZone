using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GearZone.Web.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        // Query param models
        [BindProperty(SupportsGet = true)]
        public ProductQuery Query { get; set; } = new ProductQuery();

        public PaginatedList<ProductViewModel> Products { get; set; } = new PaginatedList<ProductViewModel>();
        public ProductStats Stats { get; set; } = new ProductStats();

        // Dropdown data
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Stores { get; set; } = new List<SelectListItem>();

        public void OnGet()
        {
            // Mock Stats
            Stats = new ProductStats
            {
                TotalProducts = 12450,
                ActiveProducts = 11200,
                PendingApproval = 150,
                OutOfStock = 85
            };

            // Mock dropdowns
            Categories = new List<SelectListItem>
            {
                new SelectListItem("Keyboards", "1"),
                new SelectListItem("Mice", "2"),
                new SelectListItem("Headsets", "3"),
                new SelectListItem("Monitors", "4")
            };
            Stores = new List<SelectListItem>
            {
                new SelectListItem("GearGaming VN", "1"),
                new SelectListItem("TechSpace", "2"),
                new SelectListItem("Keychron Official", "3")
            };

            // Mock Data
            var items = new List<ProductViewModel>
            {
                new ProductViewModel { Id = Guid.NewGuid(), Name = "Logitech G Pro X Superlight", Sku = "LOG-GPX-WHT", StoreName = "GearGaming VN", Category = "Mice", Price = 3500000, Stock = 45, Status = "Active", ThumbnailUrl = "https://placehold.co/100x100?text=Mouse", CreatedAt = DateTime.Now.AddDays(-10) },
                new ProductViewModel { Id = Guid.NewGuid(), Name = "Keychron K8 Pro", Sku = "KEY-K8P-RGB", StoreName = "Keychron Official", Category = "Keyboards", Price = 2500000, Stock = 0, Status = "Active", ThumbnailUrl = "https://placehold.co/100x100?text=Keyboard", CreatedAt = DateTime.Now.AddDays(-20) },
                new ProductViewModel { Id = Guid.NewGuid(), Name = "Razer BlackShark V2", Sku = "RAZ-BSV2-BLK", StoreName = "TechSpace", Category = "Headsets", Price = 1800000, Stock = 12, Status = "Pending", ThumbnailUrl = "https://placehold.co/100x100?text=Headset", CreatedAt = DateTime.Now.AddDays(-2) },
                new ProductViewModel { Id = Guid.NewGuid(), Name = "ASUS ROG Swift PG259QN", Sku = "ASU-PG259QN", StoreName = "GearGaming VN", Category = "Monitors", Price = 15000000, Stock = 5, Status = "Suspended", ThumbnailUrl = "https://placehold.co/100x100?text=Monitor", CreatedAt = DateTime.Now.AddDays(-30) },
                new ProductViewModel { Id = Guid.NewGuid(), Name = "Fake Controller PS5", Sku = "FAK-PS5-CTL", StoreName = "TechSpace", Category = "Setup Accessories", Price = 500000, Stock = 100, Status = "Rejected", ThumbnailUrl = "https://placehold.co/100x100?text=Fake", CreatedAt = DateTime.Now.AddDays(-5) }
            };

            Products = new PaginatedList<ProductViewModel>(items, 120, Query.PageNumber, 10);
        }
    }

    public class ProductQuery
    {
        public string SearchTerm { get; set; }
        public string SearchType { get; set; } // Name, SKU, Store
        public string Status { get; set; }
        public int? CategoryId { get; set; }
        public Guid? StoreId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool OutOfStock { get; set; }
        public int PageNumber { get; set; } = 1;
    }

    public class ProductViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string StoreName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; } // Pending, Active, Suspended, Rejected
        public string ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductStats
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int PendingApproval { get; set; }
        public int OutOfStock { get; set; }
    }

    public class PaginatedList<T>
    {
        public List<T> Items { get; private set; }
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }
        public int PageSize { get; private set; }

        public PaginatedList()
        {
            Items = new List<T>();
        }

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            PageSize = pageSize;
            Items = items;
        }
    }
}
