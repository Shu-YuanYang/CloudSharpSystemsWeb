import { useState, useEffect } from "react";

const TableList = ({ contentList, refreshDisplay, hasBorder = false }) => {

    const [isExpandedLst, setIsExpandedLst] = useState([]);

    useEffect(() => {
        if (!contentList) return;
        let isExpandedInit = Array.from({ length: contentList.length }, (_, i) => false);
        setIsExpandedLst(isExpandedInit);
    }, [contentList]);

    const setIsExpanded = (index, isExpanded) => {
        setIsExpandedLst(old_lst => {
            let new_lst = [...old_lst];
            new_lst[index] = isExpanded;
            return new_lst;
        });
    };


    const comp = (
        <div className={`table large ${hasBorder ? "table-sub-entry-block" : "table-entry-block"}`}>
            {
                contentList &&
                contentList.map((item, index) => (
                    <div key={index} className="container full-height">
                        <div style={{ borderStyle: "solid", borderWidth: "1px 0 0 0", borderColor: "gray" }}>
                            <div className="column c85">
                                <span>{item.title}</span>
                                <span>  </span>
                                <span>{item.source}</span>
                                <br />
                                <span>{item.link}</span>
                            </div>
                            {
                                item.subLists && <div className="column c15">
                                    <button onClick={() => { setIsExpanded(index, !isExpandedLst[index]); }}>{item.expansion_action.text}</button>
                                </div>
                            }
                        </div>
                        {item.subLists && isExpandedLst[index] &&
                            <div className={"sub-query-fields c95"}>
                                <TableList contentList={item.subLists} refreshDisplay={refreshDisplay} hasBorder={true} />
                            </div>
                        }
                    </div>
                ))
            }
        </div>
    );

    const displayed_comp = (contentList && comp);


    return displayed_comp;

};

export default TableList;
