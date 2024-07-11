import logo from "../assets/unnamed_icon.png";

const UserProfileSimple = ({ username, name, profilePicture }) => {

  
  return (
      <div className="button-large nav-item-account">
          <div className="container align-center column">
              <img src={profilePicture ? profilePicture : logo} alt="" className={"icon-img-extra-small"}></img>
          </div>
          <div className="column nav-item-profile-width">
              {name}
              <br />
              {username}
          </div>
      </div>
  );
}

export default UserProfileSimple;