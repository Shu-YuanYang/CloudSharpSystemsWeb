

/*
    State machine to build a query list structure:

    Initialize:
        let queryListBuilder = new QueryListBuilder();

    Build Query (Simple):
        queryListBuilder.add_new_field_query("VISIT_COUNT", ">", 5);
        queryListBuilder.add_new_field_query("FIRST_NAME", "<", "E");
        queryListBuilder.add_new_field_query("PURCHASED_ITEMS", "Contains", "bag");
        ...

    Build Query (Two-Level):
        queryListBuilder.reset_query_list();
        queryListBuilder.add_new_sub_query("OR");
        queryListBuilder.enter_sub_query(0);
        queryListBuilder.add_new_field_query("VISIT_COUNT", ">", 5);
        queryListBuilder.add_new_field_query("VISIT_COUNT", "<", 15);
        queryListBuilder.exit_sub_query();

        queryListBuilder.add_new_sub_query("OR");
        //queryListBuilder.enter_sub_query(1);
        //queryListBuilder.exit_sub_query();

        queryListBuilder.add_new_sub_query("OR");
        queryListBuilder.enter_sub_query(2);
        queryListBuilder.add_new_field_query("FIRST_NAME", "<", "E");
        queryListBuilder.add_new_field_query("LAST_NAME", "<", "G");
        queryListBuilder.exit_sub_query();

        queryListBuilder.add_new_sub_query("OR");
        queryListBuilder.enter_sub_query(3);
        queryListBuilder.add_new_field_query("PURCHASED_ITEMS", "Contains", "bag");
        queryListBuilder.add_new_field_query("TOTAL_SPENT", ">", 300);
        queryListBuilder.exit_sub_query();

*/
export class QueryListBuilder {

    constructor(logic_operator = QueryListBuilder.LOGIC_AND) {
        this.reset_query_list(logic_operator);
    }

    make_new_query_lst(logic_operator, level=0) {
        return {
            level: level,
            logic_operator: logic_operator
        }
    }

    reset_query_list(logic_operator = QueryListBuilder.LOGIC_AND) {
        this.query_list = this.make_new_query_lst(logic_operator);
        this.__current_level_query_list = this.query_list;
        this.__parent_level_query_lists = [];
    }

    add_new_field_query(field_name, comparison_operator, compared_value) {
        if (this.__current_level_query_list.field_queries === null || this.__current_level_query_list.field_queries === undefined) this.__current_level_query_list.field_queries = [];
        this.__current_level_query_list.field_queries.push({ field_name: field_name, compared_value_raw: String(compared_value), comparison_operator: comparison_operator });
    }

    pop_field_query() {
        if (this.__current_level_query_list.field_queries === null || this.__current_level_query_list.field_queries === undefined)
            throw new Error(`There is no field query to pop at level ${this.__current_level_query_list.level}.`);
        const popped_field_query = this.__current_level_query_list.field_queries.pop();
        if (this.__current_level_query_list.field_queries.length === 0) this.__current_level_query_list.field_queries = null;
        return popped_field_query;
    }

    add_new_sub_query(logic_operator) {
        if (this.__current_level_query_list.sub_query_lists === null || this.__current_level_query_list.sub_query_lists === undefined) this.__current_level_query_list.sub_query_lists = [];
        const new_query_lst = this.make_new_query_lst(logic_operator, this.__current_level_query_list.level + 1);
        this.__current_level_query_list.sub_query_lists.push(new_query_lst);
    }

    enter_sub_query(index) {
        if (this.__current_level_query_list.sub_query_lists === null || this.__current_level_query_list.sub_query_lists === undefined)
            throw new Error(`There is no sub query list at level ${this.__current_level_query_list.level}.`);
        this.__parent_level_query_lists.push(this.__current_level_query_list);
        this.__current_level_query_list = this.__current_level_query_list.sub_query_lists[index];
    }

    exit_sub_query() {
        if (this.__current_level_query_list.level === 0)
            throw new Error("Current query list is at top level. It cannot exit.");
        this.__current_level_query_list = this.__parent_level_query_lists.pop();
    }


