import { useState, useMemo, useEffect, useCallback } from "react";
import { APIEndpoints } from "../site_config.json";
import { api_full_path_with_query, get_api } from "../endpoints/api/api_helper";
import useFetch from "../endpoints/api/useFetch";
import DashboardModalBase from "./DashboardModalBase";
import { useContext } from 'react';
import IdentityContext from "../auxiliary/wrappers/IdentityContext";



const DashboardModalConfigureMenuItem = ({ title, actionButtons, configurationType, configureItem }) => {

    const userIdentity = useContext(IdentityContext);

    const { data: menuConfigData, refreshData: refreshMenuConfigData, isPending: isMenuConfigDataPending, error: menuConfigDataFetchError } = useFetch(api_full_path_with_query(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_menu_config").path, "app_id=CloudSharpVisualDashboard"));
    /*const [menuConfigData, setMenuConfigData] = useState({
        menu_list: [
            {
                menu_display_name: "Links",
                menu_name: "SORTABLE_LINK_MENU",
                route_type: "HTTP",
            },
            {
                menu_display_name: "Charts",
                menu_name: "SORTABLE_CHART_MENU",
                route_type: "ASSET"
            },
            {
                menu_display_name: "Tables",
                menu_name: "DATA_TABLE_MENU",
                route_type: "PAGE"
            }
        ],
        route_types: [
            { route_type: "HTTP", description: "External URL", validation_function: null },
            { route_type: "ASSET", description: "Asset", validation_function: null },
            { route_type: "PAGE", description: "Page", validation_function: null }
        ]
    });*/


    const menuList = useMemo(() => {
        let menu_lst = menuConfigData? menuConfigData.menu_list : [];
        menu_lst = menu_lst.map(menu_config => { // tentative implementation: assign a fixed route type to each menu
            let route_type = "PAGE";
            if (menu_config.MENU_NAME === "SORTABLE_LINK_MENU") route_type = "HTTP";
            if (menu_config.MENU_NAME === "SORTABLE_CHART_MENU") route_type = "ASSET";
            return { ...menu_config, route_type: route_type };
        });
        //console.log(menu_lst);
        const comp_func = (a, b) => { return (a.DISPLAY_NAME < b.DISPLAY_NAME) ? -1 : 1; };
        const sorted_menu_lst = menu_lst.sort(comp_func);
        return sorted_menu_lst;
    }, [menuConfigData]);

    const routeTypes = useMemo(() => {
        const route_type_lst = menuConfigData ? menuConfigData.route_config : [];
        const comp_func = (a, b) => { return (a.description < b.description) ? -1 : 1; };
        const sorted_route_type_lst = route_type_lst.sort(comp_func);
        return sorted_route_type_lst;
    }, [menuConfigData]);

    const [menuTitle, setMenuTitle] = useState(title);
    const [itemDisplayName, setItemDisplayName] = useState("");
    const [itemName, setItemName] = useState("");
    const [routeType, setRouteType] = useState("PAGE");
    const [route, setRoute] = useState("");
    const [iconFile, setIconFile] = useState(null);
    const [iconPreview, setIconPreview] = useState(null);

    const [isUpdating, setIsUpdating] = useState(false);
    const [errorLocation, setErrorLocation] = useState(null);
    const [errorMessage, setErrorMessage] = useState(null);

    const set_menu_by_title = useCallback((menu_title) => {
        if (!menuList) return;
        for (let i = 0; i < menuList.length; ++i) {
            if (menuList[i].DISPLAY_NAME === menu_title) {
                setMenuTitle(menu_title);
                setRouteType(menuList[i].route_type);
            }
        }
    }, [menuList]);

    useEffect(() => {
        set_menu_by_title(title);
    }, [title, set_menu_by_title]);

    const set_menu_item_name = (item_display_name) => {
        setItemDisplayName(item_display_name);
        setItemName(item_display_name.toUpperCase().replace(/ /g, "_"));
    };

    const handleIconFileChange = (event) => {
        if (!event.target.files || !event.target.files[0]) {
            setIconFile(null);
            setIconPreview(null);
            return;
        }

        setIconFile(event.target.files[0]);
        setIconPreview(URL.createObjectURL(event.target.files[0]));
        
    };

    const configureItemSafe = async () => {
        setIsUpdating(true);

        let has_error = false;
        setErrorLocation(null);
        setErrorMessage(null);
        if (!iconFile) { setErrorLocation("other"); setErrorMessage("Icon file not selected!"); has_error = true; }
        if (!route || route === "") { setErrorLocation("route"); setErrorMessage("Address/Path cannot be empty!"); has_error = true; }
        if (!itemName || itemName === "") { setErrorLocation("item"); setErrorMessage("Item ID cannot be empty!"); has_error = true; }
        if (!itemDisplayName || itemDisplayName === "") { setErrorLocation("item"); setErrorMessage("Item name cannot be empty!"); has_error = true; }
        if (!routeType || routeType === "") { setErrorLocation("other"); setErrorMessage("Address/Path cannot be empty!"); has_error = true; }
        if (!menuTitle || menuTitle === "") { setErrorLocation("other"); setErrorMessage("Menu not selected!"); has_error = true; }
        if (has_error) { setIsUpdating(false); return; }

        const formData = new FormData();
        const jsonContent = JSON.stringify({
            MENU_DISPLAY_NAME: menuTitle,
            MENU_RANKING: -2,
            ITEM_NAME: itemName,
            DISPLAY_NAME: itemDisplayName,
            ROUTE_TYPE: routeType,
            ROUTE: route,
            ICON: "",
            RANKING: -2
        });
        console.log("Json Content: ", jsonContent);
        const blob = new Blob([jsonContent], { type: "application/json" });
        formData.set("jsonData", blob);
        formData.set("iconFile", iconFile);

        try {
            await configureItem(formData);
        } catch (err) {
            if (err.message.toUpperCase().includes("ITEM ID")) setErrorLocation("item");
            else if (err.message.toUpperCase().includes("PATH")) setErrorLocation("route");
            else setErrorLocation("other");
            setErrorMessage(err.message);
        }
        setIsUpdating(false);
    };



    const editorContent = () => (
        <>
            <div>
                <div style={{ display: "flow-root" }}>
                    <div className="column c45">
                        <span>Menu: </span>
                        <select className="query-field-input select" value={menuTitle} onChange={(e) => { set_menu_by_title(e.target.value); }} disabled={true}>
                            {/*<option disabled value=""> -- select an option -- </option>*/}
                            {menuList.map((opt) => <option key={opt.MENU_NAME} value={opt.DISPLAY_NAME}>{opt.DISPLAY_NAME}</option>)}
                        </select>
                    </div>

                    <div className="column c45">
                        <span>Menu Type: </span>
                        <select className="query-field-input select" value={routeType} onChange={(e) => { setRouteType(e.target.value); }} disabled={true}>
                            {/*<option disabled value=""> -- select an option -- </option>*/}
                            {routeTypes.map((opt) => <option key={opt.route_type} value={opt.route_type}>{opt.description}</option>)}
                        </select>
                    </div>
                </div>

                <div style={{ display: "flow-root" }}>
                    <div className="column c45">
                        <span>Item Name: </span>
                        <input className="query-field-input" id="itemDisplay" name="itemDisplay" value={itemDisplayName} onChange={(e) => { set_menu_item_name(e.target.value); }} />
                    </div>

                    <div className="column c45">
                        <span>Item ID: </span>
                        <input className="query-field-input" id="itemID" name="itemID" value={itemName} onChange={(e) => { setItemName(e.target.value); }} />
                    </div>                    
                </div>
                <span className="text-error">{errorLocation === "item" && errorMessage}</span>

                <div>
                    <span>Address/Path: </span>
                    <input className="query-field-input text-box-ex-long" id="route" name="route" value={route} onChange={(e) => { setRoute(e.target.value); }} />
                </div>
                <span className="text-error">{errorLocation === "route" && errorMessage}</span>

                <div>
                    <span>Icon: </span>
                    <input type="file" onChange={handleIconFileChange} />
                    <div className="card-img">
                        <img className="icon-img" src={iconPreview} alt="preview image" />
                    </div>
                </div>

                <br />
                <span className="text-error">{errorLocation === "other" && errorMessage}</span>
            </div>
        </>
    );


    const configureActionButtons = () => (
        <>
            {actionButtons()}
            <button className="button-small remove-icon" onClick={configureItemSafe} disabled={isUpdating || isMenuConfigDataPending}>{configurationType}</button>
        </>
    );

    return (
        <DashboardModalBase title={`New ${title} Menu Item`} content={editorContent} foot_note={"Reminder: Only add frequently visited interfaces."} actionButtons={configureActionButtons} />
    );
};

export default DashboardModalConfigureMenuItem;

