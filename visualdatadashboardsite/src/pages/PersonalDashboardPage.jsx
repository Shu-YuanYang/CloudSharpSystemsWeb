import { useState, useMemo, useEffect, useCallback } from "react";
import useFetch from "../endpoints/api/useFetch";
import useAuthorizedFetch from "../endpoints/api/useAuthorizedFetch";
import { api_full_path, api_full_path_with_query, api_authorized_post, api_authorized_post_form_data, get_api } from "../endpoints/api/api_helper";
import { APIEndpoints } from "../site_config.json";
import { useContext } from 'react';
import IdentityContext from "../auxiliary/wrappers/IdentityContext";

import { usePageTitle } from "./page_title_converter";
import DashboardMonitor from "../dashboard_core/DashboardMonitor";
import DashboardItemMenu from "../dashboard_core/DashboardItemMenu";
import DashboardNoteArea from "../dashboard_core/DashboardNoteArea";
import DashboardLinkCard from "../dashboard_core/DashboardLinkCard";
import DashboardTagCard from "../dashboard_core/DashboardTagCard";
import DashboardNoteCard from "../dashboard_core/DashboardNoteCard";

import "../styles/Dashboard.css";




const PersonalDashboardPage = () => {

    usePageTitle("Monitor");

    const userIdentity = useContext(IdentityContext);

    const dataIdentifier = (row) => { return row.DISPLAY_NAME; };

    const addNewMenuItem = async (formData) => {
        for (let entry of formData.entries()) console.log(entry);
        return await api_authorized_post_form_data(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "add_new_menu_item").path), userIdentity.session_id, formData);
    };

    const saveMenuData = async (newSortableData, newSelectableData, newDeletedData) => {
        //console.log(newSortableData);
        //console.log(newSelectableData);
        for (let i = 0; i < newSortableData.length; ++i) newSortableData[i].RANKING = i + 1;
        for (let i = 0; i < newSelectableData.length; ++i) newSelectableData[i].RANKING = -1;
        for (let i = 0; i < newDeletedData.length; ++i) {
            newDeletedData[i].RANKING = -1;
            newDeletedData[i].ROUTE = "DELETED"; // mark as deleted for backend
        }
        let changedItemsArray = [...newSortableData, ...newSelectableData, ...newDeletedData];
        changedItemsArray.forEach((item) => { delete item.ICON });  // remove the icon url path to save bytes transmitted to backend 
        console.log(changedItemsArray);
        await api_authorized_post(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "update_sortable_menu").path), userIdentity.session_id, changedItemsArray);
    };

    //const [data, setData] = useState(makeSortablePageData());
    //const dataIdentifier = (row) => { return row.page_name; };



    // Download and organise links menu data:
    //const [linksData, setLinksData] = useState(makeSortablePageData());
    //const refreshLinksData = () => { setLinksData(makeSortablePageData()); };
    //const isLinksDataPending = false;
    //const linksDataFetchError = null;
    const { data: linksData, refreshData: refreshLinksData, isPending: isLinksDataPending, error: linksDataFetchError } = useAuthorizedFetch(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_sortable_links_menu").path), userIdentity? userIdentity.session_id : null);
    
    const links_comp_func = (a, b) => { return (a.RANKING < b.RANKING) ? -1 : 1; };

    const sortableLinksData = useMemo(() => linksData ? linksData.filter(row => row.RANKING > 0).sort(links_comp_func) : linksData, [linksData]);
    const selectableLinksData = useMemo(() => linksData ? linksData.filter(row => row.RANKING < 0) : linksData, [linksData]);

    const linkItemElement = ({ item }) => {
        const icon_directory = ""; // api_full_path(APIEndpoints.CloudSharpDashboardGCPStorage.url, get_api(APIEndpoints.CloudSharpDashboardGCPStorage, item.MENU_DISPLAY_NAME).path);
        return (
            <DashboardLinkCard title={item.DISPLAY_NAME} link_path={item.ROUTE} directory={icon_directory} img_path={item.ICON} size_type={"icon-img-small"} />
        );
    };

    const link_menu_data_obj = {
        title: "Links",
        sortableDataArray: sortableLinksData,
        selectableDataArray: selectableLinksData,
        refreshData: refreshLinksData,
        addItem: addNewMenuItem,
        saveData: saveMenuData,
        isDataPending: isLinksDataPending,
        isSelectable: true,
        dataIdentifier: dataIdentifier
    };




    // Download and organise chart menu data:
    const [isChartMenuExpanded, setIsChartMenuExpanded] = useState(true);
    const [isChartMenuInSelectMode, setIsChartMenuInSelectMode] = useState(false);
    const switchChartsMenuMode = () => { setIsChartMenuExpanded(!isChartMenuExpanded); }

    const [currentChartItem, setCurrentChartItem] = useState(null);

    //const [chartsData, setChartsData] = useState(makeSortableChartData());
    //const refreshChartsData = () => { setChartsData(makeSortableChartData()); };
    //const isChartsDataPending = false;
    //const chartsDataFetchError = null;
    const { data: chartsData, refreshData: refreshChartsData, isPending: isChartsDataPending, error: chartsDataFetchError } = useAuthorizedFetch(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_sortable_charts_menu").path), userIdentity ? userIdentity.session_id : null);

    const charts_comp_func = (a, b) => { return (a.RANKING < b.RANKING) ? -1 : 1; };

    const sortableChartsData = useMemo(() => chartsData ? chartsData.filter(row => row.RANKING > 0).sort(charts_comp_func) : chartsData, [chartsData]);
    const selectableChartsData = useMemo(() => chartsData ? chartsData.filter(row => row.RANKING < 0) : chartsData, [chartsData]);

    const chartItemElement = ({ item }) => {
        const icon_directory = ""; // api_full_path(APIEndpoints.CloudSharpDashboardGCPStorage.url, get_api(APIEndpoints.CloudSharpDashboardGCPStorage, item.MENU_DISPLAY_NAME).path);
        return (
            <DashboardTagCard title={item.DISPLAY_NAME} callback={() => { setCurrentChartItem(item); }} directory={icon_directory} img_path={item.ICON} is_highlighted={currentChartItem.DISPLAY_NAME === item.DISPLAY_NAME} />
        );
    };

    const chart_menu_data_obj = {
        title: "Charts",
        sortableDataArray: sortableChartsData,
        selectableDataArray: selectableChartsData,
        refreshData: refreshChartsData,
        addItem: addNewMenuItem,
        saveData: saveMenuData,
        isDataPending: isChartsDataPending,
        isSelectable: true,
        dataIdentifier: dataIdentifier
    };
    
    useEffect(() => {
        // When chart menu refreshes, use the first
        if (sortableChartsData && sortableChartsData.length > 0) {
            setCurrentChartItem(sortableChartsData[0]);
        } else {
            setCurrentChartItem(null);
        }
    }, [sortableChartsData]);



    // Download and organise notes data:
    const { data: notesConfig, refreshData: refreshNotesConfig, isPending: isNotesConfigPending, error: notesConfigFetchError } = useFetch(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_team_note_config").path));
    const { data: notesData, refreshData: refreshNotesData, isPending: isNotesDataPending, error: notesDataFetchError } = useAuthorizedFetch(api_full_path_with_query(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_notes_for_user").path, "app_id=CloudSharpVisualDashboard"), userIdentity ? userIdentity.session_id : null);
    
    const notes_comp_func = (a, b) => {
        if (a.status_code === "TODO") { // TODO status always takes priority
            if (b.status_code === "TODO") return (a.priority_number > b.priority_number) ? -1 : 1;
            return -1;
        }
        if (b.status_code === "TODO") return 1;
        return (a.status_code === "COMPLETED")? -1 : 1; // view completed items first, and then the ones removed
    };
    
    const selectableNotesData = useMemo(() => notesData && notesConfig ? notesData.map(note => ({ ...note, priority_number: notesConfig.priority_config[note.priority], status: notesConfig.status_config[note.status_code] })).sort(notes_comp_func) : [], [notesData, notesConfig]);

    const noteItemElement = ({ item }) => {
        return (
            <DashboardNoteCard title={item.title} team_name={item.team_name} sender={item.sender_name} message={item.message} priority_number={item.priority_number} status_code={item.status_code} is_highlighted={false} size_type={"icon-img-extra-small"} />
        );
    };

    const noteDataIdentifier = useCallback((row) => { return row.note_id; }, []);

    const notes_data_obj = useMemo(() => ({
        title: "Notes",
        //sortableDataArray: sortableLinksData,
        selectableDataArray: selectableNotesData,
        refreshData: refreshNotesData,
        addNote: async (newNote) => {
            return await api_authorized_post(api_full_path_with_query(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "add_note").path, "app_id=CloudSharpVisualDashboard"), userIdentity.session_id, newNote);
        },
        modifyNote: async (newNote, updateType) => {
            return await api_authorized_post(api_full_path_with_query(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "update_note").path, `update_type=${updateType}`), userIdentity.session_id, newNote);
        },
        isDataPending: isNotesDataPending,
        /*isSelectable: true,*/
        dataIdentifier: noteDataIdentifier,
        teamNoteConfig: notesConfig
    }), [selectableNotesData, refreshNotesData, isNotesDataPending, noteDataIdentifier, userIdentity, notesConfig]);




    const ExpandableChartsMenuMap = () => (
        <div className="container full-height">
            <div className="card-editor">
                <div className="card-button-center">
                    <button className="button-small" onClick={switchChartsMenuMode}>{isChartMenuExpanded ? "Collapse" : "Expand"}</button>
                </div>
            </div>
            {isChartMenuExpanded && !chartsDataFetchError && <DashboardItemMenu title={"Charts"} data_obj={chart_menu_data_obj} itemElement={chartItemElement} isVerticalDisplay={false} allowNewItemAdding={userIdentity != null} selectModeControl={{ isInSelectMode: isChartMenuInSelectMode, setIsInSelectMode: setIsChartMenuInSelectMode }} />}
        </div>
    );


    // Area to show notes:
    const BackPositionedNoteMap = () => (
        <div className="card-editor container full-height c15 column-right">
            <div className="card-note-area">
                <div className="board component">
                    <DashboardNoteArea title={"Notes"} data_obj={notes_data_obj} itemElement={noteItemElement} isVerticalDisplay={true} />
                </div>
            </div>
        </div>
    );


    const chartMenuHeightStyle = isChartMenuExpanded ? (isChartMenuInSelectMode ? "r50" : "r25") : "r0";
    const monitorHeightStyle = isChartMenuExpanded? (isChartMenuInSelectMode ? "r50" : "r75") : "container full-height";

    return (
        <div className="board main">
            <div className="container full-height">
                <div className="page title">
                    <span>Dashboard Home</span>
                </div>
                <div className="r95">
                    <div className="container full-height">
                        <div className="column c70">
                            <div className="board component">
                                <div className={"container position-relative full-height"} >
                                    <div className={monitorHeightStyle/*"rauto rmax77"*/}>
                                        <DashboardMonitor title="Monitor" currentComponentData={currentChartItem} />
                                    </div>
                                    <div className={`${chartMenuHeightStyle} dashboard-subsection`}>
                                        <ExpandableChartsMenuMap />
                                    </div>
                                </div>

                            </div>
                        </div>

                        <div className="column c30">
                            <div className="board component">
                                {!linksDataFetchError && <DashboardItemMenu title={"Links"} data_obj={link_menu_data_obj} itemElement={linkItemElement} isVerticalDisplay={true} allowNewItemAdding={userIdentity != null} />}
                            </div>
                        </div>

                        <BackPositionedNoteMap />
                        
                    </div>
                </div>
            </div>
            
        </div>
    )
};

export default PersonalDashboardPage