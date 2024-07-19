import { useState } from "react";
import { LOCAL_STORE } from "../endpoints/local_asset_load/local_storage";

import "../styles/Dashboard.css";



const ExternalURLProtectionPage = () => {

    const [externalLinkItem, setExternalLinkItem] = useState(() => {
        let item = localStorage.getItem(LOCAL_STORE.REQUESTED_EXTERNAL_LINK);
        if (item) item = JSON.parse(item);  
        return item;
    });

    const removeRequestedLink = () => {
        localStorage.removeItem(LOCAL_STORE.REQUESTED_EXTERNAL_LINK);
    };

    return (
        <>
            <div className="title">
                <span>You are about to leave the CloudSharp Dashboard website.</span>
            </div>
            <div className="table large">
                <span>Are you sure you want to visit the following address?</span>
                <br />
                <span>Info: {externalLinkItem.info}</span>
                <br />
                <span>Link: </span><a>{externalLinkItem.url}</a>
            </div>
            <div>
                {/*<input type="checkbox">Do not ask again.</input>*/}
            </div>
            <div>
                <button className="button-small add-icon" onClick={() => { removeRequestedLink(); window.location.href = "/"; } }>No, return to home page</button>
                <button className="button-small add-icon" onClick={() => { removeRequestedLink(); window.location.href = externalLinkItem.url; } }>Yes, visit the link</button>
            </div>
        </>
        
    );

};


export default ExternalURLProtectionPage;