
				  select 
				    dbo.GenGuid() "ID", /* dummy filed as identifier for GridLayout*/
                    COALESCE(partition_name, N'Non-Partitioned') "PARTITION",
				    nm_name "PI Template",
				    count(c_advance) "# of Advanced RCs Generated",
				    c_prorateddays "# of Days Prorated",
				    trunc(c_rcintervalend - c_rcintervalstart) + 1 "# of Days in Period",
				    am_currency "Currency",
				    SUM(nvl(au.Amount, 0.0)) "Amount",
				    SUM(nvl(c_rcamount, 0.0)) "Amount if it weren't Prorated"
				  from t_acc_usage au
				  inner join t_pv_flatrecurringcharge rc 
				    on au.id_sess = rc.id_sess
				    and au.id_usage_interval = rc.id_usage_interval				
				  inner join t_vw_base_props bp on au.id_pi_template = bp.id_prop
                  left outer join vw_bus_partition_accounts bpt on bpt.id_acc = au.id_acc
				  where c_advance = 1
				    and amount >= 0.0
				    and au.id_usage_interval=%%ID_INTERVAL%%
				    and id_lang_code=%%ID_LANG_CODE%%
				  group by partition_name,nm_name, am_currency, c_prorateddays,
				          trunc(c_rcintervalend - c_rcintervalstart) + 1
				UNION ALL
				  select 
						dbo.GenGuid() "ID", /* dummy filed as identifier for GridLayout*/
                        COALESCE(partition_name, N'Non-Partitioned') "PARTITION",
						nm_name "PI Template",
				    count(c_advance) "# of Advanced RCs Generated",
				    c_prorateddays "# of Days Prorated",
				    trunc(c_rcintervalend - c_rcintervalstart) + 1 "# of Days in Period",
				    am_currency "Currency", 
				    SUM(nvl(au.Amount, 0.0)) "Amount",
				    SUM(nvl(c_rcamount, 0.0)) "Amount if it weren't Prorated"
				  from t_acc_usage au
				  inner join t_pv_udrecurringcharge rc 
				    on au.id_sess=rc.id_sess
				    and au.id_usage_interval = rc.id_usage_interval				
				  inner join t_vw_base_props bp 
				    on au.id_pi_template=bp.id_prop
                    left outer join vw_bus_partition_accounts bpt on bpt.id_acc = au.id_acc
				  where c_advance = 1
				    and amount >= 0.0
				    and au.id_usage_interval=%%ID_INTERVAL%%
				    and id_lang_code=%%ID_LANG_CODE%%
				  group by partition_name,nm_name, am_currency, c_prorateddays,
                  trunc(c_rcintervalend - c_rcintervalstart) + 1
