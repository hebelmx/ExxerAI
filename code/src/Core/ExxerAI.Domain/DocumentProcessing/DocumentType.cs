namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents the types of documents that can be processed
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Unknown document type
    /// </summary>
    Unknown,
    
    /// <summary>
    /// IMSS payment receipt
    /// </summary>
    IMSSPayment,
    
    /// <summary>
    /// General invoice
    /// </summary>
    Invoice,
    
    /// <summary>
    /// Tax document
    /// </summary>
    TaxDocument,
    
    /// <summary>
    /// Insurance payment receipt
    /// </summary>
    InsurancePayment,
    
    /// <summary>
    /// Purchase order
    /// </summary>
    PurchaseOrder,
    
    /// <summary>
    /// Bank statement
    /// </summary>
    BankStatement,
    
    /// <summary>
    /// Contract document
    /// </summary>
    Contract,
    
    /// <summary>
    /// Financial report
    /// </summary>
    FinancialReport,
    
    /// <summary>
    /// Other business document
    /// </summary>
    Other
}