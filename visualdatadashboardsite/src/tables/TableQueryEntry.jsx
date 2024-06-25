import { useState, useEffect, useMemo, useCallback } from "react";
import { QueryListBuilder } from "../endpoints/api/query_helper";
import { TableHelper } from "../auxiliary/objects/table_helper";

const TableQueryEntry = ({ fieldQuery, fieldOperationOptions = [], removeFunction }) => {

    const [fieldName, setFieldName] = useState(fieldQuery.field_name);
    const [comparisonOperator, setComparisonOperator] = useState(fieldQuery.comparison_operator);
    const [comparedValue, setComparedValue] = useState(fieldQuery.compared_value_raw);

    const [currentFieldIndex, setCurrentFieldIndex] = useState(0);


    const field_name_options = useMemo(() => fieldOperationOptions.map(opt => ({ value: opt.field_name, label: TableHelper.format_key(opt.field_name) })), [fieldOperationOptions]);


    // Ensure selection of valid comparison operator based on the selected field name:
    const adjust_comparison_operator = useCallback(() => {
        const comp_index = fieldOperationOptions.findIndex(fieldOption => fieldOption.field_name === fieldName && fieldOption.comparison_operators.includes(comparisonOperator));
        if (comp_index === -1) {
            const field_index = fieldOperationOptions.findIndex(fieldOption => fieldOption.field_name === fieldName);
            const default_operator = fieldOperationOptions[field_index].comparison_operators[0];
            setComparisonOperator(default_operator);
            QueryListBuilder.update_field_comparison_operator(fieldQuery, default_operator);
        }
    }, [fieldOperationOptions, fieldName, comparisonOperator, fieldQuery]);


    // Field name selection value on change: 
    useEffect(() => {
        if (fieldOperationOptions && fieldOperationOptions.length > 0 && fieldName === "") {
            console.log("Heya!");
            setCurrentFieldIndex(0);
            setFieldName(fieldOperationOptions[0].field_name);
        }

        const index = fieldOperationOptions.findIndex(fieldOption => fieldOption.field_name === fieldName);
        setCurrentFieldIndex(index);

        adjust_comparison_operator();
        QueryListBuilder.update_field_name(fieldQuery, fieldName);
    }, [fieldOperationOptions, fieldName, fieldQuery, adjust_comparison_operator]);


    // Comparison operator selection value on change:
    useEffect(() => {
        QueryListBuilder.update_field_comparison_operator(fieldQuery, comparisonOperator);
        adjust_comparison_operator();
    }, [fieldOperationOptions, comparisonOperator, fieldQuery, adjust_comparison_operator]);


    // Compared value on change:
    useEffect(() => {
        QueryListBuilder.update_field_compared_value(fieldQuery, comparedValue);
    }, [fieldOperationOptions, comparedValue, fieldQuery]);




    const handleSelectChange = (set_function, e) => {
        set_function(e.target.value);
    };

    


    return (
        <div>
            {
                /*fieldQuery &&*/ (
                    <div style={{ alignContent: "center" }}>
                        <select className="query-field-input select" value={fieldName} onChange={(e) => { handleSelectChange(setFieldName, e); }}>
                            {/*<option disabled value=""> -- select an option -- </option>*/}
                            {field_name_options.map((opt) => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                        </select>
                        <select className="query-field-input select" value={comparisonOperator} onChange={(e) => { handleSelectChange(setComparisonOperator, e); }}>
                            {/*<option disabled value=""> -- select an option -- </option>*/}
                            {fieldOperationOptions[currentFieldIndex].comparison_operators.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
                        </select>
                        <input className="query-field-input" value={comparedValue} onChange={(e) => { handleSelectChange(setComparedValue, e); }}></input>
                        {/*<button className="query-field-input button" onClick={removeFunction}>X</button>*/}
                    </div>
                )
            }
        </div>
    );
};

export default TableQueryEntry;
