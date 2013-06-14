﻿using MetraTech.ExpressionEngine.Flows.Enumerations;

namespace MetraTech.ExpressionEngine.Flows
{
    public class MtsqlStep : FlowStepBase
    {
        #region Properties
        public string Script { get; set; }
        #endregion

        #region Constructor
        public MtsqlStep(Flow flow) : base(flow, FlowStepType.Mtsql)
        {
        }
        #endregion

        #region Methods
        public override void UpdateInputsAndOutputs(Context context)
        {
            InputsAndOutputs.Clear();
        }
        #endregion
    }
}
