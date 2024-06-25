import logo from "../assets/unnamed_icon.png";



const DashboardProfileButton = (props) => {

    return (
        <div className="button-medium button-custom" onClick={props.disabled ? () => {} : props.onClick}>
            {
                props.authInfo ?
                    <div className="column">{props.children}</div>
                    :
                    (<>
                        <div className="column">
                            {!props.disabled && <img src={logo} alt="" className={"icon-img-extra-small"}></img>}
                        </div>
                        <div className="column">
                            {props.children}
                        </div>
                    </>)
            }            
        </div>
    );
};


export default DashboardProfileButton;