const DashboardItemMenuFooter = (props) => {

   
    return (
        <div className={props.className ? props.className : "menu-footer"}>
            <span>{props.title}</span>
            <div className="card-footer-buttons">
                {props.children}
            </div>
        </div>
    )
};

export default DashboardItemMenuFooter;
