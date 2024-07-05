import { useState, useEffect, useCallback } from "react";
//import { useNavigate } from 'react-router-dom';
//import { googleLogout, useGoogleLogin } from '@react-oauth/google';
import { get_api, api_full_path_with_query } from "../endpoints/api/api_helper";
import { APIEndpoints } from "../site_config.json";
import { generateRandomString } from "../auxiliary/math/random";
import DashboardProfileButton from "./DashboardProfileButton";
import DashboardProfileNavItemUserInfo from "./DashboardProfileNavItemUserInfo";
import useIdentity from "../endpoints/local_asset_load/useIdentity";



function DashboardProfileNavItem() {


    const { userAuthInfo, authenticateIdentity, SEPSignOut, isLoginInfoUpdating, loginInfoMessage } = useIdentity();
    
    const [loginWindow, setLoginWindow] = useState(null);
    
    const [userLoginState, setUserLoginState] = useState("");


    // Listen to login pop up window
    useEffect(() => {

        window.onmessage = (e) => {
            if (e.origin !== window.origin) {
                console.log("ORIGIN MISTACH!");
                //return;
            }

            if (loginWindow == null) return; // most likely spam message

            let returned_message = JSON.parse(e.data);
            if (returned_message.message_type !== "auth" || returned_message.event !== "login") return;
            if (returned_message.status !== "success") {
                console.error("Something went wrong with retrieving login info: ", returned_message);
                return;
            }
            if (returned_message.state !== userLoginState) {
                console.error("SECURITY ERROR: USER LOGIN STATE MISMATCH!", returned_message);
                location.reload(); // force refresh to prevent security degradation
                return;
            }
            loginWindow.close();
            setLoginWindow(null);

            authenticateIdentity();
        };

    }, [loginWindow, userLoginState, authenticateIdentity]);


    // Function to trigger pop up Google sign-in window:
    const googleSEPSignIn = () => {

        let rand_str = generateRandomString(32);
        setUserLoginState(rand_str);

        let encoded_return_url = encodeURIComponent(`${window.origin}/auth/login_success`);

        let login_path = api_full_path_with_query(
            APIEndpoints.CloudSharpLimitedCentral.url,
            get_api(APIEndpoints.CloudSharpLimitedCentral, "auth_gcp_login").path,
            `return_redirect_uri=${encoded_return_url}&state=${rand_str}`
            //`return_redirect_uri=https%3A%2F%2Fwonderful-forest-09421e010.5.azurestaticapps.net%2Fauth%2Flogin_success&state=${rand_str}`
        );
        let new_window = window.open(login_path, "AuthWindow", "popup");
        setLoginWindow(new_window);

    }



    return (
        <div className="card-editor">
            <div className="card-account-item">
                <div className="nav-item">
                    {userAuthInfo ?
                        (
                            <>
                                <DashboardProfileNavItemUserInfo authInfo={userAuthInfo} />
                                <DashboardProfileButton onClick={SEPSignOut} authInfo={userAuthInfo} disabled={false}>Log out</DashboardProfileButton>
                            </>
                        )
                        :
                        <DashboardProfileButton onClick={googleSEPSignIn} authInfo={userAuthInfo} disabled={isLoginInfoUpdating}>{isLoginInfoUpdating ? loginInfoMessage : "Sign in"}</DashboardProfileButton>
                    }
                </div>
            </div>
        </div>
    );

}

export default DashboardProfileNavItem;