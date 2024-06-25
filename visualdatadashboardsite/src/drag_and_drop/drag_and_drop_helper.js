export function __MakeSortableListData(data, dataIdentifier) {
    return data?.map((r, index) => ({ ...r, __id: `item-${dataIdentifier ? dataIdentifier(r) : index }` }));
}

export function __RemoveItemFromSortableListData(data, item_to_remove, dataIdentifier) {
    //console.log(item_to_remove.__id);
    return data.filter((item) => {
        //console.log(item.__id);
        return item.__id !== item_to_remove.__id
    });
}

export function __AddItemToSortableListData(data, item_to_add, dataIdentifier) {
    let new_arr = [...data, item_to_add];
    if (!dataIdentifier) for (let i = 0; i < new_arr.length; ++i) new_arr[i].__id = `item-${i}`;
    return new_arr;
}