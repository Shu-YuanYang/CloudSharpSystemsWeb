import { NavLink } from "react-router-dom";
import DashboardProfileNavItem from "../../dashboard_core/DashboardProfileNavItem";


const PageComponentWrapper = (props) => {

    return (<>
        <div className="card-editor">
            <div className="card-nav-item">
                <NavLink to="/menu">
                    <div className="nav-item">Go to Menu</div>
                </NavLink>
            </div>
        </div>
        <DashboardProfileNavItem />
        {props.children}
    </>);

};

export default PageComponentWrapper;