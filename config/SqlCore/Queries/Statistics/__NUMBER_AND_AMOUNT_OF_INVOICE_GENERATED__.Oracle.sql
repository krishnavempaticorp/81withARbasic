
			select count(*)  "NUMOFINV",
				  invoice_currency  "CURRENCY" ,
				  SUM(COALESCE(invoice_amount, 0.0)) - sum(COALESCE(tax_ttl_amt,0.0)) "TOTALAMT",
				  sum(COALESCE(tax_ttl_amt,0.0)) "TOTALTAX" 
			  from t_invoice inv 
				inner join t_av_internal av on inv.id_payer=av.id_acc
				left outer join t_description des on av.c_invoicemethod=des.id_desc
				where id_payer_interval= %%ID_INTERVAL%%
				  and (id_lang_code=%%ID_LANG_CODE%% 
				       or id_lang_code is null)
				group by invoice_currency