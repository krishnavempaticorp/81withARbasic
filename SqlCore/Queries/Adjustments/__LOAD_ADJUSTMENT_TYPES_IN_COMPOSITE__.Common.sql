
     SELECT
     /* __LOAD_ADJUSTMENT_TYPES_IN_COMPOSITE__ */
		ajt.id_prop	TypeID,	
		ajt.tx_guid TypeGUID, 
		ajt.id_pi_type TypePIType, 
		ajt.n_adjustmentType TypeUOM, 
		ajt. b_SupportBulk SupportsBulk,
		base1.nm_name	TypeName,	
		base1.nm_desc TypeDescription,
		base1.n_desc TypeDescriptionId,
		base1.nm_display_name TypeDisplayName,
		base1.n_display_name TypeDisplayNameId,
		ajf.tx_formula TypeFormula, 
		ajf.id_engine TypeFormulaEngine,
		ajtp.id_prop TypePropID, 
		ajtp.nm_datatype	TypePropDataType, 
		ajtp.n_direction TypePropDirection,	
		ajtp.id_adjustment_type	TypePropAdjustmentTypeID,
		base2.nm_name	TypePropName,	
		base2.nm_desc TypePropDescription, 
		/* TODO: Needs to add using localization for PROPS */
		base2.n_desc TypePropDescriptionId, 
		base2.nm_display_name TypePropDisplayName,
		/* TODO: Needs to add using localization for PROPS */
		base2.n_display_name TypePropDisplayNameId,
		ajt.id_formula TypeFormulaID, 
		ajt.tx_default_desc AdjustmentDefaultDescription,
		ajt.n_composite_adjustment IsAdjustmentComposite,
		pv.nm_table_name ProductViewTableName,
		pit.id_parent PARENTID, base3.nm_name PIName
      FROM t_adjustment_type ajt
      INNER JOIN t_pi pit on ajt.id_pi_type = pit.id_pi
      INNER JOIN t_prod_view pv on %%%UPPER%%%(pit.nm_productview) = %%%UPPER%%%(pv.nm_name)
      INNER JOIN t_calc_formula ajf ON ajt.id_formula = ajf.id_formula
      LEFT OUTER JOIN t_base_props base3 ON pit.id_pi = base3.id_prop 
      LEFT OUTER JOIN	t_adjustment_type_prop ajtp	ON ajt.id_prop = ajtp.id_adjustment_type
      LEFT OUTER JOIN	t_base_props base1 ON	ajt.id_prop	=	base1.id_prop      
      LEFT OUTER JOIN	t_base_props base2 ON	ajtp.id_prop = base2.id_prop      
      WHERE	ajt.id_prop in ( Select id_adjustment_type from t_composite_adjustment where id_prop = %%ID_PARENTID%%) 
      /* don't return rebill adjustment types */
      AND ajt.n_adjustmentType <> 4
      ORDER BY ajt.id_prop
			