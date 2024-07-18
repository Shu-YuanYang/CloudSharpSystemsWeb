import { useState, useEffect } from "react";
import DragScrollable from "../drag_and_drop/DragScrollable";
import logo from "../assets/unnamed_icon.png";

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
                    <div key={index} className="container full-height" style={{ paddingBottom: "10px" }}>
                        <div className="" style={{ display: "flex", alignItems: "center", borderStyle: "solid", borderWidth: "1px 0 0 0", borderColor: "gray" }}>
                            <div className="column c05" style={{ textAlign: "right" }}>
                                <img src={logo} alt="" className="icon-img-extra-small"></img>
                            </div>
                            <div className="column c80">
                                <div style={{ marginBottom: "2px" }}>
                                    <span className="subsection title">{item.title}</span>
                                    <span>  </span>
                                    <span className="button-custom">{item.source}</span>
                                </div>
                                {!item.subLists && (
                                    <div>
                                        <span>Source Link: {item.link}</span>
                                        <br />
                                    </div>
                                )}

                                {item.subLists &&
                                    <div className="card-editor frame-wrap">
                                        <DragScrollable className="scroll-control-x">
                                            <div className="nav-item no-pad">
                                                {item.keyTerms.map((term, index) => <div key={index} className="button-medium nav-item-account">{term}</div> )}
                                            </div>
                                        </DragScrollable>
                                    </div>
                                }
                            </div>
                            {
                                item.subLists && <div className="column c15">
                                    <button onClick={() => { setIsExpanded(index, !isExpandedLst[index]); }}>{isExpandedLst[index] ? "collapse" : item.expansionAction.text}</button>
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
