using AuxiliaryClassLibrary.DateTimeHelper;
using CloudSharpSystemsCoreLibrary.Models;
using CloudSharpSystemsCoreLibrary.Security;
using DBConnectionLibrary.Models.Mongo;
using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSharpSystemsCoreLibrary.Messaging
{
    public class UserTeamNotesHelper
    {
        public enum TeamNoteAction { 
            EDIT, REMOVE, COMPLETE
        }

        public static void CheckUserPermissions(CL_TEAM_NOTE note, string update_type, bool is_user_sender, bool is_user_team_owner) {

            List<string> permission_lst = new List<string>();

            if (update_type == TeamNoteAction.EDIT.ToString()) permission_lst = note.PERMISSIONS!.EDIT!;
            if (update_type == TeamNoteAction.REMOVE.ToString()) permission_lst = note.PERMISSIONS!.REMOVE!;
            if (update_type == TeamNoteAction.COMPLETE.ToString()) permission_lst = note.PERMISSIONS!.COMPLETE!;
            /*
            bool can_team_owner_update = permission_lst.Contains("TEAM_OWNER");
            if (is_user_team_owner && can_team_owner_update) return;

            bool can_sender_update = permission_lst.Contains("SENDER");
            if (is_user_sender && can_sender_update) return;

            bool can_everyone_update = permission_lst.Contains("EVERYONE_IN_TEAM");
            if (!can_everyone_update)
            */
            if (!CanUserUpdate(permission_lst, is_user_sender, is_user_team_owner))
                throw new UnauthorizedAccessException($"The user does not have permission to {update_type} the note!");
            
            //throw new UnauthorizedAccessException($"The user does not have permission to perform this action on the note: {update_type}!");
        }

        public static void CheckPermissionAssignments(O_TEAM_NOTE_PERMISSIONS permissions, IDictionary<string, string> permission_map) {
            if (!permissions.EDIT!.Any(role => permission_map.ContainsKey(role))) throw new Exception("Invalid team note permission assignment for EDIT!");
            if (!permissions.REMOVE!.Any(role => permission_map.ContainsKey(role))) throw new Exception("Invalid team note permission assignment for REMOVE!");
            if (!permissions.COMPLETE!.Any(role => permission_map.ContainsKey(role))) throw new Exception("Invalid team note permission assignment for COMPLETE!");
        }

        private static bool CanUserUpdate(List<string> permission_lst, bool is_user_sender, bool is_user_team_owner) {
            bool can_team_owner_update = permission_lst.Contains("TEAM_OWNER");
            if (is_user_team_owner && can_team_owner_update) return true;

            bool can_sender_update = permission_lst.Contains("SENDER");
            if (is_user_sender && can_sender_update) return true;

            bool can_everyone_update = permission_lst.Contains("EVERYONE_IN_TEAM");
            if (can_everyone_update) return true;

            return false;
        }

        public static TeamNoteUserPermissions ComputeUserPermissions(O_TEAM_NOTE_PERMISSIONS permissions, bool is_user_sender, bool is_user_team_owner) {
            TeamNoteUserPermissions user_permissions = new TeamNoteUserPermissions();
            user_permissions.can_edit = CanUserUpdate(permissions.EDIT!, is_user_sender, is_user_team_owner);
            user_permissions.can_remove = CanUserUpdate(permissions.REMOVE!, is_user_sender, is_user_team_owner);
            user_permissions.can_complete = CanUserUpdate(permissions.COMPLETE!, is_user_sender, is_user_team_owner);
            return user_permissions;
        }

        public static void CheckPriority(string priority, IDictionary<string, int> priority_map) {
            if (!priority_map.ContainsKey(priority))
                throw new Exception("Invalid team note priority!");
        }

        public static void CheckStatus(string status, IDictionary<string, string> status_map) {
            if (!status_map.ContainsKey(status!))
                throw new Exception("Invalid team note status!");
        }


        public static string GenerateNoteHash(string note_id, string editor_id, int priority_number, string title, string message, DateTime edit_time, string status_code) {
            string hash = SecurityStateGenerator.GenerateHashFromString($"{note_id},{editor_id},{priority_number},{title},{message},{TimestampHelper.ToUniversalISOFormatString(edit_time)},{status_code}");
            return hash;
        }

    }
}
