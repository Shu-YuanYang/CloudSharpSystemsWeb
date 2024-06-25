using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    public class DB_SCHEMA
    {
        private DB_SCHEMA() { /* no instance implementation */ }

        public const string APPLICATIONS = "APPLICATIONS";
        public const string PRODUCTS = "PRODUCTS";
        public const string NETWORK = "NETWORK";
        public const string INTERFACES = "INTERFACES";
        public const string EXTERNAL_STACKEXCHANGE = "EXTERNAL_STACKEXCHANGE";
        public const string AUTH = "AUTH";

    }
}
