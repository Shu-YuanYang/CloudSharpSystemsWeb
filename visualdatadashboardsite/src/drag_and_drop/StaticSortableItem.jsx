import { DragHandle } from "./DragHandle";
import styled from "styled-components";



/*
const StyledStaticTableRow = styled.div`
  box-shadow: rgb(0 0 0 / 10%) 0px 20px 25px -5px,
    rgb(0 0 0 / 30%) 0px 10px 10px -5px;
  outline: #3e1eb3 solid 1px;
  border: solid;
  border-width: 1px;
  border-color: gray;
`;*/

export const StaticSortableItem = (props) => {
    return (
        <div /*{...row.getRowProps()}*/>
            <div className="static-drag-box" /*{...row.cells[0].getCellProps()}*/>
                <DragHandle isdragging={1} />
                {props.children}
                {/*<span>{row.host_name}</span>
                <br />
                <span>{row.server_host_IP}</span>*/}
            </div>
        </div>
        
    );
};