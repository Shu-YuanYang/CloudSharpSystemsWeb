import { useState, useEffect } from "react";
import UserProfileSimple from "./UserProfileSimple";
import DragScrollable from "../drag_and_drop/DragScrollable";
import { APIEndpoints } from "../site_config.json";
import { api_full_path_with_query, get_api, api_authorized_get } from "../endpoints/api/api_helper";
import { useContext } from 'react';
import IdentityContext from "../auxiliary/wrappers/IdentityContext";



const TeamUserProfilesSimpleSubsection = ({ teamName }) => {

    const userIdentity = useContext(IdentityContext);

    const [teamUserProfiles, setTeamUserProfiles] = useState([]);

    useEffect(() => {
        let encoded_team_name = encodeURIComponent(teamName);
        api_authorized_get(api_full_path_with_query(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_identity_users_by_team").path, `app_id=CloudSharpVisualDashboard&encoded_team_name=${encoded_team_name}`), userIdentity ? userIdentity.session_id : null)
            .then(data => { setTeamUserProfiles(data); })
            .catch(error => console.error(error));
    }, [teamName, userIdentity]);


    return (
        <div className="dashboard-subsection">
            <div className="subsection title-small"><span>In this Team: ({teamName})</span></div>
            <DragScrollable className="scroll-control-x">
                <div className="nav-item no-pad">
                    {teamUserProfiles.map(userProfile => 
                        <UserProfileSimple key={userProfile.USERNAME_ALIAS} username={userProfile.USERNAME_ALIAS} name={userProfile.NAME_ALIAS} />
                    )}
                </div>
            </DragScrollable>
        </div>
    );
}

export default TeamUserProfilesSimpleSubsection;