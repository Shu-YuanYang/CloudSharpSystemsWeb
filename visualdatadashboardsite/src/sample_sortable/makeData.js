//import namor from "namor";

const range = (len) => {
    const arr = [];
    for (let i = 0; i < len; i++) {
        arr.push(i);
    }
    return arr;
};

const newPerson = (id) => {
    const statusChance = Math.random();
    return {
        id: `row-${id}`,
        firstName: id, //namor.generate({ words: 1, numbers: 0 }),
        lastName: Math.floor(Math.random() * 100), //namor.generate({ words: 1, numbers: 0 }),
        age: Math.floor(Math.random() * 30),
        visits: Math.floor(Math.random() * 100),
        progress: Math.floor(Math.random() * 100),
        status:
            statusChance > 0.66
                ? "relationship"
                : statusChance > 0.33
                    ? "complicated"
                    : "single"
    };
};

export function makeData(...lens) {
    
    const makeDataLevel = (depth = 0) => {
        const len = lens[depth];
        return range(len).map((d) => {
            return {
                ...newPerson(d),
                subRows: lens[depth + 1] ? makeDataLevel(depth + 1) : undefined
            };
        });
    };

    return makeDataLevel();
    
}


export function makeSortablePageData() {
    return [
        {
            DISPLAY_NAME: "AWS Central",
            ROUTE: "https://us-east-1.console.aws.amazon.com/billing/home?region=us-east-2#/bills", 
            ROUTE_TYPE: "HTTP_URL",
            RANKING: 1,
            ICON: "aws",
            is_selected: true
        },
        {
            DISPLAY_NAME: "CloudSharp Test",
            ROUTE: "http://3.22.167.51/",
            ROUTE_TYPE: "HTTP_URL",
            RANKING: 2,
            ICON: "react",
            is_selected: true
        },
        {
            DISPLAY_NAME: "Gmail",
            ROUTE: "https://mail.google.com/mail/u/0/#inbox",
            ROUTE_TYPE: "HTTP_URL",
            RANKING: 3,
            ICON: "gmail",
            is_selected: true
        },
        {
            DISPLAY_NAME: "UH Outlook",
            ROUTE: "https://outlook.office.com/mail/",
            ROUTE_TYPE: "HTTP_URL",
            RANKING: 4,
            ICON: "outlook",
            is_selected: true
        },
        {
            DISPLAY_NAME: "Genshin Checkin",
            ROUTE: "https://act.hoyolab.com/ys/event/signin-sea-v3/index.html?act_id=e202102251931481&mhy_auth_required=true&mhy_presentation_style=fullscreen&utm_source=ingame&utm_medium=notice&utm_campaign=pcm&lang=en&device_type=pc&ext=%7b%22loc%22%3a%7b%22x%22%3a-3050.432373046875%2c%22y%22%3a246.51016235351563%2c%22z%22%3a-4413.8642578125%7d%2c%22platform%22%3a%22WinST%22%7d&game_version=OSRELWin2.6.0_R6708157_S6988297_D6731353&plat_type=pc",
            ROUTE_TYPE: "HTTP_URL",
            RANKING: 5,
            ICON: "react",
            is_selected: true
        },
        {
            DISPLAY_NAME: "Example Dashboard",
            ROUTE: "/example",
            ROUTE_TYPE: "ROUTE",
            RANKING: -1,
            ICON: "react",
            is_selected: false
        },
        {
            DISPLAY_NAME: "Canvas Email",
            ROUTE: "https://canvas.uh.edu/conversations#filter=type=inbox",
            ROUTE_TYPE: "HTTP_URL",
            RANKING: -1,
            ICON: "canvas",
            is_selected: false
        },
        {
            DISPLAY_NAME: "Star Rail Checkin",
            ROUTE: "https://act.hoyolab.com/bbs/event/signin/hkrpg/index.html?act_id=e202303301540311",
            ROUTE_TYPE: "HTTP_URL",
            RANKING: -1,
            ICON: "react",
            is_selected: false
        }
    ];
}




export function makeSortableChartData() {
    return [
        {
            DISPLAY_NAME: "DB Latencies",
            ROUTE: "DBLatencyMonitorChart",
            ROUTE_TYPE: "ASSET",
            RANKING: 1,
            ICON: "azure",
            is_selected: true
        },
        {
            DISPLAY_NAME: "Site Memory",
            ROUTE: "SiteMemoryUsageMonitorChart",
            ROUTE_TYPE: "HTTP_URL",
            RANKING: 2,
            ICON: "react",
            is_selected: true
        }
    ];
}