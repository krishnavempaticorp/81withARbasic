//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Core.Quoting
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Runtime.Serialization;
  using System.Linq;
  using MetraTech.Basic;
  using MetraTech.BusinessEntity.Core;
  using MetraTech.BusinessEntity.Core.Model;
  using MetraTech.BusinessEntity.DataAccess.Metadata;
  using MetraTech.BusinessEntity.DataAccess.Persistence;
  
  
  [System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
  [System.SerializableAttribute()]
  [System.Runtime.Serialization.KnownTypeAttribute("GetKnownTypes")]
  [MetraTech.BusinessEntity.Core.Model.ConfigDrivenAttribute()]
  public partial class QuoteICB : MetraTech.BusinessEntity.DataAccess.Metadata.DataObject, global::Core.Quoting.Interface.IQuoteICB
  {
    
    public const string FullName = "Core.Quoting.QuoteICB";
    
    private int @__Version;
    
    public const string Property__Version = "_Version";
    
    private System.Nullable<System.DateTime> _creationDate;
    
    public const string Property_CreationDate = "CreationDate";
    
    private System.Nullable<System.DateTime> _updateDate;
    
    public const string Property_UpdateDate = "UpdateDate";
    
    private System.Nullable<System.Int32> _uID;
    
    public const string Property_UID = "UID";
    
    public const string Property_Id = "Id";
    
    private global::Core.Quoting.QuoteICBBusinessKey _quoteICBBusinessKey = new QuoteICBBusinessKey();
    
    private string _currentChargeType;
    
    public const string Property_CurrentChargeType = "CurrentChargeType";
    
    private System.Nullable<System.Int32> _priceableItemId;
    
    public const string Property_PriceableItemId = "PriceableItemId";
    
    private byte[] _chargesRates;
    
    public const string Property_ChargesRates = "ChargesRates";
    
    private global::Core.Quoting.Interface.IPOforQuote _pOforQuote;
    
    private global::MetraTech.BusinessEntity.DataAccess.Metadata.BusinessKey _pOforQuoteBusinessKey;
    
    private System.Nullable<System.Guid> _pOforQuoteId;
    
    private global::Core.Quoting.Interface.IQuoteHeader _quoteHeader;
    
    private global::MetraTech.BusinessEntity.DataAccess.Metadata.BusinessKey _quoteHeaderBusinessKey;
    
    private System.Nullable<System.Guid> _quoteHeaderId;
    
    public const string Relationship_POforQuote_QuoteICBs = "POforQuote_QuoteICBs";
    
    public const string Relationship_QuoteHeader_QuoteICBs = "QuoteHeader_QuoteICBs";
    
    public const string Property_InternalKey = "InternalKey";
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public override int _Version
    {
      get
      {
        return this.@__Version;
      }
      set
      {
        this.@__Version = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public override System.Nullable<System.DateTime> CreationDate
    {
      get
      {
        return this._creationDate;
      }
      set
      {
        this._creationDate = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public override System.Nullable<System.DateTime> UpdateDate
    {
      get
      {
        return this._updateDate;
      }
      set
      {
        this._updateDate = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual System.Nullable<System.Int32> UID
    {
      get
      {
        return this._uID;
      }
      set
      {
        this._uID = value;
      }
    }
    
    [MetraTech.BusinessEntity.DataAccess.Metadata.BusinessKeyAttribute()]
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual global::Core.Quoting.QuoteICBBusinessKey QuoteICBBusinessKey
    {
      get
      {
        return this._quoteICBBusinessKey;
      }
      set
      {
        this._quoteICBBusinessKey = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual string CurrentChargeType
    {
      get
      {
        return this._currentChargeType;
      }
      set
      {
        this._currentChargeType = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual System.Nullable<System.Int32> PriceableItemId
    {
      get
      {
        return this._priceableItemId;
      }
      set
      {
        this._priceableItemId = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual byte[] ChargesRates
    {
      get
      {
        return this._chargesRates;
      }
      set
      {
        this._chargesRates = value;
      }
    }
    
    public virtual global::Core.Quoting.Interface.IPOforQuote POforQuote
    {
      get
      {
        return this._pOforQuote;
      }
      set
      {
        this._pOforQuote = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual global::MetraTech.BusinessEntity.DataAccess.Metadata.BusinessKey POforQuoteBusinessKey
    {
      get
      {
        return this._pOforQuoteBusinessKey;
      }
      set
      {
        this._pOforQuoteBusinessKey = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual System.Nullable<System.Guid> POforQuoteId
    {
      get
      {
        return this._pOforQuoteId;
      }
      set
      {
        this._pOforQuoteId = value;
      }
    }
    
    public virtual global::Core.Quoting.Interface.IQuoteHeader QuoteHeader
    {
      get
      {
        return this._quoteHeader;
      }
      set
      {
        this._quoteHeader = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual global::MetraTech.BusinessEntity.DataAccess.Metadata.BusinessKey QuoteHeaderBusinessKey
    {
      get
      {
        return this._quoteHeaderBusinessKey;
      }
      set
      {
        this._quoteHeaderBusinessKey = value;
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual System.Nullable<System.Guid> QuoteHeaderId
    {
      get
      {
        return this._quoteHeaderId;
      }
      set
      {
        this._quoteHeaderId = value;
      }
    }
    
    public virtual void ClearPOforQuote()
    {
      _pOforQuote = null;
      _pOforQuoteBusinessKey = null;
      _pOforQuoteId = null;
    }
    
    public virtual global::Core.Quoting.Interface.IPOforQuote LoadPOforQuote()
    {
      object item = global::MetraTech.BusinessEntity.DataAccess.Persistence.StandardRepository.Instance.LoadInstanceFor("Core.Quoting.POforQuote", "Core.Quoting.QuoteICB", Id, "POforQuote_QuoteICBs");
      return item == null ? null : (global::Core.Quoting.Interface.IPOforQuote)item;
    }
    
    public virtual void ClearQuoteHeader()
    {
      _quoteHeader = null;
      _quoteHeaderBusinessKey = null;
      _quoteHeaderId = null;
    }
    
    public virtual global::Core.Quoting.Interface.IQuoteHeader LoadQuoteHeader()
    {
      object item = global::MetraTech.BusinessEntity.DataAccess.Persistence.StandardRepository.Instance.LoadInstanceFor("Core.Quoting.QuoteHeader", "Core.Quoting.QuoteICB", Id, "QuoteHeader_QuoteICBs");
      return item == null ? null : (global::Core.Quoting.Interface.IQuoteHeader)item;
    }
    
    public override void SetupRelationships()
    {
      if (POforQuote != null)
      {
POforQuoteBusinessKey = ((global::MetraTech.BusinessEntity.DataAccess.Metadata.DataObject)POforQuote).GetBusinessKey();
POforQuoteId = POforQuote.Id;
_pOforQuote = null;
      }
      if (QuoteHeader != null)
      {
QuoteHeaderBusinessKey = ((global::MetraTech.BusinessEntity.DataAccess.Metadata.DataObject)QuoteHeader).GetBusinessKey();
QuoteHeaderId = QuoteHeader.Id;
_quoteHeader = null;
      }
    }
    
    public virtual object Clone()
    {
      var _quoteICB = new global::Core.Quoting.QuoteICB();
      _quoteICB.QuoteICBBusinessKey = (global::Core.Quoting.QuoteICBBusinessKey)QuoteICBBusinessKey.Clone();
      _quoteICB.CurrentChargeType = CurrentChargeType;
      _quoteICB.PriceableItemId = PriceableItemId;
      _quoteICB.ChargesRates = ChargesRates;
      if (POforQuoteBusinessKey != null)
      {
        _quoteICB.POforQuoteBusinessKey = (MetraTech.BusinessEntity.DataAccess.Metadata.BusinessKey)POforQuoteBusinessKey.Clone();
      }
      if (QuoteHeaderBusinessKey != null)
      {
        _quoteICB.QuoteHeaderBusinessKey = (MetraTech.BusinessEntity.DataAccess.Metadata.BusinessKey)QuoteHeaderBusinessKey.Clone();
      }
      _quoteICB.POforQuoteId = POforQuoteId;
      _quoteICB.QuoteHeaderId = QuoteHeaderId;
      return _quoteICB;
    }
    
    public virtual void Save()
    {
      var item = this;
      global::MetraTech.BusinessEntity.DataAccess.Persistence.StandardRepository.Instance.SaveInstance(ref item);
    }
    
    public override void CopyPropertiesFrom(global::MetraTech.BusinessEntity.DataAccess.Metadata.DataObject dataObject)
    {
      var item = dataObject as global::Core.Quoting.QuoteICB;
      if (item.POforQuote != null)
      {
        POforQuote = item.POforQuote;
      }
      else
      {
        if (item.POforQuote == null && item.POforQuoteId == null)
        {
          POforQuote = null;
        }
      }
      if (item.QuoteHeader != null)
      {
        QuoteHeader = item.QuoteHeader;
      }
      else
      {
        if (item.QuoteHeader == null && item.QuoteHeaderId == null)
        {
          QuoteHeader = null;
        }
      }
      CurrentChargeType = item.CurrentChargeType;
      PriceableItemId = item.PriceableItemId;
      ChargesRates = item.ChargesRates;
      QuoteICBBusinessKey = item.QuoteICBBusinessKey;
    }
    
    public static new System.Type[] GetKnownTypes()
    {
      var knownTypes = new List<System.Type>();
      knownTypes.Add(typeof(global::Core.Quoting.QuoteICBBusinessKey));
      return knownTypes.ToArray();
    }
  }
  
  [System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
  [System.SerializableAttribute()]
  public partial class QuoteICBBusinessKey : MetraTech.BusinessEntity.DataAccess.Metadata.BusinessKey, global::Core.Quoting.Interface.IQuoteICBBusinessKey
  {
    
    private string _entityFullName = "Core.Quoting.QuoteICB";
    
    private System.Guid _internalKey;
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public override string EntityFullName
    {
      get
      {
        return this._entityFullName;
      }
      set
      {
        this._entityFullName = "Core.Quoting.QuoteICB";
      }
    }
    
    [System.Runtime.Serialization.DataMemberAttribute()]
    public virtual System.Guid InternalKey
    {
      get
      {
        return this._internalKey;
      }
      set
      {
        this._internalKey = value;
      }
    }
    
    public override object Clone()
    {
      var _businessKey = new QuoteICBBusinessKey();
      _businessKey.InternalKey = InternalKey;
      return _businessKey;
    }
  }
}
