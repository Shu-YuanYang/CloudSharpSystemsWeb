import { useState, useMemo, useEffect } from "react";
import { useLocation } from "react-router-dom";
import { LOCAL_STORE } from "../endpoints/local_asset_load/local_storage";







function useQuery() {
    const { search } = useLocation();
    return useMemo(() => new URLSearchParams(search), [search]);
}


const OAuth2LoginSuccessPage = () => {

    const query = useQuery();

    const userState = useMemo(() => {
        const user_state = query.get("state");
        return user_state ? user_state : "";
    }, [query]);

    const sessionID = useMemo(() => {
        let full_url = window.location.href;
        let start_index = full_url.indexOf("#session_id=") + "#session_id=".length;
        if (start_index === -1) return "";
        let end_index = full_url.length;
        let temp_end_index = end_index; //full_url.lastIndexOf("&");
        while (start_index < temp_end_index) {
            end_index = temp_end_index;
            temp_end_index = full_url.lastIndexOf("&");
        }
        let session_id = full_url.substring(start_index, end_index);
        session_id = decodeURIComponent(session_id);
        
        return session_id;
    }, []);

    const [loginResult, setLoginResult] = useState(null);

    useEffect(() => {

        // TODO: After 5 seconds, if authentication info is still incomplete, fail the authentication.

        if (userState != "" && sessionID != "") {
            localStorage.setItem(LOCAL_STORE.IDENTITY, JSON.stringify({ session_id: sessionID }));
            let messageObj = {
                message_type: "auth",
                event: "login",
                status: "",
                state: userState,
                message: ""
            };
            try {
                messageObj.status = "success";
                messageObj.message = "auth: login info returned.";
            } catch {
                console.error("auth: login window closing failed!");
                messageObj.status = "failure";
                messageObj.message = "auth: login info retrieving failed!";
            } finally {
                setLoginResult(messageObj);
                window.opener.postMessage(JSON.stringify(messageObj), window.origin);
            }
        }

    }, [userState, sessionID]);



    return (
        <>
            {
                (userState == "" || sessionID == "") ?
                    <div>Retrieving login data...</div>
                    :
                    <div>{ loginResult ? loginResult.message : "Login info retrieved." }</div>
            }
        </>
    );

};

export default OAuth2LoginSuccessPage;
