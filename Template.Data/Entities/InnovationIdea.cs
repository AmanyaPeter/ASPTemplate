namespace Template.Data.Entities
{	
public class InnovationIdea
	{
		public Guid Id { get; set; }
		public string ReferenceNumber { get; set; }
		public string SubmissionType { get; set; }
		public DateTime SubmissionDate { get; set; }
		public string SubmitterId { get; set; }
		public string TeamMembers { get; set; }
		public string Title { get; set; }
		public string SummaryDescription { get; set; }
		public string ProblemStatement { get; set; }
		public string ProposedSolution { get; set; }
		public string InnovationTypes { get; set; }
		public int? CategoryId { get; set; }
		public string KeyEnablersRequired { get; set; }
		public string ImplementationApproach { get; set; }
		public string ExpectedImpactIndicators { get; set; }
		public string ExpectedBenefits { get; set; }
		public string StrategicAlignment { get; set; }
		public string OtherStrategicAlignment { get; set; }
		public string InnovationPriorityArea { get; set; }
		public string ExpectedImplementationTimeline { get; set; }
		public string EstimatedBudgetRange { get; set; }
 
		// Demographic snapshot at time of submission — kept separate from the live
		// values on User so historical reports stay accurate even if the submitter
		// later changes department, station, job title, etc.
		public int? SubmitterBusinessUnitId { get; set; }
		public int? SubmitterStationId { get; set; }
		public int? SubmitterJobTitleRankId { get; set; }
		public string SubmitterAgeBracket { get; set; }
		public string SubmitterGender { get; set; }
 
		public string CurrentStage { get; set; }
		public string CurrentStatus { get; set; }
		public string? AssignedReviewerId { get; set; }
		public DateTime? ReviewStartDate { get; set; }
		public DateTime? ReviewEndDate { get; set; }
		public DateTime? StageDeadlineDate { get; set; }
		public int? DaysRemaining { get; set; }
		public bool IsOverdue { get; set; }
		public DateTime? DecisionDate { get; set; }
		public string DecisionReason { get; set; }
		public string? DecisionMadeBy { get; set; }
		public bool IsLocked { get; set; }
		public DateTime? LockedAt { get; set; }
		public string? LockedBy { get; set; }
		public bool IsRetracted { get; set; }
		public DateTime? RetractedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public string? UpdatedBy { get; set; }
		public byte[] RowVersion { get; set; }
	}}