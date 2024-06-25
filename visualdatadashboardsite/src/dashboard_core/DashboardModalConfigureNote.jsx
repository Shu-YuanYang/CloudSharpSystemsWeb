import { useState, useMemo } from "react";
import { APIEndpoints } from "../site_config.json";
import { api_full_path_with_query, get_api } from "../endpoints/api/api_helper";
import useAuthorizedFetch from "../endpoints/api/useAuthorizedFetch";
import DashboardModalBase from "./DashboardModalBase";
import { useContext } from 'react';
import IdentityContext from "../auxiliary/wrappers/IdentityContext";



const DashboardModalConfigureNote = ({ title, actionButtons, teamNoteConfig, configurationType, configureItem }) => {

    const userIdentity = useContext(IdentityContext);

    const { data: teamsData, refreshData: refreshTeamsData, isPending: isTeamsDataPending, error: teamsDataFetchError } = useAuthorizedFetch(api_full_path_with_query(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_teams_by_user").path, "app_id=CloudSharpVisualDashboard"), userIdentity ? userIdentity.session_id : null);

    const userTeams = useMemo(() => teamsData? teamsData.map(team => team.TEAM_NAME).sort() : [], [teamsData]); // TODO: request from CloudSharp backend based on user identity.

    const priorityList = useMemo(() => {
        const priority_lst = Object.entries(teamNoteConfig.priority_config);
        const comp_func = (a, b) => { return (a[1] < b[1]) ? -1 : 1; };
        const sorted_priority_lst = priority_lst.sort(comp_func).map(entry => entry[0]);
        return sorted_priority_lst;
    }, [teamNoteConfig]);

    const permissionList = useMemo(() => {
        const permission_lst = Object.entries(teamNoteConfig.permission_config);
        return permission_lst;
    }, [teamNoteConfig]);

    const [team, setTeam] = useState("My Notes");
    const [noteTitle, setNoteTitle] = useState("");
    const [priority, setPriority] = useState("medium");
    const [message, setMessage] = useState("");
    //const [permissions, setPermissions] = useState({ EDIT: [], REMOVE: [], COMPLETE: [] });

    const [permissionCheckMap, setPermissionCheckMap] = useState({
        EDIT: { TEAM_OWNER: true, SENDER: true, EVERYONE_IN_TEAM: false },
        REMOVE: { TEAM_OWNER: true, SENDER: true, EVERYONE_IN_TEAM: false },
        COMPLETE: { TEAM_OWNER: true, SENDER: true, EVERYONE_IN_TEAM: true }
    });
    /*
    const [note, setNote] = useState({
        note_id: "",
        team_name: "",
        sender_name: "",
        priority: "",
        title: "",
        message: "",
        last_edited_by: "",
        last_edited_time: "2024-06-05T08:01:54.869+00:00",
        note_hash: "",
        permissions: {
            "EDIT": [],
            "REMOVE": [],
            "COMPLETE": []
        },
        user_permissions: {
            can_edit: true,
            can_remove: true,
            can_complete: true
        },
        status_code: ""
    });
    */
    const [isUpdating, setIsUpdating] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");

    const priorityNumber = useMemo(() => teamNoteConfig.priority_config[priority], [teamNoteConfig, priority]);

    const handlePermissionChange = (action, role) => {
        let newCheckMap = { ...permissionCheckMap };
        newCheckMap[action][role] = !newCheckMap[action][role];
        setPermissionCheckMap(newCheckMap);
    };

    const configureItemSafe = async () => {
        setIsUpdating(true);
        // TODO: Validation
        let newNote = {
            note_id: "",
            team_name: team,
            sender_name: "",
            priority: priority,
            title: noteTitle,
            message: message,
            last_edited_by: "",
            last_edited_time: "2024-06-05T08:01:54.869+00:00", // any time will do as long as it is a timestamp
            note_hash: "",
            permissions: Object.entries(permissionCheckMap).reduce((acc, [key, subobj]) => {
                acc[key] = Object.entries(subobj).filter(entry => entry[1]).map(entry => entry[0]);
                return acc;
            }, {}),
            user_permissions: {
                can_edit: true,
                can_remove: true,
                can_complete: true
            },
            status_code: ""
        };

        let errMsg = "";
        if (!newNote.permissions || !newNote.permissions.EDIT || !newNote.permissions.REMOVE || !newNote.permissions.COMPLETE) setErrorMessage("Invalid note permission settings. Please refresh the page!");
        for (let i = 0; i < teamNoteConfig.action_config.length; ++i) {
            const action = teamNoteConfig.action_config[i];
            if (newNote.permissions[action].length === 0) errMsg = `Please select at least one permission for the ${action} action!`;
        }
        if (!newNote.message || newNote.message === "") errMsg = "Note message cannot be empty!";
        if (!newNote.priority || newNote.priority === "") errMsg = "Please set a priority for this note!";
        if (!newNote.title || newNote.title === "") errMsg = "Please give a title to this note!";
        if (!newNote.team_name || newNote.team_name === "") errMsg = "Please select a team!";
        setErrorMessage(errMsg);
        if (errMsg !== "") {
            setIsUpdating(false);
            return;
        }

        await configureItem(newNote);
        setIsUpdating(false);
    };



    const editorContent = () => (
        <>
            <div>
                <span>Team: </span>
                <select className="query-field-input select" value={team} onChange={(e) => { setTeam(e.target.value); }}>
                    {/*<option disabled value=""> -- select an option -- </option>*/}
                    {userTeams.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
                </select>
                <span>  </span>

                <span>Title: </span>
                <input className="query-field-input" id="noteTitle" name="noteTitle" value={noteTitle} onChange={(e) => { setNoteTitle(e.target.value); }} />
                <span>  </span>

                <span>Priority: </span>
                {priorityList.map((p) =>
                    <label key={p}>
                        <input type="radio" name="priorityRadio" value={p} checked={priority === p} onChange={(e) => { setPriority(e.currentTarget.value); }} />{p}
                    </label>)}
            </div>
            <div>
                <label>Note: </label>
                <br />
                <textarea className="modal-textarea" name="notemessagetext" value={message} onChange={e => setMessage(e.target.value)} disabled={isUpdating}></textarea>
                <br />
            </div>
            <div style={{display: "flow-root"}}>
                <label>Choose Permissions: </label>
                <br />
                {teamNoteConfig.action_config.map(action =>
                    <div key={action} className="column c30 query-entry-block">
                        <div>{action}</div>
                        {permissionList.map(entry =>
                            <div key={action + "_" + entry[0]}>
                                <label>
                                    <input type="checkbox" checked={permissionCheckMap[action][entry[0]]} onChange={() => { handlePermissionChange(action, entry[0]); }} />{entry[1]}
                                </label>
                            </div>)}
                    </div>)
                }
            </div>
            <div>
                <span className="text-error">{errorMessage}</span>
            </div>
        </>
    );


    const configureActionButtons = () => (
        <>
            {actionButtons()}
            <button className="button-small remove-icon" onClick={configureItemSafe} disabled={isUpdating || isTeamsDataPending}>{configurationType}</button>
        </>
    );

    return (
        <DashboardModalBase title={title} content={editorContent} foot_note={Array.from({ length: priorityNumber + 1 }, (_, i) => '*').join('')} actionButtons={configureActionButtons} />
    );
};

export default DashboardModalConfigureNote;

