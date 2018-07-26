SELECT Count(*) as [count_Rows] 
, AVG(DATEDIFF(MILLISECOND,Sending_date, Created_date)) AS [avg_Millisecond]  
, MAX(DATEDIFF(MILLISECOND,Sending_date, Created_date)) AS [max_Millisecond]  
, MIN(DATEDIFF(MILLISECOND,Sending_date, Created_date)) AS [min_Millisecond]  
FROM message; 
