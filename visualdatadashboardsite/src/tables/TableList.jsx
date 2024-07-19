import { useState, useEffect } from "react";
import DragScrollable from "../drag_and_drop/DragScrollable";
import { LOCAL_STORE } from "../endpoints/local_asset_load/local_storage";

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


    const navigateToItem = (item) => {
        localStorage.setItem(LOCAL_STORE.REQUESTED_EXTERNAL_LINK, JSON.stringify({ info: item.title, url: item.url }));
        //window.location.href = "/external_site";
        let external_site_protection_page = `${window.origin}/external_site`;
        window.open(external_site_protection_page, "ExternalSiteProtection");
    }

    const comp = (
        <div className={`table large ${hasBorder ? "table-sub-entry-block" : "table-entry-block"}`}>
            {
                contentList &&
                contentList.map((item, index) => (
                    <div key={index} className="container full-height column-header-filter">
                        <div className="table-list">
                            <div className="column c05 subsection title-small">
                                <span>{index + 1} </span>
                                <img src={item.imgUrl} alt="" className="icon-img-extra-small"></img>
                            </div>
                            <div className="column c80">
                                <div style={{ marginBottom: "2px" }}>
                                    <span className="subsection title">{item.title}</span>
                                    <span> from </span>
                                    <span className="button-custom">{item.source}</span>
                                </div>
                                {!item.subList && (
                                    <div>
                                        <span style={{ whiteSpace: "nowrap", overflow: "clip" }}>Source Link: </span>
                                        <a style={{ whiteSpace: "nowrap", overflow: "clip" }} onClick={(e) => { e.preventDefault(); navigateToItem(item); }} href={""}>{item.url}</a>
                                        {/*<Link style={{ whiteSpace: "nowrap", overflow: "clip" }} target="_blank" to={item.url}>{item.url}</Link>*/}
                                        <br />
                                    </div>
                                )}

                                {item.subList &&
                                    <div className="card-editor frame-wrap">
                                        <DragScrollable className="scroll-control-x">
                                            <div className="nav-item no-pad">
                                                {"Search: "}
                                                {item.keyTerms.map((term, index) => <div key={index} className="button-medium nav-item-account">{term}</div> )}
                                            </div>
                                        </DragScrollable>
                                    </div>
                                }
                            </div>
                            {
                                item.subList && <div className="column c15">
                                    <button onClick={() => { setIsExpanded(index, !isExpandedLst[index]); }}>{isExpandedLst[index] ? "collapse" : item.expansionAction.text}</button>
                                </div>
                            }
                        </div>
                        {item.subList && isExpandedLst[index] &&
                            <div className={"sub-query-fields c95"} style={{ marginLeft: "2%" }}>
                                <TableList contentList={item.subList} refreshDisplay={refreshDisplay} hasBorder={true} />
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
