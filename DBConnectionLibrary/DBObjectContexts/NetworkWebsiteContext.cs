using DBConnectionLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBObjectContexts
{
    public class NetworkWebsiteContext
    {
        public static async Task<TB_WEBSITE> GetWebsiteByID(AppDBMainContext DBContext, string SiteID)
        {
            return await DBContext.Websites.FirstAsync(w => w.SITE_ID == SiteID);
        }

        

    }
}
