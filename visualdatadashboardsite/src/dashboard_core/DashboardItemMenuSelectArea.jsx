import { SortableList } from "../drag_and_drop/SortableList";


const DashboardItemMenuSelectArea = ({ data, setData, isDataPending, itemElement, addItemElement, isVerticalDisplay = true }) => {
    
    return (
        <div>
            {isDataPending ?
                (<div><span>Loading...</span></div >) :
                (<SortableList dataArray={data} setDataArray={setData} itemElement={itemElement} addItemElement={addItemElement} isInEditMode={false} isVerticalDisplay={isVerticalDisplay} />)
            }
        </div>
        
    );
};

export default DashboardItemMenuSelectArea;