    /*********************************************************/
    // For user updates and interactions:
    shareQueryListData() {
        //const sharingBuilder = new QueryListBuilder();
        //sharingBuilder.query_list = this.query_list;
        //sharingBuilder.__current_level_query_list = this.__current_level_query_list;
        //sharingBuilder.__parent_level_query_lists = this.__parent_level_query_lists;
        this.__clean_up_query_list();
        return {
            level: this.query_list.level,
            logic_operator: this.query_list.logic_operator,
            field_queries: this.query_list.field_queries,
            sub_query_lists: this.query_list.sub_query_lists
        };
    }

    __clean_up_query_list(query_list = null) {
        let q_lst = query_list;
        if (!query_list) q_lst = this.query_list;
        if (q_lst.field_queries && q_lst.field_queries.length === 0) delete q_lst.field_queries;
        if (q_lst.sub_query_lists) {
            if (q_lst.sub_query_lists.length === 0) delete q_lst.sub_query_lists;
            else for (let i = 0; i < q_lst.sub_query_lists.length; ++i) this.__clean_up_query_list(q_lst.sub_query_lists[i]);
        }
    }


    static update_field_name(field_query_obj, field_name) {
        field_query_obj.field_name = field_name;
    }

    static update_field_comparison_operator(field_query_obj, comparison_operator) {
        field_query_obj.comparison_operator = comparison_operator;
    }

    static update_field_compared_value(field_query_obj, compared_value) {
        field_query_obj.compared_value_raw = String(compared_value);
    }


    static update_field_query(field_query_obj, field_name, comparison_operator, compared_value) {
        if (field_name === null || field_name === undefined) return;
        if (comparison_operator === null || comparison_operator === undefined) return;
        if (compared_value === null || compared_value === undefined) return;
        QueryListBuilder.update_field_name(field_query_obj, field_name);
        QueryListBuilder.update_field_comparison_operator(field_query_obj, comparison_operator);
        QueryListBuilder.update_field_compared_value(field_query_obj, compared_value);
        //field_query_obj.field_name = field_name;
        //field_query_obj.comparison_operator = comparison_operator;
        //field_query_obj.compared_value_raw = String(compared_value);
    }


    static remove_field_query(query_list, index) {
        query_list.field_queries.splice(index, 1);
        if (query_list.field_queries.length === 0) delete query_list.field_queries;
    }

}

QueryListBuilder.LOGIC_AND = "AND";
QueryListBuilder.LOGIC_OR = "OR";



export class QueryFormation {

    constructor(copy_from_formation) {
        if (copy_from_formation) this.shallow_copy(copy_from_formation);
        else this.reset_formation();
    }

    reset_formation() {
        // each entry is of structure of either:
        // { entry_type: QueryFormation.ENTRY_FIELD_QUERY, index: int (position in QueryListBuilder.query_list.field_queries), logic_operator: string  }, or
        // { entry_type: QueryFormation.ENTRY_SUB_QUERIES, index: int (position in QueryListBuilder.query_list.sub_query_list), logic_operator: string, sub_query_formation: QueryFormation }
        this.__field_entry_count = 0;
        this.query_metadata_list = [];
    }

    add_entry(logic_operator) {
        // base case: empty formation, assume the first entry will be a field query: 
        if (this.query_metadata_list.length === 0) {
            this.query_metadata_list.push({ entry_type: QueryFormation.ENTRY_FIELD_QUERY, index: 0, logic_operator: logic_operator });
            ++(this.__field_entry_count);
            return;
        }

        
        // Additive case: new incoming operation placements are determined by the previous metadata item entry type:
        const last_metadata_item = this.query_metadata_list[this.query_metadata_list.length - 1];
        // Case 1: Same logic operation
        if (logic_operator === last_metadata_item.logic_operator) { 
            this.query_metadata_list.push({ entry_type: QueryFormation.ENTRY_FIELD_QUERY, index: this.__field_entry_count, logic_operator: logic_operator });
            ++(this.__field_entry_count);
            return;
        }

        // Case 2: Differing logic operations:
        /*
            AND
            AND
            AND
            input OR

            -->
            AND
            AND
            [AND: [ OR, OR ]]
        */
        if (last_metadata_item.entry_type === QueryFormation.ENTRY_FIELD_QUERY) { // If the last item is not a subquery list record, transform the item into a sub query list record with a sub query formation.
            last_metadata_item.entry_type = QueryFormation.ENTRY_SUB_QUERIES;
            last_metadata_item.index = this.query_metadata_list.length - this.__field_entry_count;
            // last_metadata_item.logic_operator; // logic operator on the same QueryFormation level does not change
            last_metadata_item.sub_query_formation = new QueryFormation();
            --(this.__field_entry_count);

            // reallocate the original query entry in the new sub query formation:
            last_metadata_item.sub_query_formation.add_entry(logic_operator); // 
        }
        last_metadata_item.sub_query_formation.add_entry(logic_operator); // allocate incoming query entry

    }

