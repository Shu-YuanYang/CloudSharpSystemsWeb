import React from "react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { DragHandle } from "./DragHandle";
import styled from "styled-components";


export const DraggableSortableItem = (props) => {
    const {
        attributes,
        listeners,
        transform,
        transition,
        setNodeRef,
        isDragging
    } = useSortable({
        id: props.item_id
    });
    const style = {
        transform: CSS.Transform.toString(transform),
        transition: transition
    };
    return (
        <div ref={setNodeRef} style={style} /*{...row.getRowProps()}*/>
            {isDragging ? (
                <div className="sortable-box sink" colSpan={1}>
                    {props.children}
                </div>
            ) : (
                <div className="sortable-box" /*{...row.cells[0].getCellProps()}*/>
                    {props.is_editing && <DragHandle {...attributes} {...listeners} />}
                    {props.children}
                    {/*<span>{row.host_name}</span>
                    <br />
                    <span>{row.server_host_IP}</span>*/}
                </div>
            )}
        </div>
        
    );
};
