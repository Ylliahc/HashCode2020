using System.Collections.Generic;

namespace HashCode2020
{
    public class Library
    {
        public int ID {get;set;}
        public int BookCount { get; set; }
        public List<Book> Books { get; set; }
        public int SignupDays { get; set; }
        public int ShippingCount {get;set;}
    }
}