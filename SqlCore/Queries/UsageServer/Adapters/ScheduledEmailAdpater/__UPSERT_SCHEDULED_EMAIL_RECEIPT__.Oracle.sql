begin
UPDATE t_sch_email_adapter_status 
SET 
email_status= :EMAIL_STATUS, 
id_last_run= :ID_LAST_RUN, 
retry_counter = retry_counter + :RETRY_COUNTER, 
tx_detail = :TX_DETAIL
WHERE ((id_entity_guid = hextoraw(REPLACE(:ID_ENTITY_GUID, '-' ))) OR (id_entity_guid IS NULL)) 
AND ((id_entity_int = :ID_ENTITY_INT) OR (id_entity_int IS NULL))
AND id_sch_email_entity_mapping = :MAPPING_ID
AND id_event = :ID_EVENT;

-- Check how many rows updated above. If zero rows updated, then insert one row
if (SQL%ROWCOUNT = 0) then
  INSERT INTO t_sch_email_adapter_status  (id_sch_email_adapter_status,email_status,id_last_run,retry_counter, tx_detail, id_entity_guid, id_entity_int,id_sch_email_entity_mapping, id_event) 
  VALUES (seq_t_sch_email_adapter_status.nextval, :EMAIL_STATUS , :ID_LAST_RUN, :RETRY_COUNTER, :TX_DETAIL, hextoraw(REPLACE(:ID_ENTITY_GUID, '-' )), :ID_ENTITY_INT, :MAPPING_ID, :ID_EVENT);
end if;

commit;
end;
