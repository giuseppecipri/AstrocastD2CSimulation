SELECT Count(*) as [count_Rows] 
, AVG(DATEDIFF(MILLISECOND,Sending_date, Created_date)) AS [avg_Millisecond]  
, MAX(DATEDIFF(MILLISECOND,Sending_date, Created_date)) AS [max_Millisecond]  
, MIN(DATEDIFF(MILLISECOND,Sending_date, Created_date)) AS [min_Millisecond]  
, MAX(Message_id)
FROM message
WHERE message.Message_id > 1; 
