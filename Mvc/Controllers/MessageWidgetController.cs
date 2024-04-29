using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Mvc;
using Telerik.Sitefinity.Utilities.TypeConverters;

using TrialProject.Mvc.Models;
using Image = Telerik.Sitefinity.Libraries.Model.Image;
using Telerik.Sitefinity.Modules.Libraries;
using DocumentFormat.OpenXml.Vml;
using System.IO;
using System.Text.RegularExpressions;
using Telerik.Sitefinity.Workflow;
using Telerik.Sitefinity.Taxonomies.Model;
using Telerik.Sitefinity.Taxonomies;
using DocumentFormat.OpenXml.ExtendedProperties;
using Telerik.Microsoft.Practices.EnterpriseLibrary.Common.Configuration.Manageability.Adm;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Libraries.Model;
using static Telerik.Sitefinity.Security.SecurityConstants.Sets;
using Album = Telerik.Sitefinity.Libraries.Model.Album;
using Telerik.Sitefinity.RelatedData;

namespace TrialProject.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "MessageWidget", Title = "Message Widget", SectionName = "MvcWidgets")]
    public class MessageWidgetController : Controller
    {

        Guid albumId ;
        string albumTitle;

        // GET: MessageWidget
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult SubmitForm(MessageWidgetModel messageWidgetModel, HttpPostedFileBase ItemImage)
        {



            string name = messageWidgetModel.Tags;

            string category = messageWidgetModel.Category;





            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager();
            Type kanishkType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.DynamicModule.Kanishk");
            DynamicContent kanishkItem = dynamicModuleManager.CreateDataItem(kanishkType);





            kanishkItem.SetString("Title", messageWidgetModel.Title);
            kanishkItem.SetString("Description", messageWidgetModel.Description);





           



            // Add Tags
            AddTags(name);

            addtaxon(kanishkItem, messageWidgetModel.Tags);


            // Add Category 
            addCategory(category);
            addCategoryToModule(kanishkItem, category);



            FindFolderById();


            if (ItemImage != null && ItemImage.ContentLength > 0)
            {


                // Generate a new GUID for the image
                Guid masterImageId = Guid.NewGuid();
                // Obtain the image stream from the uploaded file
                Stream imageStream = ItemImage.InputStream;

                // Obtain the image file name
                string imageFileName = System.IO.Path.GetFileName(ItemImage.FileName);

                // Obtain the image extension from the file name
                string imageExtension = System.IO.Path.GetExtension(imageFileName);


                string imageTitle = System.IO.Path.GetFileNameWithoutExtension(imageFileName);

                // call the create image function
                CreateImageWithNativeAPI(masterImageId, imageFileName, imageExtension, imageTitle, imageStream, albumId , kanishkItem);



            }
            dynamicModuleManager.SaveChanges();



            return View("Index", messageWidgetModel);

        }

   
        // Add tag 
        private void AddTags(string name)
        {

            var taxonomyManager = TaxonomyManager.GetManager();

            //Get the Tags taxonomy
            var tagTaxonomy = taxonomyManager.GetTaxonomies<FlatTaxonomy>().SingleOrDefault(s => s.Name == "Tags");

            if (tagTaxonomy == null) return;

            //Create a new FlatTaxon
            var taxon = taxonomyManager.CreateTaxon<FlatTaxon>();

            //Associate the item with the flat taxonomy
            taxon.FlatTaxonomy = tagTaxonomy;

            taxon.Name = Regex.Replace(name.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            taxon.Title = name;
            taxon.UrlName = Regex.Replace(name.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");

            //Add it to the list
            tagTaxonomy.Taxa.Add(taxon);
            taxonomyManager.SaveChanges();







        }

        public void addtaxon(DynamicContent kanishkItem, string tagname)
        {
            TaxonomyManager taxonomyManager = TaxonomyManager.GetManager();
            var Tags = taxonomyManager.GetTaxa<FlatTaxon>().Where(t => t.Taxonomy.Name == "Tags");

            foreach (var Tag in Tags.Where(w => w.Title.ToLower() == tagname.ToLower()))
            {
                if (Tag != null)
                {
                    kanishkItem.Organizer.AddTaxa("Tags", Tag.Id);
                }
            }
        }


        // Add Category
        public void addCategory(string category)
        {
            var taxonomyManager = TaxonomyManager.GetManager();

            //Get the Categories taxonomy
            var categoryTaxonomy = taxonomyManager.GetTaxonomies<HierarchicalTaxonomy>().SingleOrDefault(s => s.Name == "Categories");

            if (categoryTaxonomy == null) return;

            //Create a new HierarchicalTaxon
            var taxon = taxonomyManager.CreateTaxon<HierarchicalTaxon>();

            //Associate the item with the hierarchical taxonomy
            taxon.Taxonomy = categoryTaxonomy;

            taxon.Name = Regex.Replace(category.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            taxon.Title = category;
            taxon.UrlName = Regex.Replace(category.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");



            //Add it to the list
            categoryTaxonomy.Taxa.Add(taxon);

            taxonomyManager.SaveChanges();
        }

        public void addCategoryToModule(DynamicContent kanishkItem, string categoryName)
        {
            TaxonomyManager taxonomyManager = TaxonomyManager.GetManager();
            var Category = taxonomyManager.GetTaxa<HierarchicalTaxon>().Where(t => t.Taxonomy.Name == "Categories");

            foreach (var categorys in Category.Where(w => w.Title.ToLower() == categoryName.ToLower()))
            {

                if (categorys != null)
                {
                    kanishkItem.Organizer.AddTaxa("Category", categorys.Id);
                }
            }
        }

        // create Album id 
        public void FindFolderById()
        {
            //gets an isntance of the LibrariesManager
            var manager = LibrariesManager.GetManager();
            Album albumManager = manager.GetAlbums().Where(a => a.Title == "ImageAlbumTitle1").FirstOrDefault();
            //creates an image album(library)

            if (albumManager == null)
            {


                var imagesAlbum = manager.CreateAlbum();
                imagesAlbum.Title = "ImageAlbumTitle3";
                manager.SaveChanges();
            }
            else
            {
                CreateAlbumNativeAPI(albumManager);
            }


            //Creating an album with predefined ID


        }

        private void CreateAlbumNativeAPI(Album imagesAlbum)
        {
             albumId = imagesAlbum.Id;
             albumTitle = imagesAlbum.Title;
        }



        //Create images

        private void CreateImageWithNativeAPI(Guid masterImageId, string imageFileName, string imageExtension, string imageTitle, Stream imageStream, Guid albumId , DynamicContent kanishkItem)
        {
            LibrariesManager librariesManager = LibrariesManager.GetManager();
            Image image = librariesManager.GetImages().Where(i => i.Id == masterImageId).FirstOrDefault();


            if (image == null)
            {
                //The album post is created as master. The masterImageId is assigned to the master version.
                image = librariesManager.CreateImage(masterImageId);

                //Set the parent album.
                Album album = librariesManager.GetAlbums().Where(i => i.Id == albumId).SingleOrDefault();
                image.Parent = album;

                //Set the properties of the album post.
                image.Title = imageTitle;
                image.DateCreated = DateTime.UtcNow;
                image.PublicationDate = DateTime.UtcNow;
                image.LastModified = DateTime.UtcNow;
                image.UrlName = Regex.Replace(imageTitle.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
                image.MediaFileUrlName = Regex.Replace(imageFileName.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");

                //Upload the image file.
                // The imageExtension parameter must contain '.', for example '.jpeg'
                librariesManager.Upload(image, imageStream, imageExtension);

                //Save the changes.
                librariesManager.SaveChanges();

                ////Publish the Albums item. The live version acquires new ID.
                //var bag = new Dictionary<string, string>();
                //bag.Add("ContentType", typeof(Image).FullName);
                //WorkflowManager.MessageWorkflow(masterImageId, typeof(Image), null, "Publish", false, bag);


                var itemImageItem = librariesManager.GetImages().FirstOrDefault(i => i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master);
                if (itemImageItem != null)
                {
                    // This is how we relate an item
                    kanishkItem.CreateRelation(itemImageItem, "ItemImage");
                }
            }
        }
    }
}