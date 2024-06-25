import { useState } from 'react';
import { DndContext } from '@dnd-kit/core';
import { Draggable } from './drag_and_drop/Draggable';
import { Droppable } from './drag_and_drop/Droppable';
import './Example.css'


function Example() {
    const [parent, setParent] = useState(null);
    const draggable = (
        <Draggable id="draggable">
            Go ahead, drag me.
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

export default Example