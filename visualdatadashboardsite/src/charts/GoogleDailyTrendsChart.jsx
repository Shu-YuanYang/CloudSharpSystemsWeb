import { useState, useEffect, useMemo, useCallback } from "react";
import { Link } from "react-router-dom";
import useFetch from "../endpoints/api/useFetch";
import { api_full_path, get_api } from "../endpoints/api/api_helper";
import { APIEndpoints } from "../site_config.json";
import TableList from "../tables/TableList";


const GoogleDailyTrendsChart = ({ title, refreshTriggered, updateRefreshStatus }) => {

    const { data: trendsData, refreshData: refreshTrendsData, isPending: isTrendsDataPending, error: trendsDataFetchError } = useFetch(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_google_daily_trends").path));

    useEffect(() => {
        refreshTrendsData();
    }, [refreshTriggered, refreshTrendsData]);

    const format_trend = useCallback((search) => {
        return {
            title: ((typeof search.title) === "string"? search.title : search.title.query),
            source: (search.source ? search.source : search.image.source),
            url: (search.url ? search.url : search.image.newsUrl),
            imgUrl: (search.image? search.image.imageUrl : null),
            keyTerms: (search.relatedQueries ? search.relatedQueries.map(query => query.query) : null),
            expansionAction: { text: "Relevant Searches" },
            subList: (search.articles ? search.articles.map(article => format_trend(article)) : null)
        };
    }, []);

    const formattedTrends = useMemo(() => {
        if (!trendsData || !trendsData.trendingSearches) return;
        const trend_searches = trendsData.trendingSearches;
        const formatted_searches = trend_searches.map(format_trend);
        return formatted_searches;
    }, [trendsData, format_trend]);
    
    
    

    return (
        <div className="container full-height">
            <div className="container full-height scroll-control-y">
                <div className="menu-header large">
                    <span>Data Source: </span>
                    <Link target="_blank" to={"https://trends.google.com/trends/trendingsearches/daily"}>https://trends.google.com/trends/trendingsearches/daily</Link>
                    <span>  </span>
                    {!isTrendsDataPending && trendsData &&
                        <span>at ({trendsData.FEED_TIME})</span>
                    }
                </div>
                {!isTrendsDataPending && 
                    <div className="container align-center">
                        <TableList contentList={formattedTrends} refreshDisplay={refreshTrendsData} />
                    </div>
                }
                <div style={{ whiteSpace: "pre-line" }}>{
                    isTrendsDataPending ? "Loading..." : ""
                }</div>
            </div>
            
        </div>
    );
};

export default GoogleDailyTrendsChart;
