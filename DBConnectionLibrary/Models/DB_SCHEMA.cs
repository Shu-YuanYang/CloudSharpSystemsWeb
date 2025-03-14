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
		/*
		public const string APPLICATIONS = "applications";
		public const string PRODUCTS = "products";
		public const string NETWORK = "network";
		public const string INTERFACES = "interfaces";
		public const string EXTERNAL_STACKEXCHANGE = "external_stackexchange";
		public const string AUTH = "auth";
		*/
	}
}
