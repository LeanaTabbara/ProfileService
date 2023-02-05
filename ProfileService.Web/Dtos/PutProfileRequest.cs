namespace ProfileService.Web.Dtos
{
    public class PutProfileRequest
    {
        public PutProfileRequest (string firstName, string lastName){
        FirstName  = firstName;
        LastName = lastName;
    }

      public string FirstName { get; init; }
        public string LastName { get; init; }
    }
}