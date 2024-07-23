import { useState, useEffect } from "react";
import DashboardItemMenuSelectArea from "./DashboardItemMenuSelectArea";
import { __MakeSortableListData } from "../drag_and_drop/drag_and_drop_helper";
import PaddedComponentWrapper from "../auxiliary/wrappers/PaddedComponentWrapper";
import DashboardItemMenuHeader from "./DashboardItemMenuHeader";
import DashboardModalBase from "./DashboardModalBase";
import DashboardModalConfigureNote from "./DashboardModalConfigureNote";
import TeamUserProfilesSimpleSubsection from "../profiles/TeamUserProfilesSimpleSubsection";
import DashboardTagCard from "./DashboardTagCard";
import { useContext } from 'react';
import IdentityContext from "../auxiliary/wrappers/IdentityContext";



const DashboardNoteArea = ({ title, data_obj, itemElement, isVerticalDisplay = true }) => {

    const userIdentity = useContext(IdentityContext);

    const [identifiableSelectData, setIdentifiableSelectData] = useState([]);
    const [actionItem, setActionItem] = useState(null);
    const [editedMessage, setEditedMessage] = useState("");
    const [isUpdating, setIsUpdating] = useState(false);


    const change_action_mode = (item, mode) => {
        if (!mode || mode === "") setActionItem(null);
        else {
            setActionItem({ item: item, action: mode });
            //console.log("set action mode: ", mode);
            //console.log("action item: ", item);
            setEditedMessage(item.message);
        }
    };


    // function to refresh and restore the UI to static state:  
    /*const hard_refresh = () => {
        data_obj.refreshData();
    };*/


    const add_item = async (item) => {
        setIsUpdating(true);
        let response = await data_obj.addNote(item);
        change_action_mode(item, "");
        data_obj.refreshData();
        setIsUpdating(false);
    }


    const edit_item = (item) => {
        setIsUpdating(true);
        let new_item = { ...item };
        new_item.message = editedMessage;
        data_obj.modifyNote(new_item, "EDIT")
            .then(response => {
                if (response.Status === "DISCREPANT_HASH") alert(response.Message);
                change_action_mode(item, "");
                data_obj.refreshData();
                //console.log(response);
            })
            .finally(() => {
                setIsUpdating(false);
            });
    };

    const remove_item = (item) => {
        setIsUpdating(true);
        data_obj.modifyNote(item, "REMOVE")
            .then(response => {
                if (response.Status === "DISCREPANT_HASH") alert(response.Message);
                change_action_mode(item, "");
                data_obj.refreshData();
                //console.log(response);
            })
            .finally(() => {
                setIsUpdating(false);
            });
    };

    const complete_item = (item) => {
        setIsUpdating(true);
        data_obj.modifyNote(item, "COMPLETE")
            .then(response => {
                if (response.Status === "DISCREPANT_HASH") alert(response.Message);
                change_action_mode(item, "");
                data_obj.refreshData();
                //console.log(response);
            })
            .finally(() => {
                setIsUpdating(false);
            });
    };

    const selectableItemElementMap = ({ item }) => (
        <PaddedComponentWrapper size="small">
            <div onDoubleClick={() => {
                if (item.status_code === "TODO" && item.user_permissions.can_edit) change_action_mode(item, "EDIT");
                else change_action_mode(item, "VIEW");
            }}>
                {itemElement({ item: item })}
            </div>
            {item && item.status_code === "TODO" &&
                <div className="card-editor">
                    <div className="card-button">
                        <>
                            <button className="button-small remove-icon" onClick={() => { change_action_mode(item, "REMOVE"); }} disabled={isUpdating || !item.user_permissions.can_remove}>Remove</button>
                            <button className="button-small remove-icon" onClick={() => { change_action_mode(item, "COMPLETE"); }} disabled={isUpdating || !item.user_permissions.can_complete}>Complete</button>
                        </>
                    </div>
                </div>
            }
        </PaddedComponentWrapper>
    );

    const addItemElementMap = () => (
        <PaddedComponentWrapper size="small">
            <div onClick={() => { /*change_action_mode(item, "ADD");*/ }}>
                <DashboardTagCard callback={() => { change_action_mode({}, "ADD"); }} directory={"assets"} img_path={"add.png"} is_highlighted={false} />
            </div>
        </PaddedComponentWrapper>
    );


    const modalHeader = (item) => { return item.team_name + ": " + item.title; };
    const modalPriorityDisplay = (item) => { return Array.from({ length: item.priority_number + 1 }, (_, i) => '*').join(''); }

    const actionItemContent = (item) => (
        <div className="container full-height card-editor frame-wrap">
            {item.sender_name}:
            <br />
            {actionItem && actionItem.action === "EDIT" ?
                <textarea className="modal-textarea" name="notemessagetext" value={editedMessage} onChange={e => setEditedMessage(e.target.value)} disabled={isUpdating || !item.user_permissions.can_edit}></textarea>
                :
                item.message
            }

            {/* Team member list */}
            <TeamUserProfilesSimpleSubsection teamName={item.team_name} />
        </div>
    );

    const actionItemButtons = (actionItem) => (
        <>
            {actionItem.action === "ADD" &&
                <button className="button-small remove-icon" onClick={() => { change_action_mode(actionItem.item, ""); }} disabled={isUpdating}>Cancel</button>
            }
            {actionItem.action === "EDIT" && 
            <>
                <button className="button-small remove-icon" onClick={() => { change_action_mode(actionItem.item, ""); }} disabled={isUpdating}>Cancel</button>
                <button className="button-small remove-icon" onClick={() => { change_action_mode(actionItem.item, "REMOVE"); }} disabled={isUpdating || !actionItem.item.user_permissions.can_remove}>Remove</button>
                <button className="button-small remove-icon" onClick={() => { change_action_mode(actionItem.item, "COMPLETE"); }} disabled={isUpdating || !actionItem.item.user_permissions.can_complete}>Complete</button>
                <button className="button-small remove-icon" onClick={() => { edit_item(actionItem.item); }} disabled={isUpdating || !actionItem.item.user_permissions.can_edit}>Save</button>
            </>
            }
            {actionItem.action === "REMOVE" &&
            <>
                <button className="button-small remove-icon" onClick={() => { change_action_mode(actionItem.item, ""); }} disabled={isUpdating}>Cancel</button>
                <button className="button-small remove-icon" onClick={() => { remove_item(actionItem.item); }} disabled={isUpdating || !actionItem.item.user_permissions.can_remove}>Confirm Remove</button>
            </>
            }
            {actionItem.action === "COMPLETE" &&
            <>
                <button className="button-small remove-icon" onClick={() => { change_action_mode(actionItem.item, ""); }} disabled={isUpdating}>Cancel</button>
                <button className="button-small remove-icon" onClick={() => { complete_item(actionItem.item); }} disabled={isUpdating || !actionItem.item.user_permissions.can_complete}>Confirm Complete</button>
            </>
            }
            {actionItem.action == "VIEW" &&
                <button className="button-small remove-icon" onClick={() => { change_action_mode(actionItem.item, ""); }} disabled={isUpdating}>Exit</button>
            }
        </>
    );


    useEffect(() => {
        if (data_obj.selectableDataArray) {
            let newArr = data_obj.selectableDataArray.map((item) => ({...item, actionMode: ""}));
            //const comp_func = (a, b) => { return (data_obj.dataIdentifier(a) < data_obj.dataIdentifier(b)) ? -1 : 1; };
            setIdentifiableSelectData(__MakeSortableListData(newArr, data_obj.dataIdentifier)/*.sort(comp_func)*/);
        }
        else {
            setIdentifiableSelectData(__MakeSortableListData([], null));
        }

    }, [data_obj]);

    return (
        <div className={`container ${isVerticalDisplay ? "full-height" : "full-width"}`}>
            {actionItem && actionItem.action !== "ADD" && <DashboardModalBase title={modalHeader(actionItem.item)} content={() => actionItemContent(actionItem.item)} priority={modalPriorityDisplay(actionItem.item)} actionButtons={() => actionItemButtons(actionItem)} />}
            {actionItem && actionItem.action === "ADD" && <DashboardModalConfigureNote title="New Note" actionButtons={() => actionItemButtons(actionItem)} teamNoteConfig={data_obj.teamNoteConfig} configurationType="Add" configureItem={add_item} />}
            <div className={isVerticalDisplay ? "selectable-menu container full-width" : "selectable-menu-horizontal"}>
                <div className={isVerticalDisplay ? "dashboard-note-section scroll-control-y" : "dashboard-note-section-horizontal scroll-control-x"}>
                    <DashboardItemMenuHeader title={title} className={`${isVerticalDisplay ? "menu-header-square" : "menu-header-square"} sticky`}>
                        {!data_obj.isDataPending &&
                            <div className="card-editor">
                                <div className="card-button">
                                    {data_obj.selectableDataArray && (
                                            <div>
                                                <button className={`button-small`} onClick={data_obj.refreshData}>Refresh</button>
                                            </div>
                                        )
                                    }
                                </div>
                            </div>
                        }
                    </DashboardItemMenuHeader>

                    <DashboardItemMenuSelectArea data={identifiableSelectData} setData={setIdentifiableSelectData} isDataPending={data_obj.isDataPending} itemElement={selectableItemElementMap} addItemElement={userIdentity ? addItemElementMap : undefined} isVerticalDisplay={isVerticalDisplay} />
                </div>
            </div>
        </div>
    );
}

export default DashboardNoteArea;
