using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CPrakash.Web.Profile.Mvc.Models
{
    public class Address
    {
        public string Street { get; set; }

        public string Unit { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }
    }
}