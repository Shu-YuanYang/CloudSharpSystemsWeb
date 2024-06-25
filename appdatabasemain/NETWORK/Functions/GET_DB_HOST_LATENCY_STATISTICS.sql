﻿CREATE FUNCTION [NETWORK].[GET_DB_HOST_LATENCY_STATISTICS](@TimeOffSetHours INT, @TimeIntervalMinutes INT, @HOST_IP VARCHAR(100))
RETURNS TABLE AS RETURN
	WITH 
	-- define intervals
	ListIntervals(INTERVAL) AS (
		SELECT DATEADD(HOUR, -@TimeOffSetHours, GETDATE()) AS INTERVAL
		UNION ALL
		SELECT DATEADD(MINUTE, @TimeIntervalMinutes, INTERVAL)
		FROM ListIntervals 
		WHERE INTERVAL < GETDATE()
	),
	-- define time slots: intervals expressed as <START_TIME, END_TIME>
	TimeSlots AS (
		SELECT INTERVAL AS START_TIME, DATEADD(MINUTE, @TimeIntervalMinutes, INTERVAL) AS END_TIME FROM ListIntervals
	),
	-- get log records
	LOGS AS (
		SELECT * FROM NETWORK.TB_HOST_STATUS_LOG with (nolock)
		WHERE HOST_IP = @HOST_IP AND EDIT_TIME >= DATEADD(HOUR, -@TimeOffSetHours, GETDATE())
	)
	-- categorise log records into timeslots: take average, median, and max of latencies
	SELECT END_TIME, MIN(L.LATENCY) AS MIN_LATENCY, AVG(L.LATENCY) AS AVG_LATENCY, MAX(L.LATENCY) AS MAX_LATENCY
	FROM LOGS L INNER JOIN TimeSlots T ON L.EDIT_TIME BETWEEN T.START_TIME AND T.END_TIME 
	GROUP BY END_TIME
	--ORDER BY END_TIME
	;
