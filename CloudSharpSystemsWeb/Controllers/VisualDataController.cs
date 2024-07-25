using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using CloudSharpSystemsCoreLibrary.Models;
using DBConnectionLibrary.DBQueryContexts;
using APIConnector.Model;
using Microsoft.Extensions.Options;
using APIConnector.GoogleCloud;
using DBConnectionLibrary.Models.Mongo;
using DBConnectionLibrary.DBObjectContexts.Mongo;
using MongoDB.Driver.Linq;
//using CloudSharpSystemsCoreLibrary.Security;
//using AuxiliaryClassLibrary.DateTimeHelper;
using MongoDB.Bson;
using CloudSharpSystemsCoreLibrary.Messaging;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using static Azure.Core.HttpHeader;
//using System.Net.Http.Headers;
//using System.Xml.Linq;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using Microsoft.Net.Http.Headers;
using System.IO;
using SharpCompress.Common;

namespace CloudSharpSystemsWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VisualDataController : TemplateController
    {
        //private readonly IConfigurationSection __queryable_fields_config;
        private readonly string _GCP_BUCKET_NAME;
        
        public VisualDataController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor, IOptions<ExternalAPIMap> ExternalAPIAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {
            //this.__queryable_fields_config = config.GetSection("DBQuerableFieldsConfig");
            this._GCP_BUCKET_NAME = this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("storage_visual_data_dashboard")!.path!;
        }


        [HttpGet("get_menu_config")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetMenuConfig(string app_id) {
            var menu_config_raw = await AppDataContext.GetAppDataControlViews(this._app_db_main_context, new V_APP_DATA_CONTROL { APP_ID = app_id, CONTROL_NAME = "WEBSITE_MENU_CONFIG" });

            var menu_lst = menu_config_raw.Where(config => config.CONTROL_TYPE == "WEBSITE_MENU").Select(config => new TB_WEBSITE_MENU_HEADER
            {
                SITE_ID = this.SITE_ID,
                MENU_NAME = config.CONTROL_VALUE,
                DISPLAY_NAME = config.CONTROL_NOTE,
                IS_ENABLED = 'Y'
            });

            var route_types = menu_config_raw.Where(config => config.CONTROL_TYPE == "ROUTE_VALIDATION_FUNC").Select(config => new
            {
                route_type = config.CONTROL_LEVEL,
                validation_function = config.CONTROL_VALUE,
                description = config.CONTROL_NOTE
            });

            return new { menu_list = menu_lst, route_config = route_types };
        }

        
        [HttpGet("get_sortable_links_menu")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<T_WEBSITE_MENU_ITEM>> GetSortableLinksMenu()
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            var result_lst = await InterfacesMenuContext.GetWebsiteMenuItemsByMenu(this._app_db_main_context, this.SITE_ID, "SORTABLE_LINK_MENU", session_info.THREAD_ID!);
            var urlSigner = GCPCredentialsHelper.GetURLSigner(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_storage_read")!, this._gcp_service_account_key_obj);
            result_lst.ForEach(item => {
                item.ICON = GoogleAPIHelper.GenerateV4SignedReadUrl(urlSigner, this._GCP_BUCKET_NAME, item.ICON!).Result;
            });
            return result_lst;
        }

        [HttpPost("update_sortable_links_menu")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<GeneralAPIResponse> UpdateSortableLinksMenu([FromBody] List<T_WEBSITE_MENU_ITEM> menu_items)
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            await InterfacesMenuContext.UpdateWebsiteMenuItemsByMenu(this._app_db_main_context, this.SITE_ID, "SORTABLE_LINK_MENU", session_info.THREAD_ID!, menu_items);
            return new GeneralAPIResponse { Status = "Success", Message = "Link menu updated!" };
        }



        [HttpGet("get_sortable_charts_menu")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<T_WEBSITE_MENU_ITEM>> GetSortableChartsMenu()
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            var result_lst = await InterfacesMenuContext.GetWebsiteMenuItemsByMenu(this._app_db_main_context, this.SITE_ID, "SORTABLE_CHART_MENU", session_info.THREAD_ID!);
            var urlSigner = GCPCredentialsHelper.GetURLSigner(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_storage_read")!, this._gcp_service_account_key_obj);
            result_lst.ForEach(item => {
                item.ICON = GoogleAPIHelper.GenerateV4SignedReadUrl(urlSigner, this._GCP_BUCKET_NAME, item.ICON!).Result;
            });
            return result_lst;
        }

        [HttpPost("update_sortable_charts_menu")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<GeneralAPIResponse> UpdateSortableChartsMenu([FromBody] List<T_WEBSITE_MENU_ITEM> menu_items)
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            await InterfacesMenuContext.UpdateWebsiteMenuItemsByMenu(this._app_db_main_context, this.SITE_ID, "SORTABLE_CHART_MENU", session_info.THREAD_ID!, menu_items);
            return new GeneralAPIResponse { Status = "Success", Message = "Chart menu updated!" };
        }
        

        [HttpPost("update_sortable_menu")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<GeneralAPIResponse> UpdateSortableMenu([FromBody] List<T_WEBSITE_MENU_ITEM> menu_items)
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            var response_obj = new GeneralAPIResponse { Status = "Success", Message = "Sortable menu updated!" };
            if (!menu_items.Any()) return response_obj;
            var menu_header = await InterfacesMenuContext.GetWebsiteMenuHeader(this._app_db_main_context, session_info.THREAD_ID!, "", menu_items.First().MENU_DISPLAY_NAME!);
            await InterfacesMenuContext.UpdateWebsiteMenuItemsByMenu(this._app_db_main_context, this.SITE_ID, menu_header.MENU_NAME!, session_info.THREAD_ID!, menu_items);
            return response_obj;
        }


        [HttpGet("get_menu_list")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetMenuList()
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            //var raw_lst = await InterfacesMenuContext.GetWebsiteSubMenuItemsPartial(this._app_db_main_context, this.SITE_ID, "MAIN_MENU_LIST", "1511196");
            var raw_lst = await InterfacesMenuContext.GetWebsiteSubMenuItemsByMenu(this._app_db_main_context, this.SITE_ID, "MAIN_MENU_LIST", session_info.THREAD_ID!);
            var result_lst = raw_lst
                .OrderBy(item => item.MENU_RANKING)
                .GroupBy(g => g.MENU_DISPLAY_NAME)
                .Select(g => new { menu_display_name = g.Key, menu_items = g.Where(item => !String.IsNullOrEmpty(item.ITEM_NAME)).ToList() });
                //.ToList();
            var urlSigner = GCPCredentialsHelper.GetURLSigner(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_storage_read")!, this._gcp_service_account_key_obj);
            foreach (var menu in result_lst) 
            {
                menu.menu_items.ForEach(item => {
                    item.ICON = GoogleAPIHelper.GenerateV4SignedReadUrl(urlSigner, this._GCP_BUCKET_NAME, item.ICON!).Result;
                });
            }
            return result_lst;
        }


        // Logic based on: https://andrewlock.net/reading-json-and-binary-data-from-multipart-form-data-sections-in-aspnetcore/
        [HttpPost("add_new_menu_item")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        public async Task<GeneralAPIResponse> AddNewMenuItem() {

            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            T_WEBSITE_MENU_ITEM? itemData = null;
            MemoryStream? iconMemoryStream = null;
            string iconFileName = "";
            //byte[]? binaryData = null;

            Dictionary<string, Func<MultipartSection?, Task?>> parse_actions = new Dictionary<string, Func<MultipartSection?, Task?>> {
                { 
                    "application/json", async (MultipartSection? section) => {
                        itemData = await JsonSerializer.DeserializeAsync<T_WEBSITE_MENU_ITEM>(section!.Body);
                        if (!string.IsNullOrEmpty(itemData!.ICON)) iconFileName = Path.GetFileName(itemData.ICON);
                        //iconMemoryStream = await GoogleAPIHelper.DownloadFileFromStorageIntoMemory(storageClient, this._GCP_BUCKET_NAME, $"COMMON_ICONS/BRANDS/{itemData.ICON}");
                    } 
                },
                {
                    "image/", async (MultipartSection? section) => {
                        var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section!.ContentDisposition, out var contentDisposition);

                        iconFileName = contentDisposition!.FileName!.Value!;

                        iconMemoryStream = new MemoryStream();
                        await section!.Body.CopyToAsync(iconMemoryStream, Request.HttpContext.RequestAborted);
                        //binaryData = memoryStream.ToArray();

                        // Do not upload the file if more than 2 MB: Shu-Yuan Yang added limitation 20240702
                        if (2097152 < iconMemoryStream.Length) {
                            throw new FileLoadException($"Selected file is too large ({iconMemoryStream.Length / (1024 * 1024)} MB)!");
                        }
                    }
                }
            };

            try
            {
                await this.ParseFormData(parse_actions);
            }
            catch (BadHttpRequestException ex) 
            {
                return new GeneralAPIResponse {
                    Status = "BAD_INPUT",
                    Message = ex.Message
                };
            }

            if (String.IsNullOrEmpty(iconFileName)) {
                return new GeneralAPIResponse
                {
                    Status = "BAD_INPUT",
                    Message = "An error occurred when parsing the file format!"
                };
            }

            bool to_upload_new_file = String.IsNullOrEmpty(itemData!.ICON);
            DirectoryInfo dir_info = new DirectoryInfo(iconFileName);
            string extension = dir_info.Extension;
            
            //try
            //{
            await DBTransactionContext.DBTransact(this._app_db_main_context, async (context, transaction) =>
            {
                // Add menu item data to database:
                var menu_header = await InterfacesMenuContext.GetWebsiteMenuHeader(this._app_db_main_context, session_info.THREAD_ID!, "", itemData!.MENU_DISPLAY_NAME!);
                itemData.ICON = $"{menu_header.MENU_NAME}/{session_info.THREAD_ID!}/{itemData.ITEM_NAME!.ToLower()}{extension}";
                //Task db_transact = InterfacesMenuContext.InsertNewMenuItemProcedure(this._app_db_main_context, menu_header.HEADER_ID!, itemData!, session_info.THREAD_ID!);
                await InterfacesMenuContext.InsertNewMenuItemProcedure(this._app_db_main_context, menu_header.HEADER_ID!, itemData!, session_info.THREAD_ID!);

                // Add icon file to cloud storage:
                var storageClient = GCPCredentialsHelper.GetStorageClient(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_storage_write")!, this._gcp_service_account_key_obj);
                if (to_upload_new_file /*&& iconMemoryStream != null*/)
                {
                    await GoogleAPIHelper.UploadFileToStorage(storageClient, this._GCP_BUCKET_NAME, itemData.ICON!, iconMemoryStream!);
                }
                else
                {
                    await GoogleAPIHelper.CopyFileInStorage(storageClient, this._GCP_BUCKET_NAME, $"COMMON_ICONS/BRANDS/{iconFileName}", this._GCP_BUCKET_NAME, itemData.ICON!);
                }

                //await db_transact;
            });

            return new GeneralAPIResponse
            {
                Status = "Success",
                Message = "Menu item successfully added."
            };
        }



        [HttpGet("get_common_icons")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetCommonIcons(string app_id) {
            var storageClient = GCPCredentialsHelper.GetStorageClient(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_storage_read")!, this._gcp_service_account_key_obj);
            var objects = GoogleAPIHelper.ListFilesFromStorage(storageClient, this._GCP_BUCKET_NAME, "COMMON_ICONS/BRANDS/");
            var urlSigner = GCPCredentialsHelper.GetURLSigner(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_storage_read")!, this._gcp_service_account_key_obj);

            var items = objects.Select(obj => {
                //string signed_obj_name = GoogleAPIHelper.GenerateV4SignedReadUrl(urlSigner, this._GCP_BUCKET_NAME, obj.Name).Result;
                //return signed_obj_name;
                return new { 
                    PATH = obj.Name,
                    NAME = obj.Name.Substring(obj.Name.LastIndexOf("/") + 1)
                };
            });

            var common_icon_controls = await AppDataContext.GetAppDataControlViews(this._app_db_main_context, new V_APP_DATA_CONTROL { APP_ID = app_id, CONTROL_NAME = "WEBSITE_ASSET_CONFIG", CONTROL_TYPE = "COMMON_ICON" });
            var noted_items = from item in items 
                         join control in common_icon_controls 
                         on item.NAME equals control.CONTROL_VALUE into joinGroups
                         from subgroup in joinGroups.DefaultIfEmpty()
                         select new
                         {
                             icon_name = item.NAME,
                             icon_note = subgroup?.CONTROL_LEVEL ?? item.NAME, // Left join and default to item file name if control does not exist
                             icon_url = GoogleAPIHelper.GenerateV4SignedReadUrl(urlSigner, this._GCP_BUCKET_NAME, item.PATH).Result
                         };

            return noted_items;
        }




        [HttpGet("get_server_usage_logistics")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<V_SERVER_USAGE> > GetServerUsageLogistics()
        {
            var result_lst = await ProductsServerContext.GetServerUsageLogistics(this._app_db_main_context);
            return result_lst;
        }







        //GetCSEducatorPostByID
        [HttpPost("query_cs_educator_posts")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<TB_CS_EDUCATOR_POSTS> > QueryStackExchangeCSEducatorPosts([FromBody] QueryList? query_lst)
        {

            //var queryable_fields_config = this.GetQueryableFieldsConfig(this.__queryable_fields_config, DB_SCHEMA.EXTERNAL_STACKEXCHANGE, nameof(ExternalStackExchangeContext.GetCSEducatorPostsByQuery));
            //if (query_lst != null) QueryListValidator.ValidateQueryableFields(query_lst, queryable_fields_config);
            var posts = await ExternalStackExchangeContext.GetCSEducatorPostsByQuery(this._app_db_main_context, query_lst);
            return posts;
        }

        /*
        //GetCSEducatorPostByID
        [HttpPost("query_str")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public IEnumerable<string> QueryStr(string queryID, [FromBody] QueryList? query_lst)
        {
            
            //var queryable_fields_config = this.GetQueryableFieldsConfig(this.__queryable_fields_config, DB_SCHEMA.EXTERNAL_STACKEXCHANGE, nameof(ExternalStackExchangeContext.GetCSEducatorPostsByQuery));
            //if (query_lst != null) QueryListValidator.ValidateQueryableFields(query_lst, queryable_fields_config);
            var query_strs = AppDataContext.GetQueryStr(this._app_db_main_context, queryID, query_lst);
            return query_strs;
        }
        */










        [HttpGet("get_teams_by_user")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IEnumerable<TB_APP_TEAM>> GetTeamsByUser(string app_id) {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            var teams_lst = await AppTeamContext.GetTeamsByUser(this._app_db_main_context, app_id, session_info.THREAD_ID!, true);

            var team_user_ids = teams_lst.Select(t => t.PRIMARY_CONTACT)
                .Concat(teams_lst.Select(t => t.OWNED_BY))
                .Concat(teams_lst.Select(t => t.EDIT_BY))
                .Distinct();

            var team_users = await AppUserContext.GetUsersByIDs(this._app_db_main_context, team_user_ids!);

            teams_lst = teams_lst.Select(team => {
                team.PRIMARY_CONTACT = team_users.FirstOrDefault(u => u.USERID == team.PRIMARY_CONTACT, new TB_APP_USER { USERID = team.PRIMARY_CONTACT, NAME_ALIAS = team.PRIMARY_CONTACT }).NAME_ALIAS;
                team.OWNED_BY = team_users.FirstOrDefault(u => u.USERID == team.OWNED_BY, new TB_APP_USER { USERID = team.OWNED_BY, NAME_ALIAS = team.OWNED_BY }).NAME_ALIAS;
                team.EDIT_BY = team_users.FirstOrDefault(u => u.USERID == team.EDIT_BY, new TB_APP_USER { USERID = team.EDIT_BY, NAME_ALIAS = team.EDIT_BY }).NAME_ALIAS;
                return team;
            });

            return teams_lst;
        }


        [HttpGet("get_team_note_config")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetTeamNotePriorities() {
            var priority_config = await MongoAppDataContext.GetTeamNotePriorityMap(this._app_db_mongo_context);
            var status_config = await MongoAppDataContext.GetTeamNoteStatusMap(this._app_db_mongo_context);
            var permission_config = await MongoAppDataContext.GetTeamNotePermissionMap(this._app_db_mongo_context);
            var action_config = Enum.GetValues(typeof(UserTeamNotesHelper.TeamNoteAction))
                                    .Cast<UserTeamNotesHelper.TeamNoteAction>()
                                    .Select(t => t.ToString()).ToList();
            var config = new { priority_config, status_config, action_config, permission_config };
            return config;
        }

        






        [HttpGet("get_notes_for_user")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IEnumerable<TeamNoteData>> GetNotesForUser(string app_id) {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");
            
            var teams_lst = await AppTeamContext.GetTeamsByUser(this._app_db_main_context, app_id, session_info.THREAD_ID!, true);
            
            var teamNotes = await TeamNoteContext.GetTeamNotesByTeams(this._app_db_mongo_context, teams_lst.Where(t => t.TEAM_NAME != "My Notes").Select(t => t.TEAM_ID)!);
            var userOwnNotes = await TeamNoteContext.GetTeamNotes(this._app_db_mongo_context, new CL_TEAM_NOTE { TEAM_ID = teams_lst.First(t => t.TEAM_NAME == "My Notes").TEAM_ID!, SENDER_ID = session_info.THREAD_ID! });
            //var priority_map = await MongoAppDataContext.GetTeamNotePriorityMap(this._app_db_mongo_context);
            //var status_map = await MongoAppDataContext.GetTeamNoteStatusMap(this._app_db_mongo_context);
            var notes = teamNotes.Concat(userOwnNotes);

            var note_user_ids = notes.Select(n => n.SENDER_ID).Concat(notes.Select(n => n.EDIT_BY)).Distinct();
            var note_users = await AppUserContext.GetUsersByIDs(this._app_db_main_context, note_user_ids!);

            var results = from note in notes
                          join team in teams_lst on note.TEAM_ID equals team.TEAM_ID
                          let sender_obj = note_users.FirstOrDefault(n => n.USERID == note.SENDER_ID, new TB_APP_USER { USERID = note.SENDER_ID, NAME_ALIAS = note.SENDER_ID })
                          let editor = note_users.FirstOrDefault(n => n.USERID == note.EDIT_BY, new TB_APP_USER { USERID = note.EDIT_BY, NAME_ALIAS = note.EDIT_BY })
                          select new TeamNoteData
                          {
                              note_id = note._id,
                              team_name = team.TEAM_NAME,
                              sender_name = sender_obj.NAME_ALIAS, //note_users.FirstOrDefault(n => n.USERID == note.SENDER_ID, new TB_APP_USER { USERID = note.SENDER_ID }).NAME_ALIAS, 
                              priority = note.PRIORITY,
                              //priority_number = priority_map[note.PRIORITY!],
                              title = note.TITLE,
                              message = note.MESSAGE,
                              //status = status_map[note.STATUS!],
                              status_code = note.STATUS,
                              permissions = note.PERMISSIONS,
                              user_permissions = UserTeamNotesHelper.ComputeUserPermissions(note.PERMISSIONS!, note.SENDER_ID == session_info.THREAD_ID!, team.TEAM_NAME == "My Notes" || team.OWNED_BY == session_info.THREAD_ID!),
                              note_hash = note.NOTE_HASH,
                              last_edited_by = editor.NAME_ALIAS, // note_users.FirstOrDefault(n => n.USERID == note.EDIT_BY, new TB_APP_USER { USERID = note.EDIT_BY }).NAME_ALIAS,
                              last_edited_time = note.EDIT_TIME
                          };

            return results;
        }



        

        [HttpPost("add_note")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<GeneralAPIResponse> AddNote(string app_id, [FromBody] TeamNoteData note_data)
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            var team = (await AppTeamContext.GetTeams(this._app_db_main_context, new TB_APP_TEAM { APP_ID = app_id, TEAM_NAME = note_data.team_name })).First();

            var priority_map = await MongoAppDataContext.GetTeamNotePriorityMap(this._app_db_mongo_context);
            var status_map = await MongoAppDataContext.GetTeamNoteStatusMap(this._app_db_mongo_context);
            var permission_map = await MongoAppDataContext.GetTeamNotePermissionMap(this._app_db_mongo_context);

            string note_id = ObjectId.GenerateNewId().ToString(); //SecurityStateGenerator.GenerateRandomState(SecurityStateGenerator.Salt.Byte64Base64);
            DateTime edit_time = DateTime.Now;
            string to_do_status = "TODO";
            CL_TEAM_NOTE db_note = new CL_TEAM_NOTE {
                _id = note_id,
                TEAM_ID = team.TEAM_ID,
                SENDER_ID = session_info.THREAD_ID!,
                PRIORITY = note_data.priority,
                TITLE = note_data.title,
                MESSAGE = note_data.message,
                EDIT_BY = session_info.THREAD_ID,
                EDIT_TIME = edit_time,
                PERMISSIONS = note_data.permissions,
                STATUS = to_do_status,
                NOTE_HASH = UserTeamNotesHelper.GenerateNoteHash(note_id, session_info.THREAD_ID!, priority_map[note_data.priority!], note_data.title!, note_data.message!, edit_time, to_do_status)
            };
            UserTeamNotesHelper.CheckPermissionAssignments(db_note.PERMISSIONS!, permission_map);
            UserTeamNotesHelper.CheckPriority(db_note.PRIORITY!, priority_map);
            UserTeamNotesHelper.CheckStatus(db_note.STATUS!, status_map);

            await TeamNoteContext.AddTeamNote(this._app_db_mongo_context, db_note);
            /*
            note_data.note_id = db_note._id;
            note_data.status = db_note.STATUS;
            note.NOTE_HASH = db_note.NOTE_HASH;
            
            return note;*/
            return new GeneralAPIResponse
            {
                Status = "OK",
                Message = "Note successfully added."
            };
        }



        [HttpPost("update_note")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<GeneralAPIResponse> UpdateNote(string update_type, [FromBody] TeamNoteData note_data)
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            update_type = update_type.ToUpper(); // uniformly change to upper case.
            var old_record = (await TeamNoteContext.GetTeamNotes(this._app_db_mongo_context, new CL_TEAM_NOTE { _id = note_data.note_id })).First();
            if (note_data.note_hash != old_record.NOTE_HASH) return new GeneralAPIResponse
            {
                Status = "DISCREPANT_HASH",
                Message = "The note was previously modified. Please refresh to see the latest version."
            };

            var priority_map = await MongoAppDataContext.GetTeamNotePriorityMap(this._app_db_mongo_context);
            var status_map = await MongoAppDataContext.GetTeamNoteStatusMap(this._app_db_mongo_context);

            // Block attempts to modify after immutable status:
            if (old_record.STATUS == "REMOVED" || old_record.STATUS == "COMPLETED")
                throw new Exception($"The note has been {old_record.STATUS} and cannot be modified!");

            var team = (await AppTeamContext.GetTeams(this._app_db_main_context, new TB_APP_TEAM { TEAM_ID = old_record.TEAM_ID })).First();

            bool is_user_sender = (old_record.SENDER_ID == session_info.THREAD_ID!);
            bool is_user_team_owner = (team.TEAM_NAME == "My Notes" || team.OWNED_BY == session_info.THREAD_ID!);

            // Permission checking should consider and block invalid actions, i.e. invalid update_type values.
            UserTeamNotesHelper.CheckUserPermissions(old_record, update_type, is_user_sender, is_user_team_owner);
            
            string update_status = "";
            if (update_type == UserTeamNotesHelper.TeamNoteAction.REMOVE.ToString()) update_status = "REMOVED";
            if (update_type == UserTeamNotesHelper.TeamNoteAction.COMPLETE.ToString()) update_status = "COMPLETED";
            bool to_complete_or_remove = !String.IsNullOrEmpty(update_status);
            if (update_type == UserTeamNotesHelper.TeamNoteAction.EDIT.ToString()) update_status = old_record.STATUS!;

            UserTeamNotesHelper.CheckStatus(update_status, status_map);
            if (!to_complete_or_remove) UserTeamNotesHelper.CheckPriority(note_data.priority!, priority_map);

            DateTime edit_time = DateTime.Now;
            CL_TEAM_NOTE db_note = new CL_TEAM_NOTE
            {
                _id = note_data.note_id,
                PRIORITY = to_complete_or_remove? "" : note_data.priority,
                TITLE = to_complete_or_remove? "" : note_data.title,
                MESSAGE = to_complete_or_remove? "" : note_data.message,
                EDIT_BY = session_info.THREAD_ID,
                EDIT_TIME = edit_time,
                PERMISSIONS = null, // note_data.permissions,
                STATUS = update_status, //note_data.status_code,
                NOTE_HASH = UserTeamNotesHelper.GenerateNoteHash(
                    note_data.note_id!, 
                    session_info.THREAD_ID!, 
                    to_complete_or_remove? priority_map[old_record.PRIORITY!] : priority_map[note_data.priority!], 
                    to_complete_or_remove? old_record.TITLE! : note_data.title!, 
                    to_complete_or_remove? old_record.MESSAGE! : note_data.message!, 
                    edit_time,
                    update_status)
            };

            // Check note hash while updating to make sure version is up to date:
            bool updated = await TeamNoteContext.UpdateTeamNote(this._app_db_mongo_context, db_note, old_record.NOTE_HASH!);
            if (!updated) return new GeneralAPIResponse
            {
                Status = "DISCREPANT_HASH",
                Message = "The note may have been previously modified. Please refresh to see the latest version."
            };

            //note.NOTE_HASH = db_note.NOTE_HASH;

            return new GeneralAPIResponse {
                Status = "OK",
                Message = "Note successfully updated."
            };
        }




    }
}
