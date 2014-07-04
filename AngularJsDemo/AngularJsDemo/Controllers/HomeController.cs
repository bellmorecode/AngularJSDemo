using AngularJsDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AngularJsDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Contacts()
        {
            var path = @"C:\Users\gferrie\Desktop\VCF\";
            var files = Directory.EnumerateFiles(path);
            var cardfile = new VCardFile();

            var rootdir = new DirectoryInfo(path);
            foreach (var fileinfo in rootdir.GetFiles("*.vcf", SearchOption.AllDirectories))
            {
                using (var reader = fileinfo.OpenText())
                {
                    cardfile.LoadFile(reader);                    
                }

            }

            return Json(cardfile);
        }
    }
}