    remove_entry(index) {
        if (this.query_metadata_list[index].entry_type !== QueryFormation.ENTRY_FIELD_QUERY)
            throw new Error(`The record to remove must be of the entry type: ${QueryFormation.ENTRY_FIELD_QUERY}`);
        this.query_metadata_list.splice(index, 1);
        --(this.__field_entry_count);
        for (let i = 0; i < this.query_metadata_list.length; ++i)
            if (this.query_metadata_list[i].entry_type === QueryFormation.ENTRY_FIELD_QUERY)
                --(this.query_metadata_list[i].index);  // decrease index of all field query records
    }


    shallow_copy(copy_from_formation) {
        this.__field_entry_count = copy_from_formation.__field_entry_count;
        this.query_metadata_list = copy_from_formation.query_metadata_list;
    }
}

QueryFormation.ENTRY_FIELD_QUERY = "field_query";
QueryFormation.ENTRY_SUB_QUERIES = "sub_query_lists";






export class TwoLevelQueryFormation extends QueryFormation {

    constructor(copy_from_formation = null)
    {
        //this.query_list_builder = query_list_builder;
        super(copy_from_formation);        
    }


    reset_formation() {
        // each entry is of structure of either:
        // { entry_type: QueryFormation.ENTRY_FIELD_QUERY, index: int (position in QueryListBuilder.query_list.field_queries), logic_operator: string  }, or
        // { entry_type: QueryFormation.ENTRY_SUB_QUERIES, index: int (position in QueryListBuilder.query_list.sub_query_list), logic_operator: string, sub_query_formation: QueryFormation }
        // *** Use indices and identifiers to update instead of array pointers/references:
        // this.level = 0;
        // this.__query_list = this.query_list_builder.find_query_list_by_level(this.level);
        super.reset_formation();
        this.query_list_builder = new QueryListBuilder();
    }

    add_entry(logic_operator, field_name, comparison_operator) {

        // Same as initial QueryListBuilder logic operator:
        if (logic_operator === this.query_list_builder.query_list.logic_operator) {
            if (this.query_list_builder.__parent_level_query_lists.length > 0) this.query_list_builder.exit_sub_query();
            //this.query_list_builder.add_new_field_query(field_name, comparison_operator, "");
            //super.add_entry(logic_operator);
            //return;
        }

        // Different logic operator:
        else if (this.query_metadata_list[this.query_metadata_list.length - 1].entry_type === QueryFormation.ENTRY_FIELD_QUERY) {
            const field_query = this.query_list_builder.pop_field_query();
            this.query_list_builder.add_new_sub_query(logic_operator);
            this.query_list_builder.enter_sub_query(this.query_list_builder.query_list.sub_query_lists.length - 1);
            this.query_list_builder.add_new_field_query(field_query.field_name, field_query.comparison_operator, field_query.compared_value_raw);
        }
        // Different logic operator but has previously exited the sub query level:
        else if (this.query_list_builder.__parent_level_query_lists.length === 0) this.query_list_builder.enter_sub_query(this.query_list_builder.query_list.sub_query_lists.length - 1);
        this.query_list_builder.add_new_field_query(field_name, comparison_operator, "");
        super.add_entry(logic_operator);
        

    }

    shallow_copy(copy_from_formation) {
        super.shallow_copy(copy_from_formation);
        this.query_list_builder = copy_from_formation.query_list_builder;
    }


}