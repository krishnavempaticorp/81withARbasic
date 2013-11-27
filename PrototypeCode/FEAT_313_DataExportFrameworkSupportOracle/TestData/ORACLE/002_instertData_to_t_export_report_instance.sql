whenever sqlerror exit 2;

DECLARE
BEGIN
	
INSERT ALL
   INTO t_export_report_instance
           (c_rep_instance_desc
           ,id_rep
           ,c_report_online
           ,dt_activate
           ,dt_deactivate
           ,c_rep_output_type
           ,c_xmlConfig_loc
           ,c_rep_distrib_type
           ,c_report_destn
           ,c_destn_direct
           ,c_access_user
           ,c_access_pwd
           ,c_exec_type
           ,c_eop_step_instance_name
           ,c_generate_control_file
           ,c_control_file_delivery_locati
           ,c_ds_id
           ,c_compressreport
           ,c_compressthreshold
           ,c_output_execute_params_info
           ,c_use_quoted_identifiers
           ,dt_last_run
           ,dt_next_run
           ,c_output_file_name)
     VALUES
           ('DiscSchedNoParCSV1'
           ,(select id_rep from t_export_reports WHERE c_report_title='Report1-DiscSchedNoParCSV' and ROWNUM = 1 )
           ,0
           ,'19-Sep-12'
           ,'19-Sep-32'
           ,'CSV'
           ,'\DataExport\Config\fieldDef'
           ,'Disk'
           ,''
           ,0
           ,''
           ,''
           ,'Scheduled'
           ,'NA'
           ,0
           ,''
           ,1
           ,0
           ,0
           ,0
           ,0
           ,'19-Sep-12'
           ,'20-Sep-12'
           ,'DiscSchedNoParCSV1')
           
  INTO t_export_report_instance
           (c_rep_instance_desc
           ,id_rep
           ,c_report_online
           ,dt_activate
           ,dt_deactivate
           ,c_rep_output_type
           ,c_xmlConfig_loc
           ,c_rep_distrib_type
           ,c_report_destn
           ,c_destn_direct
           ,c_access_user
           ,c_access_pwd
           ,c_exec_type
           ,c_eop_step_instance_name
           ,c_generate_control_file
           ,c_control_file_delivery_locati
           ,c_ds_id
           ,c_compressreport
           ,c_compressthreshold
           ,c_output_execute_params_info
           ,c_use_quoted_identifiers
           ,dt_last_run
           ,dt_next_run
           ,c_output_file_name)
     VALUES
           ('DiscSchedNoParXML1'
           ,(select id_rep from t_export_reports WHERE c_report_title='Report2-DiscSchedNoParXML' and ROWNUM = 1 )
           ,0
           ,'19-Sep-12'
           ,'19-Sep-32'
           ,'XML'
           ,'\DataExport\Config\fieldDef'
           ,'Disk'
           ,''
           ,0
           ,''
           ,''
           ,'Scheduled'
           ,'NA'
           ,0
           ,''
           ,1
           ,0
           ,0
           ,0
           ,0
           ,'19-Sep-12'
           ,'20-Sep-12'
           ,'DiscSchedNoParXML1')         
 
	INTO t_export_report_instance
           (c_rep_instance_desc
           ,id_rep
           ,c_report_online
           ,dt_activate
           ,dt_deactivate
           ,c_rep_output_type
           ,c_xmlConfig_loc
           ,c_rep_distrib_type
           ,c_report_destn
           ,c_destn_direct
           ,c_access_user
           ,c_access_pwd
           ,c_exec_type
           ,c_eop_step_instance_name
           ,c_generate_control_file
           ,c_control_file_delivery_locati
           ,c_ds_id
           ,c_compressreport
           ,c_compressthreshold
           ,c_output_execute_params_info
           ,c_use_quoted_identifiers
           ,dt_last_run
           ,dt_next_run
           ,c_output_file_name)
     VALUES
           ('testSchWithParams'
           ,(select id_rep from t_export_reports WHERE c_report_title='Report3-EOPWithParams' and ROWNUM = 1 )
           ,0
           ,'07-Oct-12'
           ,'19-Oct-32'
           ,'CSV'
           ,'\DataExport\Config\fieldDef'
           ,'Disk'
           ,'D:\reports'
           ,0
           ,''
           ,''
           ,'EOP'
           ,'QueueEOPExportReports'
           ,0
           ,''
           ,1
           ,1
           ,0
           ,1
           ,1
           ,'06-Oct-12'
           ,'07-Oct-12'
           ,'testSchWithParams')
 
SELECT * FROM dual;
COMMIT;           
END;
 /