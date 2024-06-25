import { QueryListBuilder, TwoLevelQueryFormation } from "../endpoints/api/query_helper";
import { useState, useEffect, useMemo, useCallback } from "react";
//import TableQueryEntry from "./TableQueryEntry";
import TableQueryEntryBlock from "./TableQueryEntryBlock";


const TableQueryInterface = ({ queryFunction, queryableConfigData, isQueryPending }) => {


    const [queryFormation, setQueryFormation] = useState(new TwoLevelQueryFormation());
    const [queryList, setQueryList] = useState(null);


    

    const field_operation_options = useMemo(() => {
        if (!queryableConfigData || !queryableConfigData.queryable_field_operations) return [];
        return queryableConfigData.queryable_field_operations;
    }, [queryableConfigData]);


    const refresh_display = () => {
        setQueryList(queryFormation.query_list_builder.shareQueryListData());
        setQueryFormation(new TwoLevelQueryFormation(queryFormation));
    };


    const add_entry = (logic_operator) => {
        queryFormation.add_entry(logic_operator, field_operation_options[0].field_name, field_operation_options[0].comparison_operators[0]);
        //console.log(queryFormation);
        refresh_display();
        //console.log(queryList);
    };


    const clear_entries = () => {
        queryFormation.reset_formation();
        //submit_query();
        refresh_display();
        queryFunction(queryFormation.query_list_builder.shareQueryListData());
    };


    const submit_query = () => {
        refresh_display();
        queryFunction(queryList);
        //console.log(queryFormation);
    }

    return (
        <div className="query-interface">
            {
                <TableQueryEntryBlock queryFormation={queryFormation} queryList={queryList} fieldOperationOptions={field_operation_options} refreshDisplay={refresh_display} />
            }
            {
                ( queryList && ((queryList.field_queries && queryList.field_queries.length > 0) || (queryList.sub_query_lists && queryList.sub_query_lists.length > 0)) ) ?
                    (
                        <>
                            {queryableConfigData.logical_operators.map((logic_operator, index) => <button key={index} className="query-interface-input button" onClick={() => { add_entry(logic_operator); }}>{logic_operator}</button>)}
                            {
                                /*
                                <button className="query-interface-input button" onClick={ () => { add_entry(QueryListBuilder.LOGIC_AND); } }>AND</button>
                                <button className="query-interface-input button" onClick={() => { add_entry(QueryListBuilder.LOGIC_OR); }}>OR</button>
                                */
                            }
                            <button className="query-interface-input button" onClick={clear_entries}>Clear</button>
                            <button className="query-interface-input button query-button" onClick={submit_query} disabled={isQueryPending}>Query</button>
                        </>
                    )
                    :
                    (
                        <>
                            <button className="query-interface-input button full-width" onClick={ () => { add_entry(QueryListBuilder.LOGIC_AND); } }>Query by Fields</button>
                        </>
                    )
            }
            
        </div>
    );

};

export default TableQueryInterface;
