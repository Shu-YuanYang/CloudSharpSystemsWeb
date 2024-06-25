//import { useState, useEffect } from "react";
import { QueryFormation, QueryListBuilder } from "../endpoints/api/query_helper";
import TableQueryEntry from "./TableQueryEntry";


const TableQueryEntryBlock = ({ queryFormation, queryList, fieldOperationOptions, refreshDisplay, hasBorder = false }) => {



    const remove_entry = (index) => {
        const field_query_index = queryFormation.query_metadata_list[index].index;
        QueryListBuilder.remove_field_query(queryList, field_query_index);
        console.log(queryList);
        queryFormation.remove_entry(index);
        console.log(queryFormation);
        refreshDisplay();
    }

    const comp = (
        <div className={hasBorder ? "sub-query-entry-block" : "query-entry-block"}>
            {
                queryFormation && queryList &&
                queryFormation.query_metadata_list.map((item, index) => (
                    <div key={index} style={hasBorder ? { display: "inline-block" } : {}}>
                        {index > 0 && <span className="query-logic-operator">{item.logic_operator}</span>}
                        {
                            item.entry_type === QueryFormation.ENTRY_FIELD_QUERY ? // else QueryFormation.ENTRY_SUB_QUERIES
                                (
                                    <div className={"sub-query-fields"}>
                                        <TableQueryEntry fieldQuery={queryList.field_queries[item.index]} fieldOperationOptions={fieldOperationOptions} removeFunction={() => { remove_entry(index); }} />
                                    </div>
                                ) :
                                (
                                    <div className={"sub-query-fields"}>
                                        <TableQueryEntryBlock queryFormation={item.sub_query_formation} queryList={queryList.sub_query_lists[item.index]} fieldOperationOptions={fieldOperationOptions} refreshDisplay={refreshDisplay} hasBorder={true} />
                                    </div>
                                )
                        }
                    </div>
                ))
            }
        </div>
    );

    const displayed_comp =
        (queryList && ( (queryList.field_queries && queryList.field_queries.length > 0) || (queryList.sub_query_lists && queryList.sub_query_lists.length > 0))) ?
            comp : (<></>);


    return displayed_comp;

};

export default TableQueryEntryBlock;

