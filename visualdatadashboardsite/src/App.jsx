import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import PageComponentWrapper from "./auxiliary/wrappers/PageComponentWrapper";
import PersonalDashboardPage from "./pages/PersonalDashboardPage";
//import TestDynamicComponentPage from "./pages/TestDynamicComponentPage";
import DataDetailPage from "./pages/DataDetailPage";
//import TestDynamicParamsPage from "./pages/TestDynamicParamsPage";
import MainMenuPage from "./pages/MainMenuPage";
import ChartDisplayPage from "./pages/ChartDisplayPage";
import OAuth2LoginSuccessPage from "./pages/OAuth2LoginSuccessPage";
import ExternalURLProectionPage from "./pages/ExternalURLProectionPage";
//import CSEducatorPostsTestDataPage from "./pages/CSEducatorPostsTestDataPage";
import IdentityContext from "./auxiliary/wrappers/IdentityContext";
import useIdentityState from "./endpoints/local_asset_load/useIdentityState";


function App() {

    const { userIdentity } = useIdentityState();

    return (
        <div className="container full-height">
            <IdentityContext.Provider value={userIdentity}>

                <Router>
                    <Routes>
                        <Route exact path="/" element={<PageComponentWrapper><PersonalDashboardPage /></PageComponentWrapper>} />
                        <Route exact path="/menu" element={<MainMenuPage />} />
                        <Route exact path="/datadetail/:componentName/API/:dataSourceAPIEndpoint/:dataSourceAPIName" element={<PageComponentWrapper><DataDetailPage title="DEFAULT TITLE" /></PageComponentWrapper>} />
                        <Route exact path="/:chart" element={<PageComponentWrapper><ChartDisplayPage title="DEFAULT TITLE" /></PageComponentWrapper>} />
                        <Route exact path="/auth/login_success" element={<OAuth2LoginSuccessPage />} />
                        {
                            /*
                            <Route exact path="/testcseducator" element={<CSEducatorPostsTestDataPage />} />
                            <Route exact path="/example" element=
                            {
                                <div>
                                    <NavLink to="/">Products</NavLink>
                                    <Example />
                                </div>
                            } />
                            */
                        }
                        <Route exact path="/external_site" element={<ExternalURLProectionPage />} />
                    </Routes>
                </Router>

            </IdentityContext.Provider>
        </div>
    );
}

export default App;
