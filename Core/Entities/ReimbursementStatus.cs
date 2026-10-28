namespace ChurchPortal.Core.Entities
{
    /// <summary>
    /// Reimbursement workflow states. Integer values are stored in the database,
    /// so existing values keep their numbers:
    ///   0 Draft                 - member is still preparing the request
    ///   1 UnderReview           - submitted; waiting for an Accounts Officer (was "Pending")
    ///   2 LegacyAccountsReview  - old "minister approved" stage; no longer assigned.
    ///                             Existing rows are moved to UnderReview at startup.
    ///   3 Approved              - final approval by Accounts Admin; not yet paid
    ///   4 Paid                  - payment confirmed by Accounts Admin (terminal)
    ///   5 Rejected              - rejected with a reason (terminal)
    ///   6 ReturnedForCorrection - sent back to the member (was "InfoRequested")
    ///   7 PendingApproval       - reviewed by an Accounts Officer; waiting for Accounts Admin
    /// </summary>
    public enum ReimbursementStatus
    {
        Draft = 0,
        UnderReview = 1,
        LegacyAccountsReview = 2,
        Approved = 3,
        Paid = 4,
        Rejected = 5,
        ReturnedForCorrection = 6,
        PendingApproval = 7
    }

    /// <summary>
    /// Whether a reimbursement is charged to a church department or is a personal-use
    /// claim that belongs to no department budget.
    /// </summary>
    public enum ReimbursementClassification
    {
        Department = 0,
        PersonalUse = 1
    }

    /// <summary>
    /// What a line item is. Physical items are checked by the Inventory Officer and
    /// added to inventory; services (labour, rentals, subscriptions, food) are not.
    /// Service is 0 so rows created before this field existed are never sent for checking.
    /// </summary>
    public enum ReimbursementItemType
    {
        Service = 0,
        Physical = 1
    }

    /// <summary>Inventory Officer's check of a physical line item.</summary>
    public enum InventoryVerificationStatus
    {
        /// <summary>Service item, or a draft that has not been submitted yet.</summary>
        NotRequired = 0,
        /// <summary>Submitted; waiting for the Inventory Officer.</summary>
        Pending = 1,
        /// <summary>Seen and confirmed; an inventory record was created.</summary>
        Verified = 2,
        /// <summary>Inventory Officer could not confirm it. Blocks the Accounts Officer.</summary>
        Rejected = 3
    }
}
