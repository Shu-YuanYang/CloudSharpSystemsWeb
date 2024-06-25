// Code copied from: https://codesandbox.io/p/sandbox/react-table-drag-and-drop-sort-rows-with-dnd-kit-btpy9?

import { useMemo, useState, useEffect } from "react";
import {
    closestCenter,
    DndContext,
    DragOverlay,
    KeyboardSensor,
    MouseSensor,
    TouchSensor,
    useSensor,
    useSensors
} from "@dnd-kit/core";
//import { restrictToVerticalAxis } from "@dnd-kit/modifiers";
import {
    arrayMove,
    SortableContext,
    verticalListSortingStrategy,
    rectSortingStrategy
} from "@dnd-kit/sortable";
//import { __MakeSortableListData } from "./drag_and_drop_helper";
import { DraggableSortableItem } from "./DraggableSortableItem";
import { StaticSortableItem } from "./StaticSortableItem";





export function SortableList({ dataArray, setDataArray, itemElement, addItemElement, isInEditMode, isVerticalDisplay = true }) {
    const [activeId, setActiveId] = useState();
    //const [identifiableData, setIdentifiableData] = useState([]);
    const items = useMemo(() => { return dataArray?.map((r) => r.__id) }, [dataArray]);
    
    const sensors = useSensors(
        useSensor(MouseSensor, {}),
        useSensor(TouchSensor, {}),
        useSensor(KeyboardSensor, {})
    );

    function handleDragStart(event) {
        setActiveId(event.active.id);
    }

    function handleDragEnd(event) {
        const { active, over } = event;
        if (active.id !== over.id) {
            setDataArray((data_arr) => {
                const oldIndex = items.indexOf(active.id);
                const newIndex = items.indexOf(over.id);
                const result_arr = arrayMove(data_arr, oldIndex, newIndex);
                //console.log(result_arr);
                return result_arr;
            });
            //console.log(items);
            //console.log("-----------------------------------------------------")
        }

        setActiveId(null);
    }

    function handleDragCancel() {
        setActiveId(null);
    }

    const selectedItem = useMemo(() => {
        if (!activeId) {
            return null;
        }
        const item = dataArray.find((r) => r.__id === activeId);
        return item;
    }, [activeId, dataArray]);

    // Render the UI for your table
    return (
        <div>
            
            <DndContext
                sensors={sensors}
                onDragEnd={handleDragEnd}
                onDragStart={handleDragStart}
                onDragCancel={handleDragCancel}
                collisionDetection={closestCenter}
                //modifiers={[restrictToVerticalAxis]}
            >
                <SortableContext items={items} strategy={rectSortingStrategy}>
                    {
                        dataArray.map((r) => {
                        //prepareRow(row);
                            return (
                                <div className={isVerticalDisplay ? "" : "card-inline"} key={r.__id}>
                                    <DraggableSortableItem item_id={r.__id} is_editing={isInEditMode}>
                                        {itemElement({ item: r })}
                                    </DraggableSortableItem>
                                </div>
                            );
                    })}
                </SortableContext>

                <DragOverlay>
                    {activeId && (
                        <StaticSortableItem>
                            {itemElement({ item: selectedItem })}
                        </StaticSortableItem>
                    )}
                </DragOverlay>

            </DndContext>

            {addItemElement &&
                <div className={isVerticalDisplay ? "" : "card-inline"}>
                    {addItemElement()}
                </div>
            }
        </div>
        
    );
}
