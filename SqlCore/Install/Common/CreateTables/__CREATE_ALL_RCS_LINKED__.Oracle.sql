create table all_rcs_linked (InstanceId varchar2(64), SubscriptionId int not null, PayerId int not null, PayeeId int not null, StartDate date, EndDate date, ActionType nvarchar2(255), Currency nvarchar2(3), ProratedDailyRate number(22,10), DailyRate number(22,10), Rate number(22,10), ProductOfferingId int, PriceableItemTemplateId int, PriceableItemInstanceId int, SubscriptionStartDate date, SubscriptionEndDate date, MRR number(22,10), OldRate number(22,10), OldDailyRate number(22,10), OldProratedDailyRate number(22,10), OldSubscriptionStartDate date, OldSubscriptionEndDate date);
