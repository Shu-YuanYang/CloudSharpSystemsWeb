import logo from "../assets/unnamed_icon.png";


const DashboardProfileNavItemUserInfo = (props) => {


    return (
        <div className="button-medium nav-item-account">
            <div className="column">
                <img src={(props.authInfo && props.authInfo.PROFILE_PICTURE) ? props.authInfo.PROFILE_PICTURE : logo} alt="" className={"icon-img-extra-small"}></img>
            </div>
            <div className="column">
                {props.authInfo.NAME_ALIAS}
            </div>
        </div>
    );
};


export default DashboardProfileNavItemUserInfo;