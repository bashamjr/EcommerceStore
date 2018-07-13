using EcommerceStore.Models.Data;
using EcommerceStore.Models.ViewModels.Pages;
using EcommerceStore.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EcommerceStore.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            // Declare list of CategoryVM
            List<CategoryVM> categoryVMList;
            // Init the list
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            // Return partial with list
            return PartialView(categoryVMList);
        }

        public ActionResult FeaturedProductPartial(string name)
        {
            // Declare a list of ProductVM
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                // Get category id
                CategoryDTO categorydto = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categorydto.Id;
                // Init the list
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();
                // Get category name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = categorydto.Name;
            }
            // Return view with list
            return PartialView(productVMList);
        }

        // GET: /Shop/Category/Name
        public ActionResult Category(string name)
        {
            // Declare a list of ProductVM
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                // Get category id
                CategoryDTO categorydto = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categorydto.Id;
                // Init the list
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();
                // Get category name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = categorydto.Name;
            }
            // Return view with list
            return View(productVMList);
        }

        // GET: /Shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Declare the VM and DTO
            ProductVM model;
            ProductDTO dto;
            // Init product id
            int id = 0;
            
            using (Db db = new Db())
            {
                // Check if product exists
                if (! db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                // Init ProductDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();
                // Get id
                id = dto.Id;
                // Init model
                model = new ProductVM(dto);
            }
            // Get gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));

            // Return view with model
            return View("ProductDetails", model);
        }
    }
}