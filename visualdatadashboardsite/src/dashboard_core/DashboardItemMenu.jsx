import { useState, useEffect } from "react";
import { SortableList } from "../drag_and_drop/SortableList";
import PaddedComponentWrapper from "../auxiliary/wrappers/PaddedComponentWrapper";
import DashboardItemMenuHeader from "./DashboardItemMenuHeader";
import DashboardItemMenuSelectArea from "./DashboardItemMenuSelectArea";
import { __MakeSortableListData, __RemoveItemFromSortableListData, __AddItemToSortableListData } from "../drag_and_drop/drag_and_drop_helper";
import DashboardTagCard from "./DashboardTagCard";
import DashboardModalConfigureMenuItem from "./DashboardModalConfigureMenuItem";
//import RemoveImg from "../assets/remove.png";



/**
 * 
 * @param 
 *      title: string,
 *      data_obj: {
 *          sortableDataArray: Array, 
 *          selectableDataArray: Array, 
 *          refreshData: function, 
 *          saveData: async function, 
 *          isDataPending: boolean, 
 *          isSelectable: boolean
 *          dataIdentifier: function,
 *      },
 *      itemElement: component function
 * @returns DashboardItemMenu component
 */
const DashboardItemMenu = ({ title, data_obj, itemElement, isVerticalDisplay = true, allowNewItemAdding = true }) => {

    const [isInEditMode, setIsInEditMode] = useState(false);
    const [isInAddMode, setIsInAddMode] = useState(false);
    const [identifiableSortableData, setIdentifiableSortablData] = useState([]);
    const [identifiableSelectData, setIdentifiableSelectData] = useState([]);
    const [identifiableDeletedData, setIdentifiableDeletedData] = useState([]);
    const [isDataSaving, setIsDataSaving] = useState(false);

    useEffect(() => {
        if (data_obj.sortableDataArray) {
            const comp_func = (a, b) => { return (data_obj.dataIdentifier(a) < data_obj.dataIdentifier(b)) ? -1 : 1; };
            setIdentifiableSortablData(__MakeSortableListData(data_obj.sortableDataArray, data_obj.dataIdentifier));
            setIdentifiableSelectData(__MakeSortableListData(data_obj.selectableDataArray, data_obj.dataIdentifier).sort(comp_func));
            setIdentifiableDeletedData(__MakeSortableListData([], null));
        }
        else {
            setIdentifiableSortablData(__MakeSortableListData([], null));
            setIdentifiableSelectData(__MakeSortableListData([], null));
            setIdentifiableDeletedData(__MakeSortableListData([], null));
        }

    }, [data_obj]);


    // function to refresh and restore the UI to static state:  
    const hard_refresh = () => {
        data_obj.refreshData();
        setIsInEditMode(false);
    };

    // function to remove an item from the local data list:
    const remove_item = (item) => {
        let copied_item = { ...item };
        //console.log(copied_item);
        const comp_func = (a, b) => { return (data_obj.dataIdentifier(a) < data_obj.dataIdentifier(b))? -1 : 1; };
        let new_arr = __AddItemToSortableListData(identifiableSelectData, copied_item, data_obj.dataIdentifier);
        if (data_obj.dataIdentifier) new_arr.sort(comp_func);
        setIdentifiableSelectData(new_arr);
        setIdentifiableSortablData(__RemoveItemFromSortableListData(identifiableSortableData, item, data_obj.dataIdentifier));
    };

    const delete_item = (item) => {
        let copied_item = { ...item };
        const comp_func = (a, b) => { return (data_obj.dataIdentifier(a) < data_obj.dataIdentifier(b)) ? -1 : 1; };
        let new_arr = __AddItemToSortableListData(identifiableDeletedData, copied_item, data_obj.dataIdentifier);
        if (data_obj.dataIdentifier) new_arr.sort(comp_func);
        setIdentifiableDeletedData(new_arr);
        setIdentifiableSelectData(__RemoveItemFromSortableListData(identifiableSelectData, item, data_obj.dataIdentifier));
    }

    const include_item = (item) => {
        let copied_item = { ...item };
        setIdentifiableSortablData(__AddItemToSortableListData(identifiableSortableData, copied_item, data_obj.dataIdentifier));
        setIdentifiableSelectData(__RemoveItemFromSortableListData(identifiableSelectData, item, data_obj.dataIdentifier));
    };

    // function to switch between static and editing state:
    const ChangeOnClick = () => {
        if (isInEditMode) {
            setIsDataSaving(true);
            data_obj.saveData(identifiableSortableData, identifiableSelectData, identifiableDeletedData)
                .catch((err) => {
                    console.log("Data saving failed!");
                    console.log(err);
                }).finally(() => {
                    setIsDataSaving(false);
                    hard_refresh();
                });
        }
        else
        {
            setIsInEditMode(!isInEditMode);
        }
        
    };

    const AddNewItemOnClick = async (formData) => {
        let response = await data_obj.addItem(formData);
        hard_refresh();
        setIsInAddMode(false);
        return response;
    }


    // map element with local removal button functionality to render:
    const itemElementMap = ({ item }) => (
            <PaddedComponentWrapper>
                {itemElement({ item: item })}
                {isInEditMode && data_obj.isSelectable && 
                    <div className="card-editor">
                        <div className="card-button">
                            <button className="button-small remove-icon" onClick={() => { remove_item(item); }}>{"x"}</button>
                        </div>
                    </div>
                }
                {isInEditMode && data_obj.hasMovableHint &&
                    <div className="card-editor">
                        <div className="card-button">
                            <div className="button-small move-icon" >:::</div>
                        </div>
                    </div>
                }
            </PaddedComponentWrapper>
    );

    const selectableItemElementMap = ({ item }) => (
        <PaddedComponentWrapper>
            {itemElement({ item: item })}
            <div className="card-editor">
                <div className="card-button">
                    {<button className="button-small remove-icon" onClick={() => { delete_item(item); }}>{"delete"}</button>}
                    <button className="button-small add-icon" onClick={() => { include_item(item); }}>{"+"}</button>
                </div>
            </div>
        </PaddedComponentWrapper>
    );

    const addItemElementMap = () => (
        <PaddedComponentWrapper size="medium">
            <div onClick={() => { /*change_action_mode(item, "ADD");*/ }}>
                <DashboardTagCard callback={() => { setIsInAddMode(true); }} directory={"assets"} img_path={"add.png"} is_highlighted={false} />
            </div>
        </PaddedComponentWrapper>
    );

    const newItemActionButtons = () => (
        <button className="button-small remove-icon" onClick={() => { setIsInAddMode(false); }} disabled={isDataSaving}>Cancel</button>
    );

    
    const MenuComponenet = (
        <div className={`container ${isVerticalDisplay ? "full-height" : "full-width"}`}>
            <div className={isVerticalDisplay ? "selectable-menu c50" : `${isInEditMode ? "selectable-menu-horizontal" : "menu-horitontal"}`}>
                <div className={`${isVerticalDisplay ? "dashboard scroll-control-y" : "dashboard-square scroll-control-x"}`}>
                    <DashboardItemMenuHeader title={title} className={`${isVerticalDisplay ? "menu-header" : "menu-header-square"} sticky`}>
                        {!data_obj.isDataPending &&
                            <div className="card-editor">
                                <div className="card-button">
                                    {isDataSaving ?
                                        (
                                            <div>
                                                <span>Saving...</span>
                                            </div>
                                        )
                                        :
                                        data_obj.sortableDataArray && (
                                            <div>
                                                <button className="button-small" onClick={ChangeOnClick}>{isInEditMode ? "Save" : "Edit"}</button>
                                                <button className={`button-small ${/*isInEditMode ? "" : "refresh-icon"*/""}`} onClick={hard_refresh}>{isInEditMode ? "Cancel" : "Refresh"}</button>
                                            </div>
                                        )
                                    }
                                </div>
                            </div>
                        }
                    </DashboardItemMenuHeader>

                    {data_obj.isDataPending?
                        (<div><span>Loading...</span></div>) :
                        (<SortableList dataArray={identifiableSortableData} setDataArray={setIdentifiableSortablData} itemElement={itemElementMap} addItemElement={allowNewItemAdding? addItemElementMap : undefined} isInEditMode={isInEditMode} isVerticalDisplay={isVerticalDisplay} />)
                    }
                </div>
            </div>

            {isInEditMode && data_obj.isSelectable && 
                <div className={isVerticalDisplay ? "selectable-menu c50" : "selectable-menu-horizontal"}>
                    <div className={isVerticalDisplay ? "menu-select-section scroll-control-y" : "menu-select-section-horizontal scroll-control-x"}>
                        <DashboardItemMenuSelectArea data={identifiableSelectData} setData={setIdentifiableSelectData} isDataPending={data_obj.isDataPending} itemElement={selectableItemElementMap} isVerticalDisplay={isVerticalDisplay} />
                    </div>
                </div>
            }

            {isInAddMode && <DashboardModalConfigureMenuItem title={data_obj.title} actionButtons={newItemActionButtons} configurationType="Add" configureItem={AddNewItemOnClick} />}
        </div>
    );




    // Return menu bar:
    return MenuComponenet;
};

export default DashboardItemMenu;