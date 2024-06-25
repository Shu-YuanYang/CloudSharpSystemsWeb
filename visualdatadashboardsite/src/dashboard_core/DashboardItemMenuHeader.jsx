const DashboardItemMenuHeader = (props) => {

   
    return (
        <div className={props.className ? props.className : "menu-header sticky"}>
            <span>{props.title}</span>
            { props.children }
        </div>
    )
};

export default DashboardItemMenuHeader;
