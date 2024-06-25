import { LOCAL_STORE } from "./local_storage";
import { useState, useCallback, useEffect } from "react";
import { api_authorized_get, api_full_path, get_api } from "../api/api_helper";
import { APIEndpoints } from "../../site_config.json";


const useIdentity = () => {

    const [userAuthInfo, setUserAuthInfo] = useState(() => {
        let auth_item = localStorage.getItem(LOCAL_STORE.AUTH);
        if (auth_item) auth_item = JSON.parse(auth_item);
        console.log(`auth: initial authInfo: `, auth_item);
        return auth_item;
    });

    // userIdentity must contain { session_id: string }
    const [userIdentity, setUserIdentity] = useState(() => {
        let identity_item = localStorage.getItem(LOCAL_STORE.IDENTITY);
        if (identity_item) identity_item = JSON.parse(identity_item);
        console.log(`auth: initial identity: `, identity_item);
        return identity_item;
    });


    //const [loginWindow, setLoginWindow] = useState(null);

    const [isLoginInfoUpdating, setIsLoginInfoUpdating] = useState(false);
    const [loginInfoMessage, setLoginInfoMessage] = useState("");

    //const [userLoginState, setUserLoginState] = useState("");


    const SEPSignOut = useCallback(() => {

        setIsLoginInfoUpdating(true);
        setLoginInfoMessage("Signing Out...");

        localStorage.removeItem(LOCAL_STORE.AUTH);
        localStorage.removeItem(LOCAL_STORE.IDENTITY);
        setUserIdentity(() => {
            console.log("auth: logged out identity: ", null);
            return null;
        });
        setUserAuthInfo(() => {
            console.log("auth: logged out authInfo: ", null);
            return null;
        });

        api_authorized_get(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "auth_gcp_logout").path), userIdentity.session_id)
            .then((data) => { console.log("auth: logout response: ", data); })
            .catch((err) => { console.log("auth: backend logout failed: ", err); })
            .finally(() => {
                setIsLoginInfoUpdating(false);
                setLoginInfoMessage("");
            });

    }, [userIdentity]);


    // Authenticate with local storage:
    const authenticateIdentity = useCallback(() => {

        const current_identity = localStorage.getItem(LOCAL_STORE.IDENTITY);
        if (!current_identity && (userIdentity || userAuthInfo)) {
            console.log("auth: No login info!");
            SEPSignOut();
        }
        else if (current_identity && !userIdentity) {
            console.log("auth: restore identity: ", current_identity);
            setUserIdentity(JSON.parse(current_identity));
        }
    }, [userIdentity, userAuthInfo, SEPSignOut]);


    // Periodically check local storage for identity (restore if necessary):
    useEffect(() => {
        const check_interval = setInterval(() => {
            authenticateIdentity();
        }, 5000);

        return () => clearInterval(check_interval); // This represents the unmount function, in which you need to clear your interval to prevent memory leaks.
    }, [authenticateIdentity]);




    // Use identity access token to get user profile data:
    useEffect(
        () => {
            if (!userIdentity) return;
            setIsLoginInfoUpdating(true);
            setLoginInfoMessage("Updating Profile...");

            api_authorized_get(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_identity_user_profile").path), userIdentity.session_id)
                .then((data) => {
                    if (!data) return;
                    localStorage.setItem(LOCAL_STORE.AUTH, JSON.stringify(data));
                    setUserAuthInfo(data);
                    console.log("auth: authenticated identity: ", userIdentity);
                    console.log("auth: authenticated authInfo: ", data);

                    setIsLoginInfoUpdating(false);
                    setLoginInfoMessage("");
                })
                .catch((err) => {
                    alert("auth: authentication failed!");
                    console.log("auth: authentication failed: ", err);
                    SEPSignOut();
                });
        },
        [userIdentity, SEPSignOut]
    );

    return { userIdentity, userAuthInfo, authenticateIdentity, SEPSignOut, isLoginInfoUpdating, loginInfoMessage };

};

export default useIdentity;