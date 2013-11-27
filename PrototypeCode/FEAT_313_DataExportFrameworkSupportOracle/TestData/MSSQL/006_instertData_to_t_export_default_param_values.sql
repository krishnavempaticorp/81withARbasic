INSERT INTO t_export_default_param_values
           (id_rep_instance_id
           ,id_param_name
           ,c_param_value)
     VALUES
           ((SELECT TOP(1) id_rep_instance_id 
				FROM t_export_report_instance ir
				JOIN t_export_reports r
				ON (ir.id_rep = r.id_rep)
				WHERE r.c_report_title = 'Report3-EOPWithParams') 
           ,2
           ,'2012-09-17 00:00:00.000') 
           
INSERT INTO t_export_default_param_values
           (id_rep_instance_id
           ,id_param_name
           ,c_param_value)
     VALUES
           ((SELECT TOP(1) id_rep_instance_id 
				FROM t_export_report_instance ir
				JOIN t_export_reports r
				ON (ir.id_rep = r.id_rep)
				WHERE r.c_report_title = 'Report3-EOPWithParams') 
           ,1
           ,'demo')