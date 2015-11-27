CREATE FUNCTION dbo.GetDirectionFromRout(@RoutID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
	 select   Top 1 @ret = RoutStops.Stop_ID from Routs join RoutStops on Routs.RoutId = RoutStops.Rout_RoutId where RoutId = @RoutID order by Stop_ID desc;
    RETURN @ret;
END;
GO

CREATE PROCEDURE dbo.GetStopDirections
    @StopID int,
	@Count int
AS 

   
select Distinct Top (@Count)  * from Stops join (
							select  dbo.GetDirectionFromRout(RoutId) as xx from RoutStops  join Routs on RoutStops.Rout_RoutId = Routs.RoutId where RoutStops.Stop_ID = @StopID														
							group by RoutId
						) as iidStop 
on Stops.ID =  iidStop.xx

;

GO