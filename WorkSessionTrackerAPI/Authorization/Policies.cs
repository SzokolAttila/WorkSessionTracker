namespace WorkSessionTrackerAPI.Authorization
{
    public static class Policies
    {
        public const string StudentOnly = "StudentOnly";
        public const string CompanyOnly = "CompanyOnly";
        public const string CanAccessStudentData = "CanAccessStudentData";
        public const string IsWorkSessionOwner = "IsWorkSessionOwner";
        public const string CanVerifyWorkSession = "CanVerifyWorkSession";
        public const string CanCommentOnWorkSession = "CanCommentOnWorkSession";
        public const string CanViewComment = "CanViewComment";
        public const string IsCommentOwner = "IsCommentOwner";
    }
}
