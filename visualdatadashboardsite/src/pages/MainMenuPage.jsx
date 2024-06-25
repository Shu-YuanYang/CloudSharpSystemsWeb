import { useMemo, useCallback } from "react";

import useAuthorizedFetch from "../endpoints/api/useAuthorizedFetch";
import { api_full_path, get_api, api_authorized_post } from "../endpoints/api/api_helper";
import { APIEndpoints } from "../site_config.json";
import { useContext } from 'react';
import IdentityContext from "../auxiliary/wrappers/IdentityContext";

import { usePageTitle } from "./page_title_converter";
import DashboardItemMenu from "../dashboard_core/DashboardItemMenu";
import DashboardLinkCard from "../dashboard_core/DashboardLinkCard";

import "../styles/Dashboard.css";




const MainMenuPage = () => {

    usePageTitle("Menu");

    const userIdentity = useContext(IdentityContext);

    // Download and organise chart menu data:
    const dataIdentifier = (row) => { return row.DISPLAY_NAME; };

    const { data: menuListData, refreshData: refreshMenuListData, isPending: isMenuListDataPending, error: menuListDataFetchError } = useAuthorizedFetch(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_menu_list").path), userIdentity ? userIdentity.session_id : null);

    const charts_comp_func = (a, b) => {
        let a_ranking = (a.RANKING < 0) ? 1000 : a.RANKING;
        let b_ranking = (b.RANKING < 0) ? 1000 : b.RANKING;
        if (a_ranking == b_ranking) return (a.DISPLAY_NAME < b.DISPLAY_NAME)? -1 : 1;
        return a_ranking - b_ranking;
    };

    const sortableMenuListData = useMemo(() => menuListData ? menuListData.map(menu => ({...menu, menu_items: menu.menu_items.sort(charts_comp_func) })) : [], [menuListData]);

    const saveMenuData = useCallback(async (newSortableData, newSelectableData) => {
        //console.log(newSortableData);
        //console.log(newSelectableData);
        for (let i = 0; i < newSortableData.length; ++i) newSortableData[i].RANKING = i + 1;
        for (let i = 0; i < newSelectableData.length; ++i) newSelectableData[i].RANKING = -1;
        let changedItemsArray = [...newSortableData, ...newSelectableData];
        changedItemsArray.forEach((item) => { delete item.ICON });  // remove the icon url path to save bytes transmitted to backend 
        console.log(changedItemsArray);
        await api_authorized_post(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "update_sortable_menu").path), userIdentity.session_id, changedItemsArray);
    }, [userIdentity]);

    const menu_data_obj_lst = useMemo(() => {
        let obj_lst = [];
        if (!menuListData) return obj_lst;
        for (let i = 0; i < menuListData.length; ++i) {
            obj_lst.push({
                title: sortableMenuListData[i].menu_display_name,
                sortableDataArray: sortableMenuListData[i].menu_items,
                selectableDataArray: [],
                refreshData: refreshMenuListData,
                saveData: saveMenuData,
                isDataPending: isMenuListDataPending,
                isSelectable: false,
                hasMovableHint: true,
                dataIdentifier: dataIdentifier
            });
        }
        return obj_lst;
    }, [menuListData, sortableMenuListData, refreshMenuListData, isMenuListDataPending, saveMenuData]);


    
    const MenuItemElement = ({ item }) => {
        const icon_directory = ""; // api_full_path(APIEndpoints.CloudSharpDashboardGCPStorage.url, get_api(APIEndpoints.CloudSharpDashboardGCPStorage, item.MENU_DISPLAY_NAME).path);
        return (
            <DashboardLinkCard title={item.DISPLAY_NAME} link_path={item.ROUTE} directory={icon_directory} img_path={item.ICON} />
        );
    };



    return (
        <div className="board main">
            <div className="container horizontal-centering">
                <div className="c95">
                    <div className="page title">
                        <span>Main Menu</span>
                    </div>
                    <br />
                    {/*<div className="container horizontal-centering">
                        <h1>Main Menu</h1>
                    </div>*/}
                    {
                        !menuListDataFetchError &&
                        menu_data_obj_lst.map((menu_data_obj, index) =>
                            (menu_data_obj.sortableDataArray.length > 0) ?
                                (
                                    <div key={index} className="">
                                        <DashboardItemMenu title={menu_data_obj.title} data_obj={menu_data_obj} itemElement={MenuItemElement} isVerticalDisplay={false} />
                                    </div>
                                ) :
                                (
                                    <></>
                                )
                        )
                    }
                </div>
            </div>
        </div>
    );


};

export default MainMenuPage;
