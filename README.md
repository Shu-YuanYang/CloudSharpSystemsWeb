# CloudSharpSystemsWeb

1. Project ID CloudSharpSystemsWeb is a Controller based code to expose API for Cloud based applications. <br />		
&nbsp; Notable settings in appsettings.json include <br />
&nbsp; - Database connection strings <br />
&nbsp; - Site ID mappings for specified Controllers <br />
&nbsp;&nbsp; "SiteConfig": { <br />
&nbsp;&nbsp;&nbsp; ControllerClassName1: SiteID1, <br />
&nbsp;&nbsp;&nbsp; ControllerClassName2: SiteID2, <br />
&nbsp;&nbsp;&nbsp; ControllerClassName3: SiteID3 <br />
&nbsp;&nbsp; } <br />

2. Project ID CloudSharpLimitedCentral is a central server code with a reverse proxy middleware implementation. <br />
&nbsp; Notable settings in appsettings.json include <br />
&nbsp; - Database connection strings <br />
&nbsp; - Site ID mappings for specified Controllers <br />
&nbsp;&nbsp; "SiteConfig": { <br />
&nbsp;&nbsp;&nbsp; ControllerClassName1: SiteID1, <br />
&nbsp;&nbsp;&nbsp; ControllerClassName2: SiteID2, <br />
&nbsp;&nbsp;&nbsp; ControllerClassName3: SiteID3 <br />
&nbsp;&nbsp; } <br />
&nbsp;&nbsp; "ReverseProxyConfig": { <br />
&nbsp;&nbsp;&nbsp; ForwardedPaths: [ <br />
&nbsp;&nbsp;&nbsp;&nbsp; RequestPathAllowedToBeForwardedToApplicationServers1, <br />
&nbsp;&nbsp;&nbsp;&nbsp; RequestPathAllowedToBeForwardedToApplicationServers2 <br />
&nbsp;&nbsp;&nbsp; ] <br />
&nbsp;&nbsp; } <br />

3. Project ID IPMonitoringTask is a monitoring code with recurring probe request sending behaviours to monitor website statuses. <br />
&nbsp; Notable settings in appsettings.json include <br />
&nbsp; - Database connection strings <br />
&nbsp; - Poll Interval: frequency in seconds to send probe requests <br />
&nbsp;&nbsp; "SiteConfig": [ <br />
&nbsp;&nbsp;&nbsp; WebsiteToMonitorName1, WebsiteToMonitorName2 <br />
&nbsp;&nbsp; ] <br />
&nbsp; - Monitor Route: request path from which monitoring status is determined <br />
