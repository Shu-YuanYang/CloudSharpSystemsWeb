import { createPortal } from "react-dom";
import DashboardItemMenuHeader from "./DashboardItemMenuHeader";
import DashboardItemMenuFooter from "./DashboardItemMenuFooter";
import ModalWrapper from "../auxiliary/wrappers/ModalWrapper";



const DashboardModalBase = (props) => {

    return (
        <>
            {createPortal(
                <ModalWrapper>
                    <div className="container modal modal-team-note">
                        <DashboardItemMenuHeader className="menu-header large sticky" title={props.title}></DashboardItemMenuHeader>
                        <div className="container message-box">
                            {props.content()}
                        </div>
                        <DashboardItemMenuFooter title={props.foot_note ? props.foot_note : "Actions"}>
                            {props.actionButtons()}
                        </DashboardItemMenuFooter>
                    </div>
                </ModalWrapper>
                , document.getElementById('root'))
            }
        </>
    );
};

export default DashboardModalBase;

