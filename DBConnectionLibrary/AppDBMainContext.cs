using DBConnectionLibrary.DBQueryContexts;
using DBConnectionLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;


namespace DBConnectionLibrary
{
    public class AppDBMainContext : DbContext
    {

        public DbSet<TB_APP> Apps { get; set; }
        public DbSet<TB_APP_DATA_CONTROL> AppDataControls { get; set; }
        public DbSet<TB_CENTRAL_SYSTEM_LOG> CentralSystemLogs { get; set; }

        public DbSet<TB_APP_USER> AppUsers { get; set; }
        public DbSet<TB_APP_USER_IDENTITY> AppUserIdentities { get; set; }
        public DbSet<TB_APP_TEAM> AppTeams { get; set; }

        public DbSet<TB_SERVER> Servers { get; set; }
        public DbSet<TB_WEBSITE> Websites { get; set; }
        public DbSet<TB_WEBSITE_HOST> WebsiteHosts { get; set; }
        public DbSet<TB_USER_SESSION> UserSessions { get; set; }
        public DbSet<TB_USER_SESSION_ITEM> UserSessionItems { get; set; }
        public DbSet<TB_HOST_STATUS_LOG> HostStatusLogs { get; set; }

        public DbSet<TB_WEBSITE_MENU_HEADER> WebsiteMenuHeaders { get; set; }
        public DbSet<TB_WEBSITE_MENU_ITEM> WebsiteMenuItems { get; set; }

        public DbSet<TB_CS_EDUCATOR_POSTS> ExternalStackExchangeCsEducatorPosts { get; set; }


        public DbSet<V_APP_DATA_CONTROL> AppDataControlView { get; set; }
        public DbSet<V_SERVER_USAGE> ServerUsageView { get; set; }




        private readonly QueryListValidator __query_list_validator;

        public AppDBMainContext(DbContextOptions<AppDBMainContext> options, IOptions<DynamicQueryConfig>? QueryableOptionsAccessor) : base(options)
        {
            DynamicQueryConfig dynamic_qeury_config = (QueryableOptionsAccessor == null) ? new DynamicQueryConfig() : QueryableOptionsAccessor.Value;
            this.__query_list_validator = new QueryListValidator(dynamic_qeury_config);
        }
        public QueryListValidator Validator { get => this.__query_list_validator; }






        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite primary keys:
            modelBuilder.Entity<TB_APP_USER_IDENTITY>().HasKey(table => new { table.IDENTITY_PROVIDER, table.USERNAME });
            modelBuilder.Entity<TB_USER_SESSION_ITEM>().HasKey(table => new { table.SESSION_ID, table.ITEM_NAME });
            modelBuilder.Entity<TB_WEBSITE_MENU_ITEM>().HasKey(table => new { table.HEADER_ID, table.ITEM_NAME });
            modelBuilder.Entity<TB_APP_DATA_CONTROL>().HasKey(table => new { table.APP_ID, table.CONTROL_NAME, table.CONTROL_TYPE, table.CONTROL_LEVEL, table.CONTROL_VALUE, table.CONTROL_NOTE });



            modelBuilder.Entity<V_APP_DATA_CONTROL>().ToView("V_APP_DATA_CONTROL", DB_SCHEMA.APPLICATIONS);
            modelBuilder.Entity<V_SERVER_USAGE>().ToView("V_SERVER_USAGE", DB_SCHEMA.PRODUCTS);
            //modelBuilder.Entity<TB_APP>().ToTable("APPLICATIONS.TB_APP");




            // Configure AUTH.GET_APP_IDENTITY_USER_PROFILE_HEADER function
            modelBuilder.Entity<T_APP_IDENTITY_USER_PROFILE_HEADER>().ToTable("T_APP_IDENTITY_USER_PROFILE_HEADER");
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_APP_IDENTITY_USER_PROFILE_HEADER), new[] { typeof(string), typeof(string) })!)
                .HasSchema(DB_SCHEMA.AUTH);

            // Configure AUTH.GET_APP_IDENTITY_USER_PROFILE_HEADERS_BY_TEAM function
            modelBuilder.Entity<T_APP_IDENTITY_USER_PROFILE_HEADER>().ToTable("T_APP_IDENTITY_USER_PROFILE_HEADER");
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_APP_IDENTITY_USER_PROFILE_HEADERS_BY_TEAM), new[] { typeof(string), typeof(string) })!)
                .HasSchema(DB_SCHEMA.AUTH);

            // Configure AUTH.GET_TEAMS_BY_USER function
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_TEAMS_BY_USER), new[] { typeof(string), typeof(string), typeof(char) })!)
                .HasSchema(DB_SCHEMA.AUTH);


            // Configure NETWORK.GET_SERVER_LOAD function
            modelBuilder.Entity<T_SERVER_LOAD_DISTRIBUTION>().ToTable("T_SERVER_LOAD_DISTRIBUTION");
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_SERVER_LOAD), new[] { typeof(string) })!)
                .HasSchema(DB_SCHEMA.NETWORK);

            // Configure NETWORK.GET_SERVER_DETAILS function
            modelBuilder.Entity<T_SERVER_DETAIL>().ToTable("T_SERVER_DETAIL");
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_SERVER_DETAILS), new[] { typeof(string) })!)
                .HasSchema(DB_SCHEMA.NETWORK);

            // Configure NETWORK.GET_DB_HOST_LATENCY_STATISTICS function
            modelBuilder.Entity<T_HOST_LATENCY_STATISTICS>().ToTable("T_HOST_LATENCY_STATISTICS");
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_DB_HOST_LATENCY_STATISTICS), new[] { typeof(int), typeof(int), typeof(string) })!)
                .HasSchema(DB_SCHEMA.NETWORK);
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_DB_HOST_LATENCY_STATISTICS_TEST), new[] { typeof(int), typeof(int), typeof(string), typeof(DateTime) })!)
                .HasSchema(DB_SCHEMA.NETWORK);


            // Configure APPLICATIONS.CENTRAL_SYSTEM_LOG_VOLUME function
            modelBuilder.Entity<T_CENTRAL_SYSTEM_LOG_VOLUME>().ToTable("T_CENTRAL_SYSTEM_LOG_VOLUME");
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(CENTRAL_SYSTEM_LOG_VOLUME), new[] { typeof(int), typeof(string) })!)
                .HasSchema(DB_SCHEMA.APPLICATIONS);


            // Configure funciton
            modelBuilder.Entity<T_WEBSITE_MENU_ITEM>().ToTable("T_WEBSITE_MENU_ITEM");
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_MENU_ITEMS_BY_MENU), new[] { typeof(string), typeof(string), typeof(string) })!)
                .HasSchema(DB_SCHEMA.INTERFACES);
            modelBuilder
                .HasDbFunction(typeof(AppDBMainContext).GetMethod(nameof(GET_SUBMENU_ITEMS_BY_MENU), new[] { typeof(string), typeof(string), typeof(string) })!)
                .HasSchema(DB_SCHEMA.INTERFACES);

        }


        public IQueryable<T_APP_IDENTITY_USER_PROFILE_HEADER> GET_APP_IDENTITY_USER_PROFILE_HEADER(string IDENTITY_PROVIDER, string USERID)
            => FromExpression(() => GET_APP_IDENTITY_USER_PROFILE_HEADER(IDENTITY_PROVIDER, USERID));

        public IQueryable<T_APP_IDENTITY_USER_PROFILE_HEADER> GET_APP_IDENTITY_USER_PROFILE_HEADERS_BY_TEAM(string IDENTITY_PROVIDER, string TEAM_ID)
            => FromExpression(() => GET_APP_IDENTITY_USER_PROFILE_HEADERS_BY_TEAM(IDENTITY_PROVIDER, TEAM_ID));

        public IQueryable<TB_APP_TEAM> GET_TEAMS_BY_USER(string APP_ID, string USERID, char WITH_DEFAULT_ACCOUNT)
            => FromExpression(() => GET_TEAMS_BY_USER(APP_ID, USERID, WITH_DEFAULT_ACCOUNT));


        public IQueryable<T_SERVER_LOAD_DISTRIBUTION> GET_SERVER_LOAD(string SITE_ID)
            => FromExpression(() => GET_SERVER_LOAD(SITE_ID));

        public IQueryable<T_SERVER_DETAIL> GET_SERVER_DETAILS(string SITE_ID)
            => FromExpression(() => GET_SERVER_DETAILS(SITE_ID));


        public IQueryable<T_CENTRAL_SYSTEM_LOG_VOLUME> CENTRAL_SYSTEM_LOG_VOLUME(int DateOffSet, string AppID)
            => FromExpression(() => CENTRAL_SYSTEM_LOG_VOLUME(DateOffSet, AppID));

        public IQueryable<T_HOST_LATENCY_STATISTICS> GET_DB_HOST_LATENCY_STATISTICS(int TimeOffSetHours, int TimeIntervalMinutes, string HOST_IP)
            => FromExpression(() => GET_DB_HOST_LATENCY_STATISTICS(TimeOffSetHours, TimeIntervalMinutes, HOST_IP));

        public IQueryable<T_HOST_LATENCY_STATISTICS> GET_DB_HOST_LATENCY_STATISTICS_TEST(int TimeOffSetHours, int TimeIntervalMinutes, string HOST_IP, DateTime REF_TIME)
            => FromExpression(() => GET_DB_HOST_LATENCY_STATISTICS_TEST(TimeOffSetHours, TimeIntervalMinutes, HOST_IP, REF_TIME));

        public IQueryable<T_WEBSITE_MENU_ITEM> GET_MENU_ITEMS_BY_MENU(string SITE_ID, string MENU_NAME, string USER_ID)
            => FromExpression(() => GET_MENU_ITEMS_BY_MENU(SITE_ID, MENU_NAME, USER_ID));

        public IQueryable<T_WEBSITE_MENU_ITEM> GET_SUBMENU_ITEMS_BY_MENU(string SITE_ID, string MENU_NAME, string USER_ID)
            => FromExpression(() => GET_SUBMENU_ITEMS_BY_MENU(SITE_ID, MENU_NAME, USER_ID));
    }
}
