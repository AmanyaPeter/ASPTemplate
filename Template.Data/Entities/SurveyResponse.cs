namespace ASPTemplate.Template.Data.Entities
{
public class SurveyResponse
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid? IdeaId { get; set; }
		public string SurveyType { get; set; }
		public string ResponseData { get; set; }
		public DateTime SubmittedDate { get; set; }
	}
}