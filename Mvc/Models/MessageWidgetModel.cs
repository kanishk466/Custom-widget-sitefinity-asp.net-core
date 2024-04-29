using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace TrialProject.Mvc.Models
{
    public class MessageWidgetModel
    {

        public string Title { get; set; }
        public string Description { get; set; }
       public  Image ItemImage { get; set; }
        public string Tags { get; set; }  
     
        public string Category { get; set; }
    }
}