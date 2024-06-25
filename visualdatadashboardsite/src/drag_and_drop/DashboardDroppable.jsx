import React, { useState } from 'react';
import reactLogo from '../assets/react.svg'
import { DndContext } from '@dnd-kit/core';
import { Draggable } from './Draggable';
import { Droppable } from './Droppable';
//import './Example.css'


function DashboardDroppable() {
    const [parent, setParent] = useState(null);
    

    const draggable = (
        <Draggable id="draggable">
            <div>
                <img src={reactLogo} className="logo react" alt="React logo" />
                <span>title</span>
            </div>
        </Draggable>
    );

    return (
        <DndContext onDragEnd={handleDragEnd}>
            {!parent ? draggable : null}
            <Droppable id="droppable">
                {parent === "droppable" ? draggable : 'Drop here'}

                <div className="cardboard">

                </div>
            </Droppable>
        </DndContext>
    );

    function handleDragEnd({ over }) {
        setParent(over ? over.id : null);
    }
}

export default DashboardDroppable