import { LOCAL_STORE } from "./local_storage";
import { useState, useEffect } from "react";


const useIdentityState = () => {

    // userIdentity must contain { session_id: string }
    const [userIdentity, setUserIdentityState] = useState(() => {
        let identity_item = localStorage.getItem(LOCAL_STORE.IDENTITY);
        if (identity_item) return JSON.parse(identity_item);
        return null;
    });

    /*
    const setUserIdentity = (value) => {
        setUserIdentityState(value);
        localStorage.setItem(LOCAL_STORE.IDENTITY, JSON.stringify(value));
    };
    */

    // Periodically check local storage for identity (restore if necessary):
    useEffect(() => {
        const check_interval = setInterval(() => {
            let current_identity = localStorage.getItem(LOCAL_STORE.IDENTITY);
            if (current_identity) current_identity = JSON.parse(current_identity);
            if (current_identity?.session_id !== userIdentity?.session_id) setUserIdentityState(current_identity); //setUserIdentity(current_identity);
        }, 500);

        return () => clearInterval(check_interval); // This represents the unmount function, in which you need to clear your interval to prevent memory leaks.
    }, [userIdentity]);

    return { userIdentity };
};

export default useIdentityState